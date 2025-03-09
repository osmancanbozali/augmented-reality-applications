using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PointSelector : MonoBehaviour
{
    public Image displayedImage;
    public GameObject pointIndicatorPrefab;
    private List<Vector2> selectedPoints = new List<Vector2>();
    private int maxPoints = 5;

    public delegate void OnPointsSelected(List<Vector2> points);
    public static event OnPointsSelected PointsSelected;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            if (EventSystem.current.IsPointerOverGameObject()) {
                PointerEventData pointerData = new PointerEventData(EventSystem.current) {
                    position = Input.mousePosition
                };
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, raycastResults);

                foreach (var result in raycastResults) {
                    if (result.gameObject != displayedImage.gameObject) {
                        return;
                    }
                }
            }

            Vector2 localPoint;
            RectTransform rectTransform = displayedImage.rectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                Input.mousePosition,
                null,
                out localPoint
            );
            Vector2 adjustedPoint = ConvertToTopLeftOrigin(localPoint, rectTransform);
            if (selectedPoints.Count < maxPoints) {
                selectedPoints.Add(adjustedPoint);
                CreatePointIndicator(localPoint);
                if (selectedPoints.Count == maxPoints) {
                    PointsSelected?.Invoke(selectedPoints);
                }
            }
        }
    }

    private Vector2 ConvertToTopLeftOrigin(Vector2 localPoint, RectTransform rectTransform)
    {
        return new Vector2(
            localPoint.x + (rectTransform.rect.width / 2),
            (rectTransform.rect.height / 2) - localPoint.y
        );
    }

    private void CreatePointIndicator(Vector2 localPoint)
    {
        GameObject indicator = Instantiate(pointIndicatorPrefab, displayedImage.transform);
        RectTransform indicatorRect = indicator.GetComponent<RectTransform>();
        indicatorRect.anchoredPosition = new Vector2(localPoint.x, localPoint.y);
    }

    public void ResetPoints()
    {
        selectedPoints.Clear();
        foreach (Transform child in displayedImage.transform) {
            if (child.gameObject.CompareTag("PointIndicator")) {
                Destroy(child.gameObject);
            }
        }
    }
}