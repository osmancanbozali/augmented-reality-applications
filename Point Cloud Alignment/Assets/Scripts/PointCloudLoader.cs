using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PointCloudLoader : MonoBehaviour {
    public List<Vector3> ReadPointCloud(string filePath) {
        var points = new List<Vector3>();
        var lines = File.ReadAllLines(filePath);
        int numPoints = int.Parse(lines[0]);
        for (int i = 1; i <= numPoints; i++) {
            var parts = lines[i].Split(' ');
            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float z = float.Parse(parts[2]);
            points.Add(new Vector3(x, y, z));
        }
        return points;
    }
}