using UnityEngine;

public class MovingSaws : ObstacleBase
{
    public Vector2 targetPosition;
    public bool targetInWorldSpace = false;
    public bool oscillate = true;
    public float moveSpeed = 10;
    public float waitAtEnd = 0;
    public float reachThreshold = 0.05f;

    public bool isStatic = true;
    public Transform activationTrigger;

    Vector2 startPos;
    Vector2 finalPos;
    bool isMoving = false;
    bool reachedTarget = false;
    float waitCounter = 0f;

    void Start()
    {
        startPos = transform.position;

        finalPos = (targetInWorldSpace)?  targetPosition : startPos + targetPosition;

        isMoving = isStatic;
    }

    void Update()
    {
        MoveSaw();
    }

    public void MoveSaw()
    {
        if (isMoving == false || isStatic)
        {
            return;
        }

        if (waitCounter > 0)
        {
            waitCounter = waitCounter - Time.deltaTime;
            return;
        }

        Vector2 currentPosition = transform.position;
        Vector2 destination;

        destination = (reachedTarget)? finalPos : startPos;

        Vector2 newp = Vector2.MoveTowards(currentPosition, destination, moveSpeed * Time.deltaTime);
        transform.position = newp;

        float distance = Vector2.Distance(newp, destination);

        if (distance <= reachThreshold)
        {
            if (waitAtEnd > 0)
            {
                waitCounter = waitAtEnd;
            }

            if (oscillate == true)
            {
                reachedTarget = !reachedTarget;
            }
            else
            {
                isMoving = false;
            }
        }
    }

    public override void AffectPlayer(GameObject player)
    {
        PlayerStats ps = player.GetComponent<PlayerStats>();
        if (ps != null)
        {
            ps.TakeDamage(damage);
        }

        // ps.transform.position = new Vector3(8, -4.8f, 0); //TODO: Respawn to Checkpoint
    }

    public override void ToggleActive(bool s)
    {
        base.ToggleActive(s);
        isMoving = s;
    }
}


