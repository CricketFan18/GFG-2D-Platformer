using UnityEngine;

public interface IObstacle
{
    void AffectPlayer(GameObject player);
    void ToggleActive(bool state);
}
