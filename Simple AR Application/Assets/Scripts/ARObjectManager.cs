using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ARObjectManager : MonoBehaviour
{
    public GameObject cubePrefab, spherePrefab, capsulePrefab;
    private GameObject selectedPrefab;
    private ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Dictionary<GameObject, GameObject> placedObjects = new Dictionary<GameObject, GameObject>();
    private GameObject activeObject = null;
    private Color originalColor;
    public Button cubeButton, sphereButton, capsuleButton;
    private bool isDragging = false;

    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        cubeButton.onClick.AddListener(() => SelectPrefab(cubePrefab));
        sphereButton.onClick.AddListener(() => SelectPrefab(spherePrefab));
        capsuleButton.onClick.AddListener(() => SelectPrefab(capsulePrefab));
        selectedPrefab = cubePrefab;
    }

    void Update()
    {
        HandleTouchInput();
    }

    void SelectPrefab(GameObject prefab)
    {
        selectedPrefab = prefab;
    }

    void HandleTouchInput()
    {
        var touch = Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = touch.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject touchedObject = hit.transform.gameObject;

                if (activeObject == touchedObject)
                {
                    isDragging = true;
                }
                else
                {
                    SelectActiveObject(touchedObject);
                }
                return;
            }

            if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                if (!placedObjects.ContainsKey(selectedPrefab))
                {
                    GameObject placedObject = Instantiate(selectedPrefab, hitPose.position, hitPose.rotation);
                    placedObjects[selectedPrefab] = placedObject;
                }
            }
        }

        if (isDragging && touch.press.isPressed)
        {
            DragActiveObject(touch.position.ReadValue());
        }

        if (isDragging && touch.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    void SelectActiveObject(GameObject obj)
    {
        if (activeObject != null) DeselectActiveObject();

        activeObject = obj;
        Renderer renderer = activeObject.GetComponent<Renderer>();

        originalColor = renderer.material.color;
        renderer.material.color = Color.yellow;

    }

    void DeselectActiveObject()
    {
        Renderer renderer = activeObject.GetComponent<Renderer>();
        renderer.material.color = originalColor;
        activeObject = null;

    }

    void DragActiveObject(Vector2 touchPosition)
    {
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            activeObject.transform.position = hitPose.position;
            Debug.Log($"Dragging {activeObject.name} to {hitPose.position}");
        }
    }
}
