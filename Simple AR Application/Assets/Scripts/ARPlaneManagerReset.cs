using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlaneManagerReset : MonoBehaviour
{
    private ARPlaneManager arPlaneManager;

    void Start()
    {
        arPlaneManager = GetComponent<ARPlaneManager>();
        ResetARPlanes();
    }

    public void ResetARPlanes()
    {
        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = false;
            foreach (ARPlane plane in arPlaneManager.trackables)
            {
                Destroy(plane.gameObject);
            }
            arPlaneManager.enabled = true;
        }
    }
}