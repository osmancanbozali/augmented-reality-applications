using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARSubsystems;

public class AnimateObjectOnPlane : MonoBehaviour
{
    public GameObject animatedObjectPrefab;
    private ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject placedObject = null;
    private Animator objectAnimator;

    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

            if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                if (placedObject == null)
                {
                    placedObject = Instantiate(animatedObjectPrefab, hitPose.position, hitPose.rotation);
                    objectAnimator = placedObject.GetComponent<Animator>();
                }
                else
                {
                    placedObject.transform.position = hitPose.position;
                    if (objectAnimator != null)
                    {
                        objectAnimator.SetTrigger("PlayAnimation");
                    }
                }
            }
        }
    }
}
