using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject objectPrefab;
    public RectTransform displayedImageRect;
    public Camera mainCamera;
    private Matrix4x4 intrinsicMatrix;
    private List<Matrix4x4> extrinsicMatrices = new List<Matrix4x4>();
    private int currentImageIndex = 0;
    private GameObject currentObject;

    private void Start()
    {
        LoadCameraParameters("assets/camera_data.json");
    }

    public void UpdateForImageIndex(int index)
    {
        currentImageIndex = index;
    }

    public void PlaceObject(Vector2 transformedPoint)
    {

        if (currentImageIndex >= extrinsicMatrices.Count) {
            return;
        }
        DeleteCurrentObject();
        // Teapot placement with rotation and translation
        Matrix4x4 extrinsicMatrix = extrinsicMatrices[currentImageIndex];
        Vector4 cameraSpacePoint = new Vector4(transformedPoint.x + 140, 2418 - transformedPoint.y, 5f, 1);
        Vector3 worldPosition = new Vector3(cameraSpacePoint.x, cameraSpacePoint.y, cameraSpacePoint.z);
        currentObject = Instantiate(objectPrefab, worldPosition, Quaternion.identity);
        Vector3 forward = new Vector3(extrinsicMatrix.m02, extrinsicMatrix.m12, extrinsicMatrix.m22);
        Vector3 up = new Vector3(extrinsicMatrix.m01, extrinsicMatrix.m11, extrinsicMatrix.m21);
        currentObject.transform.rotation = Quaternion.LookRotation(forward, up);
        currentObject.transform.Rotate(-60, 0, 0, Space.Self); // Rotate the teapot to match the camera (due to unity)
        Vector3 currentRotation = currentObject.transform.eulerAngles;
        currentRotation.y = -currentRotation.y;
        currentRotation.z = currentRotation.z -10f;
        currentObject.transform.eulerAngles = currentRotation;
        currentObject.transform.localScale = new Vector3(200, 200, 200);
    }

    public void DeleteCurrentObject()
    {
        if (currentObject != null)
        {
            Destroy(currentObject);
            currentObject = null;
        }
    }

    private void LoadCameraParameters(string filePath)
    {
        string json = File.ReadAllText(filePath);
        CameraData cameraData = JsonConvert.DeserializeObject<CameraData>(json);

        // Load intrinsic parameters matrix
        intrinsicMatrix = Matrix4x4.identity;
        intrinsicMatrix.m00 = cameraData.intrinsics.focal_length; // fx
        intrinsicMatrix.m11 = cameraData.intrinsics.focal_length; // fy
        intrinsicMatrix.m02 = cameraData.intrinsics.principal_point[0]; // cx
        intrinsicMatrix.m12 = cameraData.intrinsics.principal_point[1]; // cy

        // Load extrinsic parameters matrices for all images
        foreach (CameraExtrinsics extrinsicData in cameraData.extrinsics) {
            Matrix4x4 extrinsicMatrix = Matrix4x4.identity;
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    extrinsicMatrix[i, j] = extrinsicData.rotation_matrix[i][j];
                }
            }
            extrinsicMatrix.m03 = extrinsicData.translation[0];
            extrinsicMatrix.m13 = extrinsicData.translation[1];
            extrinsicMatrix.m23 = extrinsicData.translation[2];
            extrinsicMatrices.Add(extrinsicMatrix);
        }
    }
}

public class CameraIntrinsics
{
    public float focal_length { get; set; }
    public List<float> principal_point { get; set; }
    public List<int> image_size { get; set; }
    public float distortion { get; set; }
}

public class CameraExtrinsics
{
    public string image_name { get; set; }
    public List<List<float>> rotation_matrix { get; set; }
    public List<float> translation { get; set; }
}

public class CameraData
{
    public CameraIntrinsics intrinsics { get; set; }
    public List<CameraExtrinsics> extrinsics { get; set; }
}