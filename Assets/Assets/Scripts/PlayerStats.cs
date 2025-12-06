using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Respawn")]
    public Vector2 respawnPoint;

    private PlayerControls movementScript;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;

        // Assume we start where we placed them in the scene
        respawnPoint = transform.position;

        movementScript = GetComponent<PlayerControls>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took damage! Health: {currentHealth}");

        // Optional: Visual feedback (Flash Red)
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} Died!");
        Respawn();
    }

    public void Respawn()
    {
        // 1. Reset Health
        currentHealth = maxHealth;

        // 2. Teleport back to start (or last checkpoint)
        transform.position = respawnPoint;

        // 3. Reset Physics (Stop them from flying if they died mid-air)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // function to update the respawn point when touching a Checkpoint
    public void SetRespawnPoint(Vector2 newPoint)
    {
        respawnPoint = newPoint;
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white; // Or whatever your original color was
        }
    }
}