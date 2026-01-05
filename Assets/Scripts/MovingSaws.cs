using UnityEngine;

public class MovingSaws : ObstacleBase
{
    [Header("Movement Settings")]
    [Tooltip("If true, the saw starts moving immediately on Start.")]
    public bool moveOnStart = true;
    public float moveSpeed = 10f;
    public bool oscillate = true;

    [Header("Path Settings")]
    public Vector2 targetOffset;
    public bool useWorldSpace = false;
    public float waitAtEnds = 0f;
    public float reachThreshold = 0.05f;

    private Vector2 startPos;
    private Vector2 finalPos;
    private Vector2 currentTarget;

    private bool isMoving = false;
    private float waitTimer = 0f;
    private bool movingToFinalPos = true;

    private void Start()
    {
        InitializePath();

        if (moveOnStart)
        {
            isMoving = true;
        }
    }

    private void Update()
    {
        HandleMovement();
    }

    private void InitializePath()
    {
        startPos = transform.position;
        finalPos = useWorldSpace ? targetOffset : startPos + targetOffset;
        currentTarget = finalPos;
        movingToFinalPos = true;
    }

    private void HandleMovement()
    {
        if (!isMoving) return;
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }
        transform.position = Vector2.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, currentTarget) <= reachThreshold)
        {
            OnTargetReached();
        }
    }

    private void OnTargetReached()
    {
        if (waitAtEnds > 0)
        {
            waitTimer = waitAtEnds;
        }
        if (oscillate) // Flip direction
        {
            movingToFinalPos = !movingToFinalPos;
            currentTarget = movingToFinalPos ? finalPos : startPos;
        }
        else  // Stop if not oscillating
        {
            isMoving = false;
        }
    }

    public override void AffectPlayer(GameObject player)
    {
        PlayerStats ps = player.GetComponent<PlayerStats>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (ps != null)
        {
            ps.TakeDamage(damage);
        }

        if (rb)
        {
            Vector2 pushDir = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(pushDir * knockbackForce, ForceMode2D.Impulse);
        }
        
    }

    public override void ToggleActive(bool state) // This allows external scripts (like triggers/buttons) to start/stop the saw
    {
        base.ToggleActive(state);
        isMoving = state;
    }

    // ---------------------------------------------------------
    // EDITOR VISUALIZATION
    // ---------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        // Visualize the path in the Editor so you don't have to guess
        Gizmos.color = Color.red;
        Vector2 from = Application.isPlaying ? startPos : (Vector2)transform.position;
        Vector2 to = useWorldSpace ? targetOffset : from + targetOffset;

        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(to, 0.2f); // Draw circle at destination
        Gizmos.DrawWireSphere(from, 0.2f); // Draw circle at start
    }
}