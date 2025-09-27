using UnityEngine;

public class WASDOrbitCamera : MonoBehaviour
{
    // === Public Variables (Camera Settings) ===
    public Transform target;           // The object to orbit around
    public float distance = 5f;        // Initial distance from the target
    public float rotationSpeed = 50f;  // Speed of orbit (WASD / Arrow Keys)
    public float zoomSpeed = 2f;       // Speed of zoom (Mouse Scroll Wheel)
    public float minDistance = 1f;     // Minimum zoom distance
    public float maxDistance = 20f;    // Maximum zoom distance

    // === Camera Rotation Variables ===
    private float currentX = 0f;       // Horizontal rotation angle
    private float currentY = 20f;      // Vertical rotation angle
    public float minY = 5f;            // Minimum vertical angle
    public float maxY = 80f;           // Maximum vertical angle

    void Update()
    {
        // === Handle Orbit Rotation with WASD / Arrow Keys ===
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float verticalInput = Input.GetAxis("Vertical");     // W/S or Up/Down

        // Update rotation angles based on input
        currentX += horizontalInput * rotationSpeed * Time.deltaTime;
        currentY -= verticalInput * rotationSpeed * Time.deltaTime;

        // Clamp vertical angle so the camera doesn't flip
        currentY = Mathf.Clamp(currentY, minY, maxY);

        // === Handle Zoom with Mouse Scroll Wheel ===
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        distance -= scrollInput * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        // === Position and Orient Camera Around Target ===
        if (target == null) return;

        // Create rotation from X/Y angles
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // Calculate camera position relative to target
        Vector3 direction = new Vector3(0, 0, -distance);
        transform.position = target.position + rotation * direction;

        // Always look at the target
        transform.LookAt(target);
    }
}

