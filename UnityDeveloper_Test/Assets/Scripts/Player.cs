using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // using new input actions for player input
   public PlayerInput_Actions input {  get; private set; }

    [Header("Animations")]
    private Animator anim;

    [Header("Ground Layer")]
    [SerializeField] private LayerMask groundLayer;

    private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    private float rotationSpeed = 10f;
    private bool isOnGround;

    [Header("Components")]
    private Rigidbody rb;

    private void Awake()
    {
        input = new PlayerInput_Actions();

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
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
    }

    private void Update()
    {
        float currentSpeed = moveInput.magnitude;
        anim.SetFloat("Speed", currentSpeed);

        anim.SetBool("isGrounded", isOnGround);
    }

    private void FixedUpdate()
    {
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

        // using camera forward and right vector to get the move direction and rotating the character;
        Transform camTransform = Camera.main.transform;

        Vector3 cameraForward = Vector3.ProjectOnPlane(camTransform.forward, transform.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(camTransform.right, transform.up).normalized;

        Vector3 movementDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        // movement and player rotation
        if (movementDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection, transform.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

            Vector3 targetPosition = rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !isOnGround) return;

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        isOnGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isOnGround = true;
        }
    }

    private void OnDisable()
    {
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementDisabled;

        input.Player.Jump.performed -= HandleJump;

        input.Disable();
    }
}
