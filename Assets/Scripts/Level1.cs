using System.Collections;
using UnityEngine;

public class Level1 : MonoBehaviour
{
    //All the events and behaviours unique to level1 to be implemented here

    public Transform[] lastObstacles;

    public void openLastArea()
    {
        IEnumerator scaleDown()
        {
            float duration = 1f;
            float elapsed = 0f;
            Vector3[] startScales = new Vector3[lastObstacles.Length];
            for (int i = 0; i < lastObstacles.Length; i++)
                startScales[i] = lastObstacles[i].localScale;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                for (int i = 0; i < lastObstacles.Length; i++)
                {
                    Vector3 s = startScales[i];
                    lastObstacles[i].localScale = new Vector3(s.x,Mathf.Lerp(s.y, 0f, t),s.z);
                }
                yield return null;
            }
        }

        StartCoroutine(scaleDown());
    }

    public void LevelCompleted()
    {
        Debug.Log("Level Completed");
        ///Level Completion Logic here
    }
}
