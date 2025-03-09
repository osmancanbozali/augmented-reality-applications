using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using Unity.Collections;

public class JoystickMovement : MonoBehaviour
{
    public GameObject objectToMovePrefab;
    public Joystick joystick;
    private ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject movingObject = null;
    public float speed = 0.1f;
    private ARPlane currentPlane;
    private Vector3 moveDirection;

    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        HandleTouchToPlaceObject();
        MoveObjectWithJoystick();
    }

    void HandleTouchToPlaceObject()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

            if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                if (movingObject == null)
                {
                    movingObject = Instantiate(objectToMovePrefab, hitPose.position, hitPose.rotation);
                    currentPlane = hits[0].trackable as ARPlane;
                }
            }
        }
    }

    void MoveObjectWithJoystick()
    {
        if (movingObject != null && (joystick.Horizontal != 0 || joystick.Vertical != 0))
        {
            moveDirection = new Vector3(joystick.Horizontal, 0, joystick.Vertical).normalized;
            Vector3 newPosition = movingObject.transform.position + moveDirection * speed * Time.deltaTime;

            if (currentPlane != null && IsPointInPolygon(new Vector2(newPosition.x, newPosition.z), currentPlane.boundary))
            {
                movingObject.transform.position = newPosition;
            }
        }
    }

    bool IsPointInPolygon(Vector2 point, NativeArray<Vector2> boundary)
    {
        Vector3 localPoint = currentPlane.transform.InverseTransformPoint(new Vector3(point.x, 0, point.y));
        bool inside = false;

        for (int i = 0, j = boundary.Length - 1; i < boundary.Length; j = i++)
        {
            Vector2 vertex1 = boundary[i];
            Vector2 vertex2 = boundary[j];

            if (((vertex1.y > localPoint.z) != (vertex2.y > localPoint.z)) &&
                (localPoint.x < (vertex2.x - vertex1.x) * (localPoint.z - vertex1.y) / (vertex2.y - vertex1.y) + vertex1.x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}