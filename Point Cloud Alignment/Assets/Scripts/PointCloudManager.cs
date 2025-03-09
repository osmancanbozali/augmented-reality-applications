using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointCloudManager : MonoBehaviour
{
    public string pointCloudFile1 = "Assets/PointCloudData/5a.txt";
    public string pointCloudFile2 = "Assets/PointCloudData/5b.txt";
    public Text infoText;

    private PointCloudLoader loader;
    private PointCloudRenderer pointCloudRenderer;
    private PointCloudAligner aligner;

    private List<Vector3> points1;
    private List<Vector3> points2;
    private List<Vector3> transformed2;

    private List<GameObject> lines = new List<GameObject>();
    private Transform parentTransformed2;
    private bool linesVisible = true;

    void Start() {
        loader = GetComponent<PointCloudLoader>();
        pointCloudRenderer = GetComponent<PointCloudRenderer>();
        aligner = GetComponent<PointCloudAligner>();

        points1 = loader.ReadPointCloud(pointCloudFile1);
        points2 = loader.ReadPointCloud(pointCloudFile2);

        var parent1 = new GameObject("PointCloud1").transform;
        var parent2 = new GameObject("PointCloud2").transform;

        pointCloudRenderer.RenderPointCloud(points1, Color.magenta, parent1);
        pointCloudRenderer.RenderPointCloud(points2, Color.blue, parent2);
    }

    public void RunAlignment() {
        Vector3 translation;
        Matrix4x4 rotation;

        ClearPreviousResults(); // Delete previous results from the scene

        aligner.RunRANSAC(points1, points2, out translation, out rotation);
        transformed2 = aligner.TransformPointCloud(points2, rotation, translation);

        parentTransformed2 = new GameObject("TransformedPointCloud2").transform;
        pointCloudRenderer.RenderPointCloud(transformed2, Color.green, parentTransformed2);
        DrawLines(points2, transformed2);
        DisplayRansacResults(rotation, translation);
    }

    public void ToggleLines() {
        linesVisible = !linesVisible;
        foreach (var line in lines)
        {
            line.SetActive(linesVisible);
        }
    }

    private void DrawLines(List<Vector3> original, List<Vector3> transformed) {
        for (int i = 0; i < original.Count; i++) {
            var line = new GameObject("Line");
            var lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[] { original[i], transformed[i] });
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material.color = Color.yellow;
            lines.Add(line);
        }
    }

    private void ClearPreviousResults() {
        if (parentTransformed2 != null) {
            Destroy(parentTransformed2.gameObject);
        }
        foreach (var line in lines) { // Delete lines
            Destroy(line);
        }
        lines.Clear();
    }
    
    private void DisplayRansacResults(Matrix4x4 rotation, Vector3 translation) {
        infoText.text = $"Translation Vector:\n[{translation.x:F4} {translation.y:F4} {translation.z:F4}]\nRotation Matrix:\n" +
                              $"{rotation.m00:F4} {rotation.m01:F4} {rotation.m02:F4}\n" +
                              $"{rotation.m10:F4} {rotation.m11:F4} {rotation.m12:F4}\n" +
                              $"{rotation.m20:F4} {rotation.m21:F4} {rotation.m22:F4}";
    }
}