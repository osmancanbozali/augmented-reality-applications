using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class InteractableObject : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    private Vector3 initialScale;
    private float initialDistance;
    private Vector3 initialObjectScale;
    private bool isDragging = false;
    private ARRaycastManager arRaycastManager;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
        initialScale = transform.localScale;
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    void Update()
    {
        DetectTouch();
        DetectPinch();
        HandleDrag();
    }

    private void DetectTouch()
    {
        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Touchscreen.current.primaryTouch.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                objectRenderer.material.color = Random.ColorHSV(); //rand color
            }
        }
    }

    private void DetectPinch()
    {
        if (Touchscreen.current.touches.Count >= 2)
        {
            var touch0 = Touchscreen.current.touches[0];
            var touch1 = Touchscreen.current.touches[1];

            if (touch0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began &&
                touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touch0.position.ReadValue(), touch1.position.ReadValue());
                initialObjectScale = transform.localScale;
            }
            else if (touch0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved &&
                     touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touch0.position.ReadValue(), touch1.position.ReadValue());
                float scaleFactor = currentDistance / initialDistance;
                transform.localScale = initialObjectScale * scaleFactor;
            }
        }
    }

    private void HandleDrag()
    {
        if (Touchscreen.current.primaryTouch.press.isPressed && !isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Touchscreen.current.primaryTouch.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                isDragging = true;
            }
        }

        if (isDragging)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                transform.position = hitPose.position;
            }

            if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                isDragging = false;
            }
        }
    }
}