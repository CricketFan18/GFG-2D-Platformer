using UnityEngine;

public class SpikeMech : MonoBehaviour,ObstacleBase
{
    public float speed;
    Vector3 targetPos;

    public GameObject ways;
    public Transform[] wayPoints;
    int pointIndex;
    int pointCount;
    int direction = 1;

    private void Awake()
    {
        wayPoints = new Transform[ways.transform.childCount];
        for (int i = 0; i < ways.gameObject.transform.childCount; i++)
        {
            wayPoints[i] = ways.transform.GetChild(i).gameObject.transform;
        }
    }

    private void Start()
    {
        pointCount = wayPoints.Length;
        pointIndex = 1;
        targetPos = wayPoints[pointIndex].transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (transform.position == targetPos)
        {
            NextPoint();
        }
    }

    public override void AffectPlayer(GameObject player)
    {
        // 1. Try to find the Stats Script on the object we hit
        PlayerStats health = player.GetComponent<PlayerStats>();

        // 2. If we found it, deal damage
        if (health != null)
        {
            health.TakeDamage(damageAmount);
        }

        // 3. Apply Knockback (Optional, keeps them from sitting on the spike)
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 pushDir = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = Vector2.zero; // Stop momentum
            rb.AddForce(pushDir * knockbackForce, ForceMode2D.Impulse);
        }
    }
    void NextPoint()
    {
        if(pointIndex == pointCount - 1)
        {
            direction = -1;
        }

        if(pointIndex == 0)
        {
            direction = 1;
        }

        pointIndex += direction;
        targetPos = wayPoints[pointIndex].transform.position;
    }
}
