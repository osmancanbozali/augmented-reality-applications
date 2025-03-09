using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class Part1Homography : MonoBehaviour
{
    [System.Serializable]
    public struct ImageData
    {
        public Vector2[] points;
        public Matrix<double> homography;
    }

    private ImageData[] imageData = new ImageData[3];
    private ImageData[] additionalImageData = new ImageData[3];
    private Vector2[] scenePoints;
    private Vector2[] additionalScenePoints;

    void Start()
    {
        InitializePoints();
        CalculateAllHomographies();
        TestProjections();
    }

    void InitializePoints()
    {
        // Manually calculated scene points
        scenePoints = new Vector2[]
        {
            new Vector2(100, 100),
            new Vector2(800, 100),
            new Vector2(800, 600),
            new Vector2(100, 600),
            new Vector2(500, 400)
        };

        imageData = new ImageData[3];
        
        // Manually calculated image points for IMG_1
        imageData[0].points = new Vector2[]
        {
            new Vector2(704, 755),
            new Vector2(686, 2378),
            new Vector2(1837, 2382),
            new Vector2(1853, 770),
            new Vector2(1384, 1687)
        };

        // Manually calculated image points for IMG_2
        imageData[1].points = new Vector2[]
        {
            new Vector2(507, 737),
            new Vector2(489, 2379),
            new Vector2(1670, 2426),
            new Vector2(1690, 708),
            new Vector2(1192, 1682)
        };

        // Manually calculated image points for IMG_3
        imageData[2].points = new Vector2[]
        {
            new Vector2(745, 876),
            new Vector2(716, 2463),
            new Vector2(1763, 2430),
            new Vector2(1901, 947),
            new Vector2(1398, 1827)
        };

        additionalScenePoints = new Vector2[] { new Vector2(100, 300), new Vector2(200, 400), new Vector2(300, 500) }; // additional scene points [1.5]
        additionalImageData = new ImageData[3];
        additionalImageData[0].points = new Vector2[] { new Vector2(1167, 762), new Vector2(1393, 996), new Vector2(1618, 1230) }; // additional image points for IMG_1 [1.5]
        additionalImageData[1].points = new Vector2[] { new Vector2(969, 728), new Vector2(1202, 963), new Vector2(1438, 1202) }; // additional image points for IMG_2 [1.5]
        additionalImageData[2].points = new Vector2[] { new Vector2(1228, 908), new Vector2(1442, 1159), new Vector2(1644, 1396) }; // additional image points for IMG_3 [1.5]
    }

    void CalculateAllHomographies()
    {
        for (int i = 0; i < 3; i++)
        {
            imageData[i].homography = CalculateHomography(scenePoints, imageData[i].points);
            string matrixString = "\n";
            for (int k = 0; k < 3; k++)
            {
                for (int j = 0; j < 3; j++)
                {
                    matrixString += imageData[i].homography[k, j].ToString("F6") + "\t";
                }
                matrixString += "\n";
            }
            Debug.Log($"[1.1] Homography Matrix for Image {i + 1}:" + matrixString);
        }
    }

    Matrix<double> CalculateHomography(Vector2[] srcPoints, Vector2[] dstPoints)
    {
        int n = srcPoints.Length;
        var A = DenseMatrix.Create(2 * n, 9, 0);

        for (int i = 0; i < n; i++)
        {
            double x = srcPoints[i].x;
            double y = srcPoints[i].y;
            double u = dstPoints[i].x;
            double v = dstPoints[i].y;

            A.SetRow(2 * i, new double[] { -x, -y, -1, 0, 0, 0, u * x, u * y, u });
            A.SetRow(2 * i + 1, new double[] { 0, 0, 0, -x, -y, -1, v * x, v * y, v });
        }

        var svd = A.Svd();
        var V = svd.VT.Transpose();
        var h = V.Column(V.ColumnCount - 1);

        var H = DenseMatrix.Create(3, 3, 0);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                H[i, j] = h[i * 3 + j];
            }
        }
        H = (DenseMatrix)H.Divide(H[2, 2]);
        return H;
    }

    Vector2 ProjectSceneToImage(Vector2 scenePoint, Matrix<double> homography)
    {
        var p = Vector<double>.Build.Dense(3);
        p[0] = scenePoint.x;
        p[1] = scenePoint.y;
        p[2] = 1;

        var result = homography.Multiply(p);
        return new Vector2(
            (float)(result[0] / result[2]),
            (float)(result[1] / result[2])
        );
    }

    Vector2 ProjectImageToScene(Vector2 imagePoint, Matrix<double> homography)
    {
        var inverseH = homography.Inverse();
        var p = Vector<double>.Build.Dense(3);
        p[0] = imagePoint.x;
        p[1] = imagePoint.y;
        p[2] = 1;

        var result = inverseH.Multiply(p);
        return new Vector2(
            (float)(result[0] / result[2]),
            (float)(result[1] / result[2])
        );
    }

    void TestProjections()
    {
        // Scene Points from [1.6]
        Vector2[] testScenePoints = new Vector2[]
        {
            new Vector2(7.5f, 5.5f),
            new Vector2(6.3f, 3.3f),
            new Vector2(0.1f, 0.1f)
        };
        // Image Points from [1.7]
        Vector2[] testImagePoints = new Vector2[]
        {
            new Vector2(500, 400),
            new Vector2(86, 167),
            new Vector2(10, 10)
        };

        for (int i = 0; i < 3; i++)
        {
            string matrixString = "\n";
            matrixString += $"Results for Image {i + 1}:\n";
            matrixString += ProjectWithErrorCalculation(i);
            matrixString += "\n[1.6] Scene to Image Projections:\n";
            foreach (var point in testScenePoints)
            {
                Vector2 projected = ProjectSceneToImage(point, imageData[i].homography);
                matrixString += $"Scene point {point} -> Projected Image point {projected}\n";
            }
            matrixString += "\n[1.7] Image to Scene Projections:\n";
            foreach (var point in testImagePoints)
            {
                Vector2 projected = ProjectImageToScene(point, imageData[i].homography);
                matrixString += $"Image point {point} -> Projected Scene point {projected}\n";
            }

            Debug.Log(matrixString);      
        }
    }

    string ProjectWithErrorCalculation(int imageIndex)
    {
        string result = "[1.5]\n";
        for (int i = 0; i < additionalScenePoints.Length; i++)
        {
            Vector2 projected = ProjectSceneToImage(additionalScenePoints[i], imageData[imageIndex].homography);
            float error = Vector2.Distance(projected, additionalImageData[imageIndex].points[i]);
            result += $"Scene point {additionalScenePoints[i]} -> Projected Image point {projected} --> Actual Image point {additionalImageData[imageIndex].points[i]}\n";
            result += $"Error: {error} pixels\n";
        }
        return result;
    }
}