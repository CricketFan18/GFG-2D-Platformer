using UnityEngine;

public class SpikeMech : ObstacleBase
{
    [Header("Movement Settings")]
    public float speed = 5f;
    [Tooltip("How close the object must get to the waypoint before switching.")]
    public float reachThreshold = 0.1f;

    [Header("Path References")]
    [Tooltip("Drag the parent GameObject holding the waypoint transforms here.")]
    public Transform pathContainer;
    public bool startOnAwake = true;

    private Transform[] wayPoints;
    private int targetIndex = 0;
    private Vector3 currentTarget;

    private void Awake()
    {
        InitializePath();
        ToggleActive(startOnAwake);
    }

    private void Start()
    {
        if (wayPoints.Length > 0)
        {
            // Start moving towards the first point (or the second if we act as the first)
            // This setup assumes the object starts physically at wayPoints[0]
            targetIndex = 1 % wayPoints.Length;
            currentTarget = wayPoints[targetIndex].position;
        }
    }

    private void Update()
    {
        if (wayPoints.Length == 0) return;

        MovePlatform();
    }

    private void InitializePath()
    {
        if (pathContainer == null)
        {
            enabled = false;
            return;
        }
        int childCount = pathContainer.childCount;
        wayPoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            wayPoints[i] = pathContainer.GetChild(i);
        }
    }

    private void MovePlatform()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentTarget) <= reachThreshold)
        {
            NextPoint();
        }
    }

    private void NextPoint()
    {
        targetIndex = (targetIndex + 1) % wayPoints.Length; // Cycle through points: 0 -> 1 -> 2 -> 0 ...
        currentTarget = wayPoints[targetIndex].position;
    }

    public override void AffectPlayer(GameObject player)
    {
        Debug.Log("player collided");
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(damage);
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 pushDir = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(pushDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    // Helps you see the path in the Editor
    private void OnDrawGizmos()
    {
        if (pathContainer == null || pathContainer.childCount < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < pathContainer.childCount; i++)
        {
            Transform current = pathContainer.GetChild(i);
            Transform next = pathContainer.GetChild((i + 1) % pathContainer.childCount);

            if (current && next)
                Gizmos.DrawLine(current.position, next.position);
        }
    }
}