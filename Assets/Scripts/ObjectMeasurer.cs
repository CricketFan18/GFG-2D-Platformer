using UnityEngine;

public class ObjectMeasurer : MonoBehaviour
{
    void Start()
    {
        Renderer objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            Vector3 objectSize = objectRenderer.bounds.size;
            Debug.Log($"Object size: X={objectSize.x}, Y={objectSize.y}, Z={objectSize.z}");
        }
        else
        {
            Debug.LogWarning("No Renderer found on this GameObject.");
        }
    }
}