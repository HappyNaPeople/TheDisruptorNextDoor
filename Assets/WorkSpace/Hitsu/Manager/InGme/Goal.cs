using UnityEngine;

public class Goal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == UseLayerName.runnerLayer)
        {
            InGame.Instance.ThroughGoal();
        }
    }
}
