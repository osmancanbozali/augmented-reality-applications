using UnityEngine;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour
{
    public Image displayImage;
    public Sprite[] imageSprites;
    private int currentImageIndex = 0;
    private PointSelector pointSelector;
    public ObjectPlacer objectPlacer;

    private void Start()
    {
        pointSelector = Object.FindAnyObjectByType<PointSelector>();
        if (imageSprites.Length > 0) {
            ShowImage(currentImageIndex);
        }
    }

    public void ShowNextImage()
    {
        currentImageIndex = (currentImageIndex + 1) % imageSprites.Length;
        ShowImage(currentImageIndex);
    }

    public void ShowPreviousImage()
    {
        currentImageIndex = (currentImageIndex - 1 + imageSprites.Length) % imageSprites.Length;
        ShowImage(currentImageIndex);
    }

    private void ShowImage(int index)
    {
        displayImage.sprite = imageSprites[index];
        if (pointSelector != null) {
            pointSelector.ResetPoints();
        }
        if (objectPlacer != null) {
            objectPlacer.DeleteCurrentObject();
            objectPlacer.UpdateForImageIndex(index);
        }
    }
}