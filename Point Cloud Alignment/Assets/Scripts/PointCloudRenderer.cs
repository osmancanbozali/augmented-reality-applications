using System.Collections.Generic;
using UnityEngine;

public class PointCloudRenderer : MonoBehaviour {
    public void RenderPointCloud(List<Vector3> points, Color color, Transform parent) {
        foreach (var point in points) {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = point;
            sphere.transform.localScale = Vector3.one * 0.6f;
            var renderer = sphere.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
            sphere.transform.parent = parent;
        }
    }
}