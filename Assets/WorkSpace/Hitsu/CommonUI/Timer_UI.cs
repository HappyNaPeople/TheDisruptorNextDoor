using UnityEngine;

public class Timer_UI : MonoBehaviour
{
    [Header("Timer Object")]
    public Material timerBar;
    public SpriteRenderer[] timer_numbers;

    public void SpriteChange(int timerTime)
    {
        int hun = timerTime / 100;
        int ten = (timerTime / 10) % 10;
        int one = timerTime % 10;

        timer_numbers[0].sprite = GameManager.Instance.numberSprites[hun];
        timer_numbers[1].sprite = GameManager.Instance.numberSprites[ten];
        timer_numbers[2].sprite = GameManager.Instance.numberSprites[one];

    }

}
