using UnityEngine;

public abstract class ObstacleBase : MonoBehaviour,IObstacle
{
    [Header("Obstacle Settings")]
    [Tooltip("Damage dealt to the player")]
    public int damage = 1;
    [Tooltip("Force to push the player back")]
    public float knockbackForce = 5f;

    public abstract void AffectPlayer(GameObject player);
    public virtual void ToggleActive(bool state)
    {
        gameObject.SetActive(state);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object hitting us is on the "Player" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            AffectPlayer(collision.gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            AffectPlayer(other.gameObject);
        }
    }
    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        // Child classes can override this
    }

}
