using UnityEngine;

public class WalkDistence_UI : MonoBehaviour
{
    [Header("WalkDistence Object")]
    public SpriteRenderer[] passed_UI;

    public void SpriteChange(int passDistance)
    {
        int hun = passDistance / 100;
        int ten = (passDistance / 10) % 10;
        int one = passDistance % 10;

        passed_UI[0].sprite = GameManager.Instance.numberSprites[hun];
        passed_UI[1].sprite = GameManager.Instance.numberSprites[ten];
        passed_UI[2].sprite = GameManager.Instance.numberSprites[one];

    }


}
