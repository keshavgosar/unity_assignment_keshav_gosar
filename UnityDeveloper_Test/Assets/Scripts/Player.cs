using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // using new input actions for player input
    public PlayerInput_Actions input { get; private set; }

    [Header("Animations")]
    private Animator anim;

    [Header("Ground Layer")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Input Settings")]
    private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    private float rotationSpeed = 10f;
    private bool isOnGround;

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

        // force the player to stay perfectly straight against the wall every frame
        AlignToGravity();

        HandleMovement();
    }

    private void OnMovementPerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMovementDisabled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void HandleMovement()
    {
        if (moveInput.magnitude < 0.1f) return;

        // true up direction based on current gravity
        Vector3 trueUp = -currentGravityDir;

        Transform camTransform = Camera.main.transform;

        // project movement onto the gravity plane
        Vector3 cameraForward = Vector3.ProjectOnPlane(camTransform.forward, trueUp).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(camTransform.right, trueUp).normalized;

        Vector3 movementDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        if (movementDirection != Vector3.zero)
        {
            // force the player to stand perfectly straight relative to the gravity wall
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection, trueUp);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

            Vector3 targetPosition = rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }

    private void AlignToGravity()
    {
        // absolute up vector based on current gravity
        Vector3 trueUp = -currentGravityDir;

        // take where the player is currently looking, but flatten it perfectly against the gravity wall
        Vector3 flattenedForward = Vector3.ProjectOnPlane(transform.forward, trueUp).normalized;

        if (flattenedForward != Vector3.zero)
        {
            // eliminating the tilt by forcing rotation
            Quaternion perfectlyUprightRotation = Quaternion.LookRotation(flattenedForward, trueUp);

            rb.MoveRotation(perfectlyUprightRotation);
        }
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !isOnGround) return;

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isOnGround = true;
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
