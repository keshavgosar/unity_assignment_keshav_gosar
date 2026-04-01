using UnityEngine;

/// <summary>
/// Created manual camera controller to make third person camera
/// </summary>

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Positioning")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 2.5f;
    [SerializeField] private float positionSmoothSpeed = 10f;

    [Header("Camera Rotation")]
    [SerializeField] private float lookAtHeightOffset = 1.5f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    [Header("Collision (Wall Clipping)")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float cameraCollisionRadius = 0.3f;

    private Vector3 currentUp;
    private Vector3 cameraForward;

    private void Start()
    {
        if (target != null)
        {
            currentUp = target.up;
            cameraForward = target.forward;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        bool gravityShifted = false;

        // check if gravity shifted (the player's "Up" changed)
        if (Vector3.Angle(currentUp, target.up) > 0.1f)
        {
            currentUp = target.up;
            cameraForward = target.forward;
            gravityShifted = true; // flag that a shift just happened
        }

        cameraForward = Vector3.ProjectOnPlane(cameraForward, target.up).normalized;

        HandleCameraPosition();
        HandleCameraRotation(gravityShifted);
    }

    private void HandleCameraPosition()
    {
        Vector3 desiredPosition = target.position - (cameraForward * distance) + (target.up * height);

        Vector3 headPosition = target.position + (target.up * lookAtHeightOffset);
        Vector3 directionToCamera = (desiredPosition - headPosition).normalized;
        float targetDistance = Vector3.Distance(headPosition, desiredPosition);

        if (Physics.SphereCast(headPosition, cameraCollisionRadius, directionToCamera, out RaycastHit hit, targetDistance, obstacleLayer))
        {
            desiredPosition = headPosition + directionToCamera * hit.distance;
        }

        // snap position to prevent stuttering against the Rigidbody
        transform.position = desiredPosition;
    }

    private void HandleCameraRotation(bool snapRotation)
    {
        Vector3 lookAtTarget = target.position + (target.up * lookAtHeightOffset);
        Vector3 lookDirection = (lookAtTarget - transform.position).normalized;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, target.up);

            if (snapRotation)
            {
                // instantly snap the rotation so it doesn't look inside the player's body!
                transform.rotation = targetRotation;
            }
            else
            {
                // smoothly rotate during normal gameplay
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
            }
        }
    }
}