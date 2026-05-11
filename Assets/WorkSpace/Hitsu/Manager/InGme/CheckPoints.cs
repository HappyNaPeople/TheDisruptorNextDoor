using UnityEngine;
using System.Collections;


public enum CheckPointIndex
{
    CheckPoint01 = 0,
    CheckPoint02,
    CheckPoint03
}


public class CheckPoints : MonoBehaviour
{
    public CheckPointIndex checkPointIndex;
    public GameObject effectPrefab;
    public float effectTime = 1.5f;
    public bool through;

    public void AnimationControl(bool isPlay)
    {
        through = isPlay;
        GetComponent<Animator>().SetBool("Through", through);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == UseLayerName.runnerLayer && !through)
        {
            AnimationControl(true);
            if (effectPrefab != null) 
            {
                GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, effectTime);

            }
            InGame.Instance.PassCheckPoint(this.transform);
        }
    }

}
