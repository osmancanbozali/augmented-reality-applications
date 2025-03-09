using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

public class HomographyCalculator : MonoBehaviour
{
    public ObjectPlacer objectPlacer;

    private Vector2[] referencePoints = new Vector2[]
    {
        new Vector2(1580, 1600), // Blue USB
        new Vector2(1440, 1770), // Red USB 
        new Vector2(1903, 1773), // Dot on white object
        new Vector2(1828, 1745), // Upper left corner of usb cables
        new Vector2(1840, 1704) // Lower left corner of usb cables
    };

    private void OnEnable()
    {
        PointSelector.PointsSelected += OnPointsSelected;
    }

    private void OnDisable()
    {
        PointSelector.PointsSelected -= OnPointsSelected;
    }

    private void OnPointsSelected(List<Vector2> userSelectedPoints)
    {
        if (userSelectedPoints.Count != referencePoints.Length) {
            return;
        }

        Matrix<double> homography = CalculateHomography(referencePoints, userSelectedPoints.ToArray());
        Vector2 placementPointReference = new Vector2(1910, 1730);
        Vector2 transformedPlacementPoint = TransformPoint(placementPointReference, homography);
        objectPlacer.PlaceObject(transformedPlacementPoint);
    }

    private Matrix<double> CalculateHomography(Vector2[] refPoints, Vector2[] imgPoints)
    {
        int n = refPoints.Length;
        var A = Matrix<double>.Build.Dense(2 * n, 9);

        for (int i = 0; i < n; i++) {
            double x = refPoints[i].x;
            double y = refPoints[i].y;
            double u = imgPoints[i].x;
            double v = imgPoints[i].y;

            A.SetRow(2 * i, new double[] { -x, -y, -1, 0, 0, 0, x * u, y * u, u });
            A.SetRow(2 * i + 1, new double[] { 0, 0, 0, -x, -y, -1, x * v, y * v, v });
        }
        var svd = A.Svd(true);
        var h = svd.VT.Row(svd.VT.RowCount - 1);
        return Matrix<double>.Build.DenseOfRowMajor(3, 3, h.ToArray());
    }

    private Vector2 TransformPoint(Vector2 point, Matrix<double> homography)
    {
        var vec = Vector<double>.Build.DenseOfArray(new double[] { point.x, point.y, 1 });
        var result = homography.Multiply(vec);
        float u = (float)(result[0] / result[2]);
        float v = (float)(result[1] / result[2]);
        return new Vector2(u, v);
    }
}