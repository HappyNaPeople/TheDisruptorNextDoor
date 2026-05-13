using UnityEngine;



public class Release_Canvas : MonoBehaviour
{
    public enum Option
    {
        None,
        Replay,
        BackToTitle,
        QuitTheGame
    }

    public Option option;

    public SpriteRenderer backGround;
    public SpriteRenderer winnerResult;

    [Header("Player01 Result")]
    public SpriteRenderer[] player01_Time;
    public SpriteRenderer[] player01_Distance;
    public SpriteRenderer player01_Crown;

    [Header("Player02 Result")]
    public SpriteRenderer[] player02_Time;
    public SpriteRenderer[] player02_Distance;
    public SpriteRenderer player02_Crown;


    private Winner thisPlayer;
    private bool isWin => thisPlayer == Release.Instance.winner;

    private void Player01DataShow()
    {
        PlayerData _player01Result = GameManager.Instance.player01.playerData;

        int min = Mathf.FloorToInt(_player01Result.passTime / 60.0f);
        int sec = Mathf.FloorToInt(_player01Result.passTime % 60.0f);

        string timeResult = $"{min:00}{sec:00}";
        for (int i = 0; i < player01_Time.Length; i++)
        {
            int number = timeResult[i] - '0';
            player01_Time[i].sprite = GameManager.Instance.numberSprites[number];
        }

        int distanceValue = Mathf.RoundToInt(_player01Result.passDistance * 10.0f);
        string distanceResult = distanceValue.ToString("0000");
        for (int i = 0; i < player01_Distance.Length; i++)
        {
            int number = distanceResult[i] - '0';
            player01_Distance[i].sprite = GameManager.Instance.numberSprites[number];
        }

    }
    private void Player02DataShow()
    {
        PlayerData _player02Result = GameManager.Instance.player02.playerData;

        int min = Mathf.FloorToInt(_player02Result.passTime / 60.0f);
        int sec = Mathf.FloorToInt(_player02Result.passTime % 60.0f);

        string timeResult = $"{min:00}{sec:00}";
        for (int i = 0; i < player02_Time.Length; i++)
        {
            int number = timeResult[i] - '0';
            player02_Time[i].sprite = GameManager.Instance.numberSprites[number];
        }

        int distanceValue = Mathf.RoundToInt(_player02Result.passDistance * 10.0f);
        string distanceResult = distanceValue.ToString("0000");
        for (int i = 0; i < player02_Distance.Length; i++)
        {
            int number = distanceResult[i] - '0';
            player02_Distance[i].sprite = GameManager.Instance.numberSprites[number];
        }
    }

    private void ResultShow()
    {
        //winnerResult.sprite = isWin ? Release.Instance.your : Release.Instance.others;

        backGround.sprite = isWin ? Release.Instance.backGround_won : Release.Instance.backGround_lose;

        Player01DataShow();
        Player02DataShow();

        backGround.sprite = isWin ? Release.Instance.backGround_won : Release.Instance.backGround_lose;

        if(Release.Instance.winner == Winner.Player01) player02_Crown.sprite = null;
        else player01_Crown.sprite = null;

    }


    public void Init(Winner targetPlayer)
    {
        thisPlayer = targetPlayer;
        ResultShow();
    }

    public void ResetOption()
    {
        option = Option.None;
    }
    public void Button_BackToTitle()
    {
        if (option != Option.None) return;
        option = Option.BackToTitle;
    }
    public void Button_RePlay()
    {
        if (option != Option.None) return;
        option = Option.Replay;
    }
    public void Button_QuitTheGame()
    {
        if (option != Option.None) return;
        option = Option.QuitTheGame;
    }
}