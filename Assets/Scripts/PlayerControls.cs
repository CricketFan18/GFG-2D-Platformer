using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float interactRadius = 1.5f;
    private Rigidbody2D rb;
    private Vector2 moveDirection;

    [Header("Input References")]
    public InputActionReference moveRef;
    public InputActionReference jumpRef;
    public InputActionReference skill_1Ref;
    public InputActionReference skill_2Ref;
    public InputActionReference interactRef;
    [HideInInspector] public bool wallJumping = false;
    [HideInInspector] public bool attachedToWall = false;

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
        if(!wallJumping && !attachedToWall) rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
        if(rb.linearVelocity.y < 0.2f) wallJumping = false;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, interactRadius);
        foreach (Collider2D col in overlappingColliders)
        {
            InteractableObject obj = col.GetComponent<InteractableObject>();
            if (obj)
            {
                obj.Interact();
            }
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (Mathf.Abs(rb.linearVelocity.y) < 0.01f) // Only jump if on ground
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            //Debug.Log(gameObject.name + " Jumped!");
        }
    }

    private void OnSkill1(InputAction.CallbackContext context)
    {
        //Debug.Log("Skill 1 Active!");
    }

    private void OnSkill2(InputAction.CallbackContext context)
    {
        //Debug.Log("Skill 2 Active!");
    }
}
