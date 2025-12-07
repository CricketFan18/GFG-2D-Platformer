using UnityEngine;

public class SpikeMech : ObstacleBase
{
    public float speed;
    Vector3 targetPos;

    public GameObject ways;
    public Transform[] wayPoints;
    int pointIndex;
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
        Debug.Log("Affected " + player.name);
        //// 1. Try to find the Stats Script on the object we hit
        //PlayerStats health = player.GetComponent<PlayerStats>();

        //// 2. If we found it, deal damage
        //if (health != null)
        //{
        //    health.TakeDamage(damage);
        //}

        //// 3. Apply Knockback (Optional, keeps them from sitting on the spike)
        //Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        //if (rb != null)
        //{
        //    Vector2 pushDir = (player.transform.position - transform.position).normalized;
        //    rb.linearVelocity = Vector2.zero; // Stop momentum
        //    rb.AddForce(pushDir * knockbackForce, ForceMode2D.Impulse);
        //}
    }
    void NextPoint()
    {
        pointIndex = pointIndex == 1 ? 0 : 1;
        targetPos = wayPoints[pointIndex].transform.position;
    }
}
