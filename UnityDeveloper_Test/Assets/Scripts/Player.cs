using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement, jumping, and gravity alignment logic.
/// </summary>

public class Player : MonoBehaviour
{
    // using new input actions for player input
    public PlayerInput_Actions input { get; private set; }

    [Header("Animations")]
    private Animator anim;

    // layer used to compare collision with the ground
    [Header("Ground Layer")]
    [SerializeField] private LayerMask groundLayer;

    // layer used to compare the collision object
    [Header("Collectible Layer")]
    [SerializeField] private LayerMask collectibleLayer;

    [Header("Input Settings")]
    private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    private float rotationSpeed = 10f;

    [Header("Ground & Fall Checks")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private float maxFallDistance = 50f; // distance before game over triggers
    private bool isOnGround;

    [Header("Surface Alignment")]
    [SerializeField] private float surfaceAlignmentSpeed = 15f;
    [SerializeField] private float surfaceDetectionDistance = 2f;

    [Header("Hologram Settings")]
    [SerializeField] private float headHeightOffset = 1.5f;

    [Header("Gravity Manipulation Settings")]
    [SerializeField] private float gravityStrength = 9.81f; // usign standard earth gravity
    private Vector3 currentGravityDir = Vector3.down;
    private Vector3 pendingGravityDir;
    private Quaternion pendingRotation;
    private bool isHologramActive = false;

    [Header("Components")]
    private Rigidbody rb;
    private HologramSystem hologramSystem;

    private void Awake()
    {
        input = new PlayerInput_Actions();

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        hologramSystem = GetComponent<HologramSystem>();

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        isOnGround = true;
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementDisabled;

        input.Player.Jump.performed += HandleJump;

        input.Player.Hologram.performed += OnShowHologram;
        input.Player.Hologram.canceled += OnHideHologram;

        input.Player.GravityChange.performed += OnGravityChangeReq;
    }

    private void Update()
    {
        float currentSpeed = moveInput.magnitude;
        anim.SetFloat("Speed", currentSpeed);

        anim.SetBool("isGrounded", isOnGround);
    }

    private void FixedUpdate()
    {
        // constantly apply force for the gravity
        rb.AddForce(currentGravityDir * gravityStrength, ForceMode.Acceleration);

        // handle ground checks
        CheckGrounded();
        CheckFreeFall();

        // movement and rotation
        HandleMovementAndRotation();
    }

    private void OnMovementPerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMovementDisabled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void HandleMovementAndRotation()
    {
        Vector3 trueUp = -currentGravityDir;
        if (isOnGround && Physics.Raycast(transform.position, currentGravityDir, out RaycastHit hit, surfaceDetectionDistance, groundLayer))
        {
            trueUp = hit.normal;
        }

        // calculate target rotation for aligning to the surface
        Quaternion targetRotation = transform.rotation;

        if (moveInput.magnitude >= 0.1f)
        {
            // if moving, face the movement direction while keeping the correct UP
            Transform camTransform = Camera.main.transform;
            Vector3 cameraForward = Vector3.ProjectOnPlane(camTransform.forward, trueUp).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(camTransform.right, trueUp).normalized;

            Vector3 movementDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            if (movementDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(movementDirection, trueUp);

                // move the player
                Vector3 targetPosition = rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(targetPosition);
            }
        }
        else
        {
            // if not moving, just correct the tilt
            Vector3 currentForward = Vector3.ProjectOnPlane(transform.forward, trueUp).normalized;
            if (currentForward != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(currentForward, trueUp);
            }
        }

        // Apply rotation smoothly using slerp
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !isOnGround) return;

        rb.AddForce(-currentGravityDir * jumpForce, ForceMode.Impulse);
        isOnGround = false;
    }

    private void OnShowHologram(InputAction.CallbackContext ctx)
    {
        Vector2 dir = ctx.ReadValue<Vector2>();

        Vector3 offsetDirection = Vector3.zero;
        Vector3 hologramUp = Vector3.zero;
        Vector3 hologramForward = transform.forward;

        if (dir.y > 0.5f) // UP ARROW (Targeting Wall in Front)
        {
            offsetDirection = transform.forward;
            hologramUp = -transform.forward;
            hologramForward = transform.up;
        }
        else if (dir.y < -0.5f) // DOWN ARROW (Targeting Wall Behind)
        {
            offsetDirection = -transform.forward;
            hologramUp = transform.forward;
            hologramForward = transform.up;
        }
        else if (dir.x > 0.5f) // RIGHT ARROW (Targeting Wall on Right)
        {
            offsetDirection = transform.right;
            hologramUp = -transform.right;
            hologramForward = transform.forward;
        }
        else if (dir.x < -0.5f) // LEFT ARROW (Targeting Wall on Left)
        {
            offsetDirection = -transform.right;
            hologramUp = transform.right;
            hologramForward = transform.forward;
        }

        if (offsetDirection != Vector3.zero)
        {
            // calculating the head position approximately
            Vector3 headPosition = transform.position + (transform.up * headHeightOffset);
            hologramSystem.ShowHologram(headPosition, offsetDirection, hologramUp, hologramForward);

            // store the selected arrow direction and apply the hologram rotation to player;
            pendingGravityDir = offsetDirection;
            pendingRotation = Quaternion.LookRotation(hologramForward, hologramUp);
            isHologramActive = true;
        }
    }

    private void OnHideHologram(InputAction.CallbackContext ctx)
    {
        isHologramActive = false;
        hologramSystem.HideHologram();
    }

    private void OnGravityChangeReq(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !isHologramActive) return;

        ApplyGravityChange();
    }

    /// <summary>
    /// Triggers the actual gravity shift towards the active holograms direction.
    /// </summary>
    private void ApplyGravityChange()
    {
        // changing current gravity direction to arrow direction
        currentGravityDir = pendingGravityDir;

        // applying the hologram rotation to player via Rigidbody
        rb.rotation = pendingRotation;

        // adding slight push to ensure player leave the current ground
        rb.AddForce(currentGravityDir * 2f, ForceMode.VelocityChange);

        isHologramActive = false;
        hologramSystem.HideHologram();
        isOnGround = false;
    }

    private void CheckGrounded()
    {
        // creates an invisible sphere at the players feet
        Vector3 spherePosition = transform.position + (currentGravityDir * groundCheckOffset);

        // checks if that sphere overlaps with anything on the Ground layer
        isOnGround = Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer);
    }

    private void CheckFreeFall()
    {
        if (!isOnGround)
        {
            // cast a ray far down in the direction of gravity.
            // if it hit nothing, the player fell off the puzzle.
            if (!Physics.Raycast(transform.position, currentGravityDir, maxFallDistance, groundLayer))
            {
                GameManager.Instance.TriggerGameOver(false);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // checking the collided layer using bit operation
        if (((1 << collision.gameObject.layer) & collectibleLayer) != 0)
        {
            GameManager.Instance.AddScore(1);
            Destroy(collision.gameObject);
        }
    }

    private void OnDisable()
    {
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementDisabled;

        input.Player.Jump.performed -= HandleJump;

        input.Player.Hologram.performed -= OnShowHologram;
        input.Player.Hologram.canceled -= OnHideHologram;

        input.Player.GravityChange.performed -= OnGravityChangeReq;

        input.Disable();
    }
}
