using UnityEngine;

public class Goal : MonoBehaviour
{
    public GameObject effectPrefab;
    public float effectTime = 1.5f;
    public SfxData sfxData;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == UseLayerName.runnerLayer)
        {
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, effectTime);
                AudioManager.Instance.PlaySfx(sfxData);
            }
            InGame.Instance.ThroughGoal();
        }
    }
}
