using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;


public class ChoseTrapData
{
    public TrapName trapName;
    public uint trapCount;
}

public class TitleCanvas : MonoBehaviour
{
    [Header("プレイヤー情報")]
    public Player targetPlayer;
    public TMP_Text costText;
    public TMP_Text timerText;
    public bool isPlayerReady = false;

    private int nowCost = Backpack.maxCost;
    private const int timerLimit = 10;
    private float timer = timerLimit;

    private IEnumerator Timer()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            timerText.text = $"{(int)timer:D2}";
            yield return null;
        }
        isPlayerReady = true;
    }

    public void ChoseTrapToBackpack(out bool isDone)
    {
        if (playerTrap.Count == 0)
        {
            isDone = false;
            return;
        }

        foreach (ChoseTrapData trap in playerTrap)
        {
            targetPlayer.hunter.backpack.AddToBackpack(trap.trapName);
        }

        isDone = targetPlayer.hunter.backpack.trapsPack.Count == playerTrap.Count;
    }

    [Header("選んだ罠")]
    public List<TrapUI> choseTrap = new List<TrapUI>();

    private List<ChoseTrapData> playerTrap = new List<ChoseTrapData>();
    private int showChoseTrapPageLimit => playerTrap.Count / choseTrap.Count;
    private int choseTrapNowPage = 0;

    private void UpdateChoseTrap()
    {
        costText.text = $"Cost Left : {nowCost:D2}";
        int trapIndex = choseTrapNowPage * showChoseTrapPageLimit;
        int choseTrapMax = playerTrap.Count > choseTrap.Count ? choseTrap.Count : playerTrap.Count;

        for (int index = 0; index < choseTrap.Count; index++)
        {
            choseTrap[index].gameObject.SetActive(false);

            if(trapIndex < playerTrap.Count)
            {
                choseTrap[index].SetTrap(playerTrap[trapIndex]);
                choseTrap[index].gameObject.SetActive(true);
                trapIndex++;
            }

        }
    }
    private void AddToPlayerTrap(TrapName targetTrap)
    {
        int checkCost = nowCost - CanUseTrap.allTrap[targetTrap].cost;
        if (checkCost < 0) return;
        nowCost = checkCost;

        ChoseTrapData trap = playerTrap.Find(t => t.trapName == targetTrap);
        if (trap == null)
        {
            playerTrap.Add(new ChoseTrapData()
            {
                trapName = targetTrap,
                trapCount = 1
            });
        }
        else trap.trapCount += 1;

        UpdateChoseTrap();
    }

    //public void Button_ChoseTrapNextPage()
    //{
    //    int nextPage = choseTrapNowPage + 1;
    //    if (nextPage > showChoseTrapPageLimit) return;
    //    else choseTrapNowPage = nextPage;
    //    UpdateChoseTrap();
    //}
    //public void Button_ChoseTrapBackPage()
    //{
    //    int backPage = choseTrapNowPage - 1;
    //    if (backPage < 0) return;
    //    else choseTrapNowPage = backPage;
    //    UpdateChoseTrap();

    //}

    [Header("選べる罠")]

    public List<TrapButtonUI> chooseTrapButtons = new List<TrapButtonUI>();
    public GameObject CanChooseTrapNextPage;
    public GameObject CanChooseTrapBackPage;


    private int canChooseTrapNowPage = 0;
    private int showCanChooseTrapPageLimit => Mathf.CeilToInt((float)CanUseTrap.allTrap.Count / chooseTrapButtons.Count) - 1;

    private void UpdateCanChooseTrap()
    {
        int trapIndex = canChooseTrapNowPage * chooseTrapButtons.Count;
        for (int index = 0; index < chooseTrapButtons.Count; index++)
        {
            chooseTrapButtons[index].gameObject.SetActive(false);
            chooseTrapButtons[index].button.onClick.RemoveAllListeners();
            if (trapIndex < CanUseTrap.allTrap.Count)
            {
                TrapName targetTrap = (TrapName)trapIndex;
                switch (targetTrap)
                {
                    case TrapName.Spikes:
                        chooseTrapButtons[index].button.onClick.AddListener(Button_ChooseSpikes);
                        break;
                    case TrapName.FallRock:
                        chooseTrapButtons[index].button.onClick.AddListener(Button_ChooseFallRock);
                        break;
                    case TrapName.Boom:
                        chooseTrapButtons[index].button.onClick.AddListener(Button_ChooseBoom);
                        break;
                    case TrapName.JumpPad:
                        chooseTrapButtons[index].button.onClick.AddListener(Button_ChooseJumpPad);
                        break;
                }

                chooseTrapButtons[index].SetTrap(CanUseTrap.allTrap[targetTrap]);
                chooseTrapButtons[index].gameObject.SetActive(true);
                trapIndex++;
            }

        }

        if(canChooseTrapNowPage >= showCanChooseTrapPageLimit) CanChooseTrapNextPage.SetActive(false);
        else CanChooseTrapNextPage.SetActive(true);

        if (canChooseTrapNowPage <= 0) CanChooseTrapBackPage.SetActive(false);
        else CanChooseTrapBackPage.SetActive(true);

    }

    public void Button_CanChooseTrapNextPage()
    {
        int nextPage = canChooseTrapNowPage + 1;
        if (nextPage > showCanChooseTrapPageLimit) return;
        else canChooseTrapNowPage = nextPage;
        UpdateCanChooseTrap();
    }

    public void Button_CanChooseTrapBackPage()
    {
        int backPage = canChooseTrapNowPage - 1;
        if (backPage < 0) return;
        else canChooseTrapNowPage = backPage;
        UpdateCanChooseTrap();
    }

    public void Button_ChooseSpikes() => AddToPlayerTrap(TrapName.Spikes);
    public void Button_ChooseFallRock() => AddToPlayerTrap(TrapName.FallRock);
    public void Button_ChooseBoom() => AddToPlayerTrap(TrapName.Boom);
    public void Button_ChooseJumpPad() => AddToPlayerTrap(TrapName.JumpPad);



    public void TitleCanvas_Init()
    {
        UpdateChoseTrap();
        UpdateCanChooseTrap();

        StartCoroutine(Timer());
    }


}
