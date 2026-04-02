using UnityEngine;

/// <summary>
/// Third person camera controller with mouse orbit using the New Input System.
/// Handles dynamic gravity shifts and prevents wall clipping.
/// </summary>

public class CameraController : MonoBehaviour
{
    private PlayerInput_Actions input;

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Positioning")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float positionSmoothSpeed = 10f; // smooth clipping recovery

    [Header("Camera Rotation & Mouse")]
    [SerializeField] private float lookAtHeightOffset = 1.5f;
    [SerializeField] private float rotationSmoothSpeed = 15f;
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float minPitch = -30f; // look down limit
    [SerializeField] private float maxPitch = 60f;  // look up limit

    [Header("Collision (Wall Clipping)")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float cameraCollisionRadius = 0.3f;

    private Vector3 currentUp;
    private Vector3 cameraForward;

    // mouse input values
    private Vector2 lookInput;
    private float pitch; // vertical angle

    private void Awake()
    {
        input = new PlayerInput_Actions();
    }

    private void OnEnable()
    {
        input.Enable();

        // read mouse delta from the new input system
        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        GameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        input.Player.Look.performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled -= ctx => lookInput = Vector2.zero;

        GameManager.OnGameOver -= HandleGameOver;

        input.Disable();
    }

    private void Start()
    {
        if (target != null)
        {
            currentUp = target.up;
            cameraForward = target.forward;
        }

        // lock cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        bool gravityShifted = false;

        // Check if gravity shifted, if the players "Up" changed
        if (Vector3.Angle(currentUp, target.up) > 0.1f)
        {
            currentUp = target.up;

            // snap the camera forward to match the player when gravity changes
            cameraForward = target.forward;
            pitch = 0f; // reset pitch to look straight ahead
            gravityShifted = true;
        }

        // process Mouse Input
        // Yaw (left/right) rotates the cameraForward vector around the players Up axis
        float yaw = lookInput.x * mouseSensitivity;
        if (yaw != 0)
        {
            cameraForward = Quaternion.AngleAxis(yaw, target.up) * cameraForward;
        }

        // keep cameraForward strictly flat relative to the gravity plane
        cameraForward = Vector3.ProjectOnPlane(cameraForward, target.up).normalized;

        // pitch (Up/Down) adjusts the vertical viewing angle
        pitch -= lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        HandleCameraPositionAndRotation(gravityShifted);
    }

    private void HandleGameOver(bool isWin)
    {
        // disable camera mouse input
        input.Disable();

        // unlock and show the mouse cursor again
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleCameraPositionAndRotation(bool snapCamera)
    {
        // calculate where the camera should pivot around
        Vector3 headPosition = target.position + (target.up * lookAtHeightOffset);

        // combine Yaw (cameraForward) and Pitch to get the final look direction
        Vector3 cameraRight = Vector3.Cross(target.up, cameraForward).normalized;
        Vector3 finalLookDirection = Quaternion.AngleAxis(pitch, cameraRight) * cameraForward;

        // calculate ideal position
        Vector3 desiredPosition = headPosition - (finalLookDirection * distance);

        // wall clipping check
        Vector3 directionToCamera = (desiredPosition - headPosition).normalized;
        float targetDistance = Vector3.Distance(headPosition, desiredPosition);
        Vector3 finalPosition = desiredPosition;

        if (Physics.SphereCast(headPosition, cameraCollisionRadius, directionToCamera, out RaycastHit hit, targetDistance, obstacleLayer))
        {
            finalPosition = headPosition + directionToCamera * hit.distance;
        }

        // apply Position & Rotation instantly
        // mouse look cameras must be responsive to prevent jitter against Rigidbodies
        transform.position = finalPosition;
        transform.rotation = Quaternion.LookRotation(finalLookDirection, target.up);
    }
}