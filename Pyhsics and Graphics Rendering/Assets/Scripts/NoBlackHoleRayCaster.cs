using UnityEngine;

public class NoBlackHoleRayCaster : MonoBehaviour
{
    public Camera mainCamera;
    public int imageWidth = 640;
    public int imageHeight = 480;
    public Color backgroundColor = Color.black;
    private float maxRayDistance = 150f;
    private Texture2D renderedImage;

    void Start() {
        if (mainCamera == null)
            mainCamera = Camera.main;
        renderedImage = new Texture2D(imageWidth, imageHeight);
        RenderScene();
    }

    void RenderScene() {

        for (int y = 0; y < imageHeight; y++) {
            for (int x = 0; x < imageWidth; x++) {
                float viewportX = (float)x / (imageWidth - 1);
                float viewportY = (float)y / (imageHeight - 1);

                Vector3 rayOrigin = mainCamera.transform.position;
                Vector3 rayTarget = mainCamera.ViewportToWorldPoint(new Vector3(viewportX, viewportY, 1.0f));
                Vector3 rayDirection = (rayTarget - rayOrigin).normalized;

                Color pixelColor = backgroundColor;

                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxRayDistance)) {
                    Renderer renderer = hit.collider.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null) {
                        float diffuse = Mathf.Max(0.2f, Vector3.Dot(hit.normal, -rayDirection));
                        pixelColor = renderer.material.color * diffuse;
                        pixelColor += new Color(0.1f, 0.1f, 0.1f, 0f);
                    }
                }
                renderedImage.SetPixel(x, y, pixelColor);
            }
        }
        renderedImage.Apply();
        SaveRenderedImage();
    }

    void SaveRenderedImage() {
        try {
            string filePath = $"{Application.dataPath}/NormalRender.png";
            byte[] imageBytes = renderedImage.EncodeToPNG();
            System.IO.File.WriteAllBytes(filePath, imageBytes);
            Debug.Log($"Normal render saved at: {filePath}");
        }
        catch (System.Exception e) {
            Debug.LogError($"Failed to save image: {e.Message}");
        }
    }

    void OnDestroy() {
        if (renderedImage != null)
            Destroy(renderedImage);
    }
}