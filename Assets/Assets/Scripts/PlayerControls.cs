using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Rigidbody2D rb;
    private Vector2 moveDirection;

    [Header("Input References")]
    public InputActionReference moveRef;
    public InputActionReference jumpRef;
    public InputActionReference skill_1Ref;
    public InputActionReference skill_2Ref;
    public InputActionReference interactRef;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        if (moveRef) moveRef.action.Enable();
        if (jumpRef) jumpRef.action.Enable();
        if (interactRef) interactRef.action.Enable();
        if (skill_1Ref) skill_1Ref.action.Enable();
        if (skill_2Ref) skill_2Ref.action.Enable();

        jumpRef.action.performed += OnJump;
        interactRef.action.performed += OnInteract;
        skill_1Ref.action.performed += OnSkill1;
        skill_2Ref.action.performed += OnSkill2;

    }

    void OnDisable() // Removing them is necessary else the gameObject will have additional functions
    {
        jumpRef.action.performed -= OnJump;
        interactRef.action.performed -= OnInteract;
        skill_1Ref.action.performed -= OnSkill1;
        skill_2Ref.action.performed -= OnSkill2;
        moveRef.action.Disable();
    }

    void Update()
    {
        moveDirection = moveRef.action.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Pressed Down");
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (Mathf.Abs(rb.linearVelocity.y) < 0.01f) // Only jump if on ground
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log(gameObject.name + " Jumped!");
        }
    }

    private void OnSkill1(InputAction.CallbackContext context)
    {
        Debug.Log("Skill 1 Active!");
    }

    private void OnSkill2(InputAction.CallbackContext context)
    {
        Debug.Log("Skill 2 Active!");
    }
}
