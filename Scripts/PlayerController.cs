using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float groundDrag = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private CharacterController controller;
    private Vector3 moveInput;
    private Vector3 velocity;
    private bool jumpRequested = false;
    private bool isGrounded = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Default");
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpRequested = true;
            Debug.Log($"Jump input pressed - Currently grounded: {isGrounded}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Basic grounded check via CharacterController
        isGrounded = controller.isGrounded;

        // If CharacterController reports not grounded, try a short raycast slightly below the controller
        if (!isGrounded)
        {
            RaycastHit hit;
            // Raycast from just above the bottom of controller bounds
            Vector3 rayStart = controller.bounds.center + Vector3.up * 0.1f;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, controller.bounds.extents.y + groundCheckDistance, groundLayer))
            {
                isGrounded = true;
            }
        }

        // Apply gravity. When grounded, keep a small downward bias to keep the controller snapped to ground.
        if (isGrounded && velocity.y < 0f)
        {
            // small negative value keeps the controller in contact without penetrating the ground
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Handle jump
        if (jumpRequested && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isGrounded = false;
            jumpRequested = false;
        }
        else if (jumpRequested && !isGrounded)
        {
            // Not grounded, consume the jump input
            jumpRequested = false;
        }

        // Handle movement -- combine horizontal and vertical movement into a single Move call
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y) * speed;
        Vector3 final = move + new Vector3(0f, velocity.y, 0f);
        controller.Move(final * Time.deltaTime);
    }

    // Public helper to reset internal movement state after teleport/respawn
    public void ResetMovementState()
    {
        moveInput = Vector3.zero;
        velocity = Vector3.zero;
        jumpRequested = false;
        // Also clear any pending input from the new Input System by ensuring no movement applied next frame
        Debug.Log("Player movement state reset after respawn.");
    }
}
