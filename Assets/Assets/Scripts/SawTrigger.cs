using UnityEngine;

public class SawTrigger : MonoBehaviour
{
    public MovingSaws saw;

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