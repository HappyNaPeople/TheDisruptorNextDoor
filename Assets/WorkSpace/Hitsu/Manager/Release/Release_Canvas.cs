using UnityEngine;
using System.Collections;



[System.Serializable]
public class Release_PlayerResultData
{
    public PlayerData playerData { get; private set; }

    public void Init(PlayerData targetData)
    {
        playerData = targetData;

        distanceShow.SetActive(false);

    }

    public SpriteRenderer[] time;

    public GameObject distanceShow;
    public SpriteRenderer[] distance;

    public Transform player_Start;
    public Transform player_Goal;
    public Transform player_Hard;

    private void ShowTimeData(float targetTime)
    {
        int min = Mathf.FloorToInt(targetTime / 60.0f);
        int sec = Mathf.FloorToInt(targetTime % 60.0f);

        string timeResult = $"{min:00}{sec:00}";
        for (int i = 0; i < time.Length; i++)
        {
            int number = timeResult[i] - '0';
            time[i].sprite = GameManager.Instance.numberSprites[number];
        }
    }

    public IEnumerator TimeResult()
    {
        float endTime = playerData.passTime;
        float timer = 0.0f;
        int oldTime = -1;

        while (timer < endTime)
        {
            int currentTime = Mathf.FloorToInt(timer);

            if (currentTime > oldTime)
            {
                ShowTimeData(currentTime);
                oldTime = currentTime;
            }

            timer += Time.deltaTime * 10;
            yield return null;
        }

        ShowTimeData(endTime);

    }

    private void ShowDistanceData(float targetDistance)
    {
        int distanceValue = Mathf.RoundToInt(targetDistance * 10.0f);
        string distanceResult = distanceValue.ToString("0000");
        for (int i = 0; i < distance.Length; i++)
        {
            int number = distanceResult[i] - '0';
            distance[i].sprite = GameManager.Instance.numberSprites[number];
        }
    }

    public IEnumerator DistanceResult()
    {

        float endDistance = playerData.passDistance;

        float offset = Vector3.Distance(player_Start.localPosition, player_Goal.localPosition);
        offset /= 100;
        float targetDistance = (endDistance / Release.gameLimitDistance) * 100;
        offset *= targetDistance;
        Vector3 targetLocalPosition = player_Start.localPosition + Vector3.right * offset;

        player_Hard.localPosition = player_Start.localPosition;

        float timer = 0.0f;
        float duration = 1.0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress = timer / duration;
            player_Hard.localPosition = Vector3.Lerp(player_Start.localPosition, targetLocalPosition, progress);

            yield return null;
        }


        player_Hard.localPosition = targetLocalPosition;

        ShowDistanceData(endDistance);
        distanceShow.SetActive(true);

    }
}



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
    public Release_PlayerResultData player01Result;
    public SpriteRenderer player01_Crown;

    [Header("Player02 Result")]
    public Release_PlayerResultData player02Result;
    public SpriteRenderer player02_Crown;

    [Header("Button")]
    public GameObject button_RePlay;
    public GameObject button_BackToTitle;



    private Winner thisPlayer;
    private bool isWin => thisPlayer == Release.Instance.winner;

    private IEnumerator ResultShow()
    {
        button_RePlay.SetActive(false);
        button_BackToTitle.SetActive(false);


        yield return StartCoroutine(player01Result.TimeResult());
        yield return null;
        yield return StartCoroutine(player01Result.DistanceResult());
        yield return null;
        yield return StartCoroutine(player02Result.TimeResult());
        yield return null;
        yield return StartCoroutine(player02Result.DistanceResult());


        yield return new WaitForSeconds(2);

        winnerResult.sprite = isWin ? Release.Instance.resultLogo_won : Release.Instance.resultLogo_lose;
        backGround.sprite = isWin ? Release.Instance.backGround_won : Release.Instance.backGround_lose;

        if(Release.Instance.winner == Winner.Player01) player02_Crown.sprite = null;
        else player01_Crown.sprite = null;

        yield return new WaitForSeconds(1);

        button_RePlay.SetActive(true);
        button_BackToTitle.SetActive(true);

    }


    public void Init(Winner targetPlayer)
    {
        thisPlayer = targetPlayer;
        player01Result.Init(GameManager.Instance.player01.playerData);
        player02Result.Init(GameManager.Instance.player02.playerData);

        StartCoroutine(ResultShow());
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