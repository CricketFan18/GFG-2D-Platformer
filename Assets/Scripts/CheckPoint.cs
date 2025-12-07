using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // Set the player's new respawn point to this object's location
                stats.SetRespawnPoint(transform.position);
                Debug.Log("Checkpoint Reached!");
            }
        }
    }
}