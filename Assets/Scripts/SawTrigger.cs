using UnityEngine;

public class SawTrigger : MonoBehaviour
{
    public ObstacleBase saw;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<PlayerControls>())
        {
            if (saw != null)
            {
                saw.ToggleActive(true);
            }
        }
        Destroy(this.gameObject);
    }
}