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
    private Color initialColor;

    void Start()
    {
        currentHealth = maxHealth;
        respawnPoint = transform.position; // Assume we start where we placed them in the scene
        movementScript = GetComponent<PlayerControls>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        initialColor = spriteRenderer.color;
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
        currentHealth = maxHealth;
        transform.position = respawnPoint;
        Rigidbody2D rb = GetComponent<Rigidbody2D>(); // Reset Physics (Stop them from flying if they died mid-air)
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void SetRespawnPoint(Vector2 newPoint) // function to update the respawn point when touching a Checkpoint
    {
        respawnPoint = newPoint;
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = initialColor;
        }
    }
}