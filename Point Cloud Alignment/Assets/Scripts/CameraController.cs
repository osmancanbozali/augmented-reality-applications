using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 100f;
    public bool allowRotation = false;

    private Camera cam;

    private void Start() {
        cam = GetComponent<Camera>();
    }

    private void Update() {
        HandleMovement();
        if (allowRotation) {
            HandleRotation();
        }
    }

    private void HandleMovement() {
        float horizontal = Input.GetAxis("Horizontal"); // w: up s: down a: left d: right
        float vertical   = Input.GetAxis("Vertical");

        float upDown = 0f;
        if (Input.GetKey(KeyCode.Q)) // up
        {
            upDown = 1f;
        }
        else if (Input.GetKey(KeyCode.E)) // down
        {
            upDown = -1f;
        }

        Vector3 movement = (transform.right * horizontal) + (transform.forward * vertical) + (transform.up * upDown);
        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation() // k: rotate left l: rotate right
    {
        float rotateY = 0f;
        if (Input.GetKey(KeyCode.K))
        {
            rotateY = -1f;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            rotateY = 1f;
        }
        if (rotateY != 0f)
        {
            transform.Rotate(Vector3.up, rotateY * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}