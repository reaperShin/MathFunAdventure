using UnityEngine;
using UnityEngine.InputSystem;

public class input : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField] private float _movementSpeed = 10f;
    [SerializeField] private float _jumpHeight = 8f;
    [SerializeField] private float _gravity = 8f;
    [SerializeField] private int lives;
    private float velocityY;
    private Animator animator;
    private Vector2 moveInput;   // stores input from new system
    private bool jumpPressed;

    private PlayerInputActions inputs;
    void Awake()
    {
        // Create and enable the input actions
        inputs = new PlayerInputActions();
        inputs.Gameplay.Enable();

        // Subscribe to actions
        inputs.Gameplay.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputs.Gameplay.Movement.canceled += ctx => moveInput = Vector2.zero;

        inputs.Gameplay.Jump.performed += ctx => jumpPressed = true;
        inputs.Gameplay.Jump.canceled += ctx => jumpPressed = false;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Horizontal movement (X axis only, extend to Z if needed)
        Vector3 direction = new Vector3(moveInput.x, 0, 0);

        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        // Jump & gravity
        if (controller.isGrounded)
        {
            velocityY = -1f; // keep grounded
            if (jumpPressed)
            {
                velocityY = _jumpHeight;
                animator.SetTrigger("Jumping");
                if (AudioManager.Instance != null) AudioManager.Instance.PlayJump();
                jumpPressed = false; // consume jump
            }
        }
        else
        {
            velocityY -= _gravity * Time.deltaTime;
        }

        // Final movement vector
        Vector3 velocity = direction * _movementSpeed;
        velocity.y = velocityY;

        controller.Move(velocity * Time.deltaTime);

        // Animator
        bool isMoving = direction != Vector3.zero;
        animator.SetBool("IsRunning", isMoving);
    }

    void OnDestroy()
    {
        // Clean up subscriptions
        inputs.Gameplay.Movement.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        inputs.Gameplay.Movement.canceled -= ctx => moveInput = Vector2.zero;
        inputs.Gameplay.Jump.performed -= ctx => jumpPressed = true;
        inputs.Gameplay.Jump.canceled -= ctx => jumpPressed = false;

        inputs.Gameplay.Disable();
    }
}
