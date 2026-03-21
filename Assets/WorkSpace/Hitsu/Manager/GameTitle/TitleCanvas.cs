using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Trap 選択データ
/// </summary>
public class ChoseTrapData
{
    // Trap の種類
    public TrapName trapName;
    // 選択した個?
    public int trapCount;
}

/// <summary>
/// プ?イ?ーがゲー?開始前に Trap を選択する UI 管?ク?ス。
///
/// 主な役?：
/// ・Trap の選択 UI 表示
/// ・選択した Trap の管?
/// ・Trap コスト計算
/// ・Trap ページ切り替え
/// ・プ?イ?ー?備タイマー管?
/// ・選択した Trap を Backpack に登録
/// </summary>
public class TitleCanvas : MonoBehaviour
{
    [Header("プ?イ?ー情報")]

    /// <summary>
    /// この UI を使用するプ?イ?ー
    /// </summary>
    public Player targetPlayer;

    /// <summary>
    /// 残りコスト表示
    /// </summary>
    public TMP_Text costText;

    /// <summary>
    /// ?備?間タイマー表示
    /// </summary>
    public TMP_Text timerText;

    /// <summary>
    /// プ?イ?ーが?備完了したか
    /// </summary>
    public bool isPlayerReady = false;

    /// <summary>
    /// 現在使用可能なコスト
    /// </summary>
    private int nowCost = Backpack.maxCost;

    /// <summary>
    /// Trap 選択制限?間
    /// </summary>
    private const int timerLimit = 10;

    /// <summary>
    /// 現在の残り?間
    /// </summary>
    private float timer = timerLimit;

    /// <summary>
    /// Trap 選択タイマー Coroutine
    /// </summary>
    private IEnumerator Timer()
    {
        while (timer > 0)
        {
            // ?間減少
            timer -= Time.deltaTime;
            // UI 更新
            timerText.text = $"{(int)timer:D2}";
            yield return null;
        }
        // ?間切れで?備完了
        isPlayerReady = true;
    }

    /// <summary>
    /// 選択した Trap を Backpack に登録する
    /// </summary>
    public void ChoseTrapToBackpack(out bool isDone)
    {
        // Trap が選択されていない
        if (playerTrap.Count == 0)
        {
            isDone = false;
            return;
        }

        // Backpack に追加
        foreach (ChoseTrapData trap in playerTrap)
        {
            targetPlayer.hunter.backpack.AddToBackpack(trap.trapName, trap.trapCount);
        }

        // 追加完了判定
        isDone = targetPlayer.hunter.backpack.trapsPack.Count == playerTrap.Count;
    }

    [Header("選んだ罠")]

    /// <summary>
    /// 選択した Trap UI
    /// </summary>
    public List<TrapUI> choseTrap = new List<TrapUI>();

    /// <summary>
    /// プ?イ?ーが選択した Trap データ
    /// </summary>
    private List<ChoseTrapData> playerTrap = new List<ChoseTrapData>();

    /// <summary>
    /// 選択 Trap UI のページ?
    /// </summary>
    private int showChoseTrapPageLimit => playerTrap.Count / choseTrap.Count;

    /// <summary>
    /// 現在表示しているページ
    /// </summary>
    private int choseTrapNowPage = 0;

    /// <summary>
    /// 選択済み Trap UI 更新
    /// </summary>
    private void UpdateChoseTrap()
    {
        // 残りコスト表示更新
        costText.text = $"Cost Left : {nowCost:D2}";
        int trapIndex = choseTrapNowPage * showChoseTrapPageLimit;
        int choseTrapMax = playerTrap.Count > choseTrap.Count ? choseTrap.Count : playerTrap.Count;

        for (int index = 0; index < choseTrap.Count; index++)
        {
            // UI 非表示
            choseTrap[index].gameObject.SetActive(false);

            // Trap が存在する場?表示
            if (trapIndex < playerTrap.Count)
            {
                choseTrap[index].SetTrap(playerTrap[trapIndex]);
                choseTrap[index].gameObject.SetActive(true);
                trapIndex++;
            }

        }
    }

