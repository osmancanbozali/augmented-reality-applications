using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

public class PointCloudAligner : MonoBehaviour
{
    [SerializeField] private float alignmentThreshold = 0.5f; // Threshold for inlier distance
    [SerializeField] private float earlyStopInlierRatio = 0.8f; // Terminate loop if the algorithm gets 80% inliers
    private const int SampleSize = 3;
    private System.Random random = new System.Random();

    public void RunRANSAC(List<Vector3> points1, List<Vector3> points2, out Vector3 translation, out Matrix4x4 rotation) {
        translation = Vector3.zero;
        rotation = Matrix4x4.identity;

        if (points1.Count < SampleSize || points2.Count < SampleSize) {
            Debug.LogError($"Point clouds must have at least {SampleSize} points for RANSAC.");
            return;
        }

        int maxInliers = 0;
        Vector3 bestTranslation = Vector3.zero;
        Matrix4x4 bestRotation = Matrix4x4.identity;

        int pointCount = points1.Count;
        int earlyStopThreshold = Mathf.CeilToInt(pointCount * earlyStopInlierRatio);

        for (int iteration = 0; iteration < 10000; iteration++)
        {
            List<Vector3> sample1 = ExtractPoints(points1, GetRandomIndices(pointCount, SampleSize));
            List<Vector3> sample2 = ExtractPoints(points2, GetRandomIndices(pointCount, SampleSize));

            if (ArePointsDegenerate(sample1)) continue;
            if (!ComputeTransformation(sample1, sample2, out var currentRotation, out var currentTranslation)) continue;
            List<Vector3> transformedPoints = TransformPoints(points2, currentRotation, currentTranslation);

            int inliers = CountInliers(points1, transformedPoints, alignmentThreshold);

            if (inliers > maxInliers) {
                maxInliers = inliers;
                bestRotation = currentRotation;
                bestTranslation = currentTranslation;
                if (maxInliers >= earlyStopThreshold) {
                    break;
                }
            }
        }
        translation = bestTranslation;
        rotation = bestRotation;
    }

    private List<Vector3> ExtractPoints(List<Vector3> points, List<int> indices) {
        List<Vector3> sampled = new List<Vector3>(indices.Count);
        foreach (int idx in indices) {
            sampled.Add(points[idx]);
        }
        return sampled;
    }

    private List<int> GetRandomIndices(int count, int sampleSize) {
        HashSet<int> chosen = new HashSet<int>();
        while (chosen.Count < sampleSize) {
            chosen.Add(random.Next(count));
        }
        return new List<int>(chosen);
    }

    private bool ArePointsDegenerate(List<Vector3> pts) {
        if (pts.Count < 3) return true;
        Vector3 v1 = pts[1] - pts[0];
        Vector3 v2 = pts[2] - pts[0];
        float area = Vector3.Cross(v1, v2).magnitude;
        return area < 1e-10f;
    }

    private bool ComputeTransformation(List<Vector3> points1, List<Vector3> points2, out Matrix4x4 rotation, out Vector3 translation) {
        rotation = Matrix4x4.identity;
        translation = Vector3.zero;

        Vector3 centroid1 = ComputeCentroid(points1);
        Vector3 centroid2 = ComputeCentroid(points2);

        List<Vector3> centered1 = CenterPoints(points1, centroid1);
        List<Vector3> centered2 = CenterPoints(points2, centroid2);

        var mat1 = DenseMatrix.OfRows(centered1.Count, 3, centered1.ConvertAll(p => new[] { p.x, p.y, p.z }));
        var mat2 = DenseMatrix.OfRows(centered2.Count, 3, centered2.ConvertAll(p => new[] { p.x, p.y, p.z }));
        var covarianceMatrix = mat1.TransposeThisAndMultiply(mat2);

        var svd = covarianceMatrix.Svd(true);
        var U = svd.U;
        var V = svd.VT.Transpose();

        if (U.Determinant() * V.Determinant() < 0) {
            var S = DenseMatrix.CreateIdentity(3);
            S[2, 2] = -1; 
            U = U * S;
        }
        var R = U * V;
        if (float.IsNaN(R[0, 0])) return false;
        rotation = Matrix4x4FromMathNet(R);
        translation = centroid1 - rotation.MultiplyPoint3x4(centroid2);
        return true;
    }

    private Vector3 ComputeCentroid(List<Vector3> points) {
        Vector3 sum = Vector3.zero;
        foreach (var p in points) {
            sum += p;
        }
        return sum / points.Count;
    }

    private List<Vector3> CenterPoints(List<Vector3> points, Vector3 centroid) {
        List<Vector3> result = new List<Vector3>(points.Count);
        foreach (var p in points) {
            result.Add(p - centroid);
        }
        return result;
    }

    private List<Vector3> TransformPoints(List<Vector3> points, Matrix4x4 rotation, Vector3 translation) {
        List<Vector3> result = new List<Vector3>(points.Count);
        foreach (var p in points) {
            result.Add(rotation.MultiplyPoint3x4(p) + translation);
        }
        return result;
    }

    private int CountInliers(List<Vector3> referencePoints, List<Vector3> transformedPoints, float threshold) {
        int inliers = 0;
        for (int i = 0; i < referencePoints.Count; i++) {
            if (Vector3.Distance(referencePoints[i], transformedPoints[i]) < threshold) {
                inliers++;
            }
        }
        return inliers;
    }

    private Matrix4x4 Matrix4x4FromMathNet(Matrix<float> mat) {
        return new Matrix4x4(
            new Vector4(mat[0,0], mat[0,1], mat[0,2], 0),
            new Vector4(mat[1,0], mat[1,1], mat[1,2], 0),
            new Vector4(mat[2,0], mat[2,1], mat[2,2], 0),
            new Vector4(0,       0,       0,       1)
        );
    }

    public List<Vector3> TransformPointCloud(List<Vector3> Q, Matrix4x4 rotation, Vector3 translation) {
        var transformedPoints = new List<Vector3>(Q.Count);
        foreach (var point in Q) {
            transformedPoints.Add(rotation.MultiplyPoint3x4(point) + translation);
        }
        return transformedPoints;
    }
}