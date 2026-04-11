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
            InGame.Instance.PassCheckPoint(this.transform);
        }
    }

}
