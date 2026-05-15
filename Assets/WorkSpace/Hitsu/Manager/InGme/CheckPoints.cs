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
    public Vector2 effectOffset;
    public bool through;
    public SfxData sfxData;

    public Animator flagAnimator;

    public void AnimationControl(bool isPlay)
    {
        through = isPlay;
        flagAnimator.SetBool("Through", through);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == UseLayerName.runnerLayer && !through)
        {
            AnimationControl(true);
            if (effectPrefab != null) 
            {
                GameObject effect = Instantiate(effectPrefab, transform.position + (Vector3)effectOffset, Quaternion.identity);
                AudioManager.Instance.PlaySfx(sfxData);

            }
            InGame.Instance.PassCheckPoint(this.transform);
        }
    }

}
