using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Pause Settings")]
    [SerializeField] private float pauseTime = 1f; // Pause at edges

    [Header("Movement Axis")]
    [SerializeField] private bool moveHorizontally = true;

    private Vector3 startPosition;
    private float direction = 1f;
    private float pauseTimer;
    private bool isPaused;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (isPaused)
        {
            HandlePause();
            return;
        }

        MovePlatform();
        CheckEdge();
    }

    private void MovePlatform()
    {
        float movement = moveSpeed * Time.deltaTime * direction;

        if (moveHorizontally)
            transform.Translate(Vector3.right * movement);
        else
            transform.Translate(Vector3.up * movement);
    }

    private void CheckEdge()
    {
        float offset = moveHorizontally
            ? transform.position.x - startPosition.x
            : transform.position.y - startPosition.y;

        if (Mathf.Abs(offset) >= moveDistance)
        {
            isPaused = true;
            pauseTimer = pauseTime;
        }
    }

    private void HandlePause()
    {
        pauseTimer -= Time.deltaTime;

        if (pauseTimer <= 0f)
        {
            direction *= -1f;
            isPaused = false;
        }
    }

    // Optional: player stick to platform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            collision.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            collision.transform.SetParent(null);
    }
}
