using UnityEngine;

public abstract class ObstacleBase : MonoBehaviour,IObstacle
{
    [Header("Obstacle Settings")]
    [Tooltip("Damage dealt to the player")]
    public int damage = 1;

    [Tooltip("Force to push the player back")]
    public float knockbackForce = 5f;

    public abstract void AffectPlayer(GameObject player);

    // Structure for your concrete function implementation of AffectPlayer()
    //public override void AffectPlayer(GameObject player)
    //{
    //    // 1. Try to find the Stats Script on the object we hit
    //    PlayerStats health = player.GetComponent<PlayerStats>();

    //    // 2. If we found it, deal damage
    //    if (health != null)
    //    {
    //        health.TakeDamage(damageAmount);
    //    }

    //    // 3. Apply Knockback (Optional, keeps them from sitting on the spike)
    //    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
    //    if (rb != null)
    //    {
    //        Vector2 pushDir = (player.transform.position - transform.position).normalized;
    //        rb.linearVelocity = Vector2.zero; // Stop momentum
    //        rb.AddForce(pushDir * knockbackForce, ForceMode2D.Impulse);
    //    }
    //}

    public virtual void ToggleActive(bool state)
    {
        gameObject.SetActive(state);
    }

    // This saves you from writing OnCollisionEnter2D in every new script
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object hitting us is on the "Player" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            AffectPlayer(collision.gameObject);
        }
    }

    // Also handle Triggers (in case you set IsTrigger = true)
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            AffectPlayer(other.gameObject);
        }
    }
}