    /// <summary>
    /// Trap をプ?イ?ー選択?ストに追加
    /// </summary>
    private void AddToPlayerTrap(TrapName targetTrap)
    {
        // コスト計算
        int checkCost = nowCost - GameManager.allTrap[targetTrap].cost;
        // コスト不足
        if (checkCost < 0) return;
        // コスト更新
        nowCost = checkCost;

        // 既に選択されている Trap を?索
        ChoseTrapData trap = playerTrap.Find(t => t.trapName == targetTrap);
        if (trap == null)
        {
            // 新規追加
            playerTrap.Add(new ChoseTrapData()
            {
                trapName = targetTrap,
                trapCount = 1
            });
        }
        // 個??加
        else trap.trapCount += 1;

        // UI 更新
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

    /// <summary>
    /// Trap 選択ボタ?
    /// </summary>
    public List<TrapButtonUI> chooseTrapButtons = new List<TrapButtonUI>();

    /// <summary>
    /// ?ページボタ?
    /// </summary>
    public GameObject CanChooseTrapNextPage;

    /// <summary>
    /// 前ページボタ?
    /// </summary>
    public GameObject CanChooseTrapBackPage;

    /// <summary>
    /// 現在表示?のページ
    /// </summary>
    private int canChooseTrapNowPage = 0;

    /// <summary>
    /// 最大ページ?
    /// </summary>
    private int showCanChooseTrapPageLimit => Mathf.CeilToInt((float)GameManager.allTrap.Count / chooseTrapButtons.Count) - 1;

    /// <summary>
    /// 選択可能な Trap ボタ? UI を更新する。
    ///
    /// ??内容：
    /// ・現在のページに?じて Trap を表示
    /// ・ボタ?イベ?トを設定
    /// ・?ページ / 前ページボタ?の表示更新
    /// </summary>
    private void UpdateCanChooseTrap()
    {
        // 現在ページの先頭 Trap イ?デックス
        int trapIndex = canChooseTrapNowPage * chooseTrapButtons.Count;
        for (int index = 0; index < chooseTrapButtons.Count; index++)
        {
            // いったん UI を非表示
            chooseTrapButtons[index].gameObject.SetActive(false);

            // 以前設定されていたイベ?トを削?
            chooseTrapButtons[index].button.onClick.RemoveAllListeners();

            // 表示できる Trap がまだ?る場?
            if (trapIndex < GameManager.allTrap.Count)
            {
                // TrapName を取得
                TrapName targetTrap = (TrapName)trapIndex;
                // ボタ??下?に Trap を追加
                chooseTrapButtons[index].button.onClick.AddListener(() => AddToPlayerTrap(targetTrap));
                // Trap 情報を UI に設定
                chooseTrapButtons[index].SetTrap(GameManager.allTrap[targetTrap]);
                // ボタ?表示
                chooseTrapButtons[index].gameObject.SetActive(true);
                // ?の Trap へ
                trapIndex++;
            }

        }

        // ?ページボタ?表示制御
        if (canChooseTrapNowPage >= showCanChooseTrapPageLimit) CanChooseTrapNextPage.SetActive(false);
        else CanChooseTrapNextPage.SetActive(true);

        // 前ページボタ?表示制御
        if (canChooseTrapNowPage <= 0) CanChooseTrapBackPage.SetActive(false);
        else CanChooseTrapBackPage.SetActive(true);

    }

    /// <summary>
    /// ?ページへ移動
    /// </summary>
    public void Button_CanChooseTrapNextPage()
    {
        int nextPage = canChooseTrapNowPage + 1;
        if (nextPage > showCanChooseTrapPageLimit) return;
        else canChooseTrapNowPage = nextPage;
        UpdateCanChooseTrap();
    }

    /// <summary>
    /// 前ページへ移動
    /// </summary>
    public void Button_CanChooseTrapBackPage()
    {
        int backPage = canChooseTrapNowPage - 1;
        if (backPage < 0) return;
        else canChooseTrapNowPage = backPage;
        UpdateCanChooseTrap();
    }

    /// <summary>
    /// TitleCanvas ?期化
    /// </summary>
    public void TitleCanvas_Init()
    {
        // UI 更新
        UpdateChoseTrap();
        UpdateCanChooseTrap();

        // タイマー開始
        StartCoroutine(Timer());
    }


}
