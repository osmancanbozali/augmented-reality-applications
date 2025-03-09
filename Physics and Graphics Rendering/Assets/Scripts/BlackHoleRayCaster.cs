using UnityEngine;
using System.Collections.Generic;

public class BlackHoleRayCaster : MonoBehaviour
{
    public Camera mainCamera;
    public Transform blackHole;
    [Range(0.1f, 10f)]
    public float gravitationalStrength = 1.0f;
    public int imageWidth = 640;
    public int imageHeight = 480;
    public int raySteps = 50;
    public float eventHorizonRadius = 0.5f;
    public Color backgroundColor = Color.black;
    private float maxRayDistance = 100f;
    private bool shouldRender = true;
    private RenderTexture renderTarget;
    private readonly object lockObject = new object();

    void Start() {
        if (mainCamera == null) mainCamera = Camera.main;
        renderTarget = new RenderTexture(imageWidth, imageHeight, 24);
        renderTarget.enableRandomWrite = true;
        renderTarget.Create();
        RenderScene();
    }

    Vector3 CalculateGravitationalBending(Vector3 point, Vector3 direction) {
        Vector3 toBlackHole = blackHole.position - point;
        float distance = toBlackHole.magnitude;
        distance = Mathf.Max(distance, 0.1f);
        float gravityFactor = gravitationalStrength / (distance * distance);
        return Vector3.Lerp(direction, toBlackHole.normalized, 
            Mathf.Clamp01(gravityFactor)).normalized;
    }

    bool CastCurvedRay(Vector3 origin, Vector3 direction, out RaycastHit finalHit, out Vector3 lastPosition) {
        finalHit = new RaycastHit();
        Vector3 currentPos = origin;
        Vector3 currentDir = direction;
        float totalDistance = 0f;
        float stepSize = maxRayDistance / raySteps;
        lastPosition = origin;

        for (int i = 0; i < raySteps; i++) {
            currentDir = CalculateGravitationalBending(currentPos, currentDir);
            
            if (Physics.Raycast(currentPos, currentDir, out RaycastHit hit, stepSize)) {
                finalHit = hit;
                lastPosition = hit.point;
                return true;
            }

            currentPos += currentDir * stepSize;
            lastPosition = currentPos;
            totalDistance += stepSize;

            float distToBlackHole = Vector3.Distance(currentPos, blackHole.position);
            if (distToBlackHole < eventHorizonRadius) {
                return false;
            }

            if (totalDistance > maxRayDistance) {
                return false;
            }
        }

        return false;
    }

    void RenderScene() {
        if (blackHole == null) {
            Debug.LogError("Black hole reference not set!");
            return;
        }

        shouldRender = false;
        Texture2D outputTexture = new Texture2D(imageWidth, imageHeight);

        float fov = mainCamera.fieldOfView;
        float aspect = mainCamera.aspect;
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        Vector3 cameraUp = mainCamera.transform.up;

        for (int y = 0; y < imageHeight; y++) {
            for (int x = 0; x < imageWidth; x++) {
                float normalizedX = (2.0f * x / imageWidth - 1.0f) * aspect;
                float normalizedY = 2.0f * y / imageHeight - 1.0f;
                float tanFov = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);

                Vector3 rayDirection = cameraForward + cameraRight * (normalizedX * tanFov) + cameraUp * (normalizedY * tanFov);
                rayDirection.Normalize();

                Color pixelColor = backgroundColor;
                Vector3 lastPos = Vector3.zero;

                if (CastCurvedRay(cameraPosition, rayDirection, out RaycastHit hit, out lastPos)) {
                    Renderer renderer = hit.collider.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null) {
                        float diffuse = Mathf.Max(0.2f, Vector3.Dot(hit.normal, -rayDirection));
                        pixelColor = renderer.material.color * diffuse;
                        float distToBlackHole = Vector3.Distance(hit.point, blackHole.position);
                        float darkening = Mathf.Clamp01(distToBlackHole / (gravitationalStrength * 2));                       
                        pixelColor = Color.Lerp(Color.black, pixelColor, darkening);
                        pixelColor += new Color(0.1f, 0.1f, 0.1f, 0f);
                    }
                }
                outputTexture.SetPixel(x, y, pixelColor);
            }
        }

        outputTexture.Apply();
        SaveRenderedImage(outputTexture);
        
        Destroy(outputTexture);
        shouldRender = true;
    }

    void SaveRenderedImage(Texture2D texture) {
        try {
            string filePath = $"{Application.dataPath}/BlackHoleRender.png";
            byte[] imageBytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(filePath, imageBytes);
            Debug.Log($"Rendered image saved at: {filePath}");
        }
        catch (System.Exception e) {
            Debug.LogError($"Failed to save image: {e.Message}");
        }
    }
}