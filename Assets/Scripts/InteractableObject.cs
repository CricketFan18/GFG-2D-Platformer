using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    public UnityEvent onInteract;
    public void Interact()
    {
        Debug.Log("InteractInvoked");
        onInteract.Invoke();
    }
}
