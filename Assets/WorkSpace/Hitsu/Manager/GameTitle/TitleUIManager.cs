using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Player;

/// <summary>
/// 参考用のタイト?
/// タイト?シー?を管?するク?ス。
///
/// 主な役?：
/// ・プ?イ?ーごとの TitleCanvas ?期化
/// ・カ??の表示ディスプ?イ設定
/// ・Trap 選択の完了監?
/// ・Trap 選択完了後に InGame シー?へ遷移
///
/// 2人のプ?イ?ーが Trap を選択し終わると、
/// 自動的にゲー?シー?へ移動する。
/// </summary>
public class TitleUIManager : MonoBehaviour
{
    /// <summary>
    /// Singleton イ?スタ?ス
    /// </summary>
    public static TitleUIManager Instance;

    // ==============================
    // Player01
    // ==============================

    /// <summary>
    /// Player01 取得（GameManager から）
    /// </summary>
    public Player _player01 => GameManager.Instance.player01;

    /// <summary>
    /// Player01 用カ??
    /// </summary>
    public Camera player01Camera;

    /// <summary>
    /// Player01 Trap 選択 UI
    /// </summary>
    public TitlePlayerCanvas player01TitlePlayerCanvas;


    // ==============================
    // Player02
    // ==============================

    /// <summary>
    /// Player02 取得
    /// </summary>
    public Player _player02 => GameManager.Instance.player02;

    /// <summary>
    /// Player02 用カ??
    /// </summary>
    public Camera player02Camera;

    /// <summary>
    /// Player02 Trap 選択 UI
    /// </summary>
    public TitlePlayerCanvas player02TitlePlayerCanvas;

    public float trapSelectTime = 10f;

    public CanvasGroup[] Titles = new CanvasGroup[2];
    public CanvasGroup[] TurnSelect = new CanvasGroup[2];
    public CanvasGroup[] TrapSelect = new CanvasGroup[2];


    /// <summary>
    /// プ?イ?ーごとの Canvas ?期化
    /// </summary>
    private void PlayerCanvas_Init()
    {
        // Player01 Canvas 設定
        player01TitlePlayerCanvas.targetPlayer = _player01;
        player01Camera.targetDisplay = (int)_player01.displayCode;

        // Player02 Canvas 設定
        player02TitlePlayerCanvas.targetPlayer = _player02;
        player02Camera.targetDisplay = (int)_player02.displayCode;

        // TitleCanvas ?期化
        //player01TitlePlayerCanvas.TitleTrapCanvas_Init();
        //player02TitlePlayerCanvas.TitleTrapCanvas_Init();
    }


    private bool IsBothPlayersPressedStart()
        => player01TitlePlayerCanvas.playerState == TitlePlayerCanvas.TitlePlayerState.WaitingStart
        && player02TitlePlayerCanvas.playerState == TitlePlayerCanvas.TitlePlayerState.WaitingStart;

    /// <summary>
    /// 両プ?イ?ーが Trap 選択完了したか判定
    /// </summary>
    private bool IsEndChooseTrap() 
        => player01TitlePlayerCanvas.playerState == TitlePlayerCanvas.TitlePlayerState.IsReady 
        && player02TitlePlayerCanvas.playerState == TitlePlayerCanvas.TitlePlayerState.IsReady;

    /// <summary>
    /// Trap 選択終了?? Coroutine
    /// </summary>
    private IEnumerator TitleProcess()
    {
        player01TitlePlayerCanvas.AddStartButtonListener();
        player02TitlePlayerCanvas.AddStartButtonListener();

        // スタート待ち
        while (!IsBothPlayersPressedStart())
        {
            yield return null;
        }

        foreach(var title in Titles)
        {
            title.gameObject.SetActive(false);
        }

        foreach (var turn in TurnSelect)
        {
            turn.gameObject.SetActive(true);
        }
        player01TitlePlayerCanvas.ChangeState(TitlePlayerCanvas.TitlePlayerState.SelectingSide);
        player02TitlePlayerCanvas.ChangeState(TitlePlayerCanvas.TitlePlayerState.SelectingSide);

        var rnd = 0;// Random.Range(0, 2);
        player01TitlePlayerCanvas.sideTmp.text = rnd == 0 ? $"あなたは先行（ゴールを目指す）" : $"あなたは後攻（妨害）";
        player02TitlePlayerCanvas.sideTmp.text = rnd == 1 ? $"あなたは先行（ゴールを目指す）" : $"あなたは後攻（妨害）";
        var player01Job = rnd == 0 ? Job.Runner : Job.Hunter;
        var player02Job = rnd == 1 ? Job.Runner : Job.Hunter;
        _player01.SetJob(player01Job);
        _player02.SetJob(player02Job);

        yield return new WaitForSeconds(3f);

        foreach (var turn in TurnSelect)
        {
            turn.gameObject.SetActive(false);
        }
        foreach (var trap in TrapSelect)
        {
            trap.gameObject.SetActive(true);
        }

        player01TitlePlayerCanvas.ChangeState(TitlePlayerCanvas.TitlePlayerState.SelectingTrap);
        player02TitlePlayerCanvas.ChangeState(TitlePlayerCanvas.TitlePlayerState.SelectingTrap);

        // --- 修正後 ---

        // 1. まずUIの初期化（ボタンの中身の生成や表示設定）を先に行う
        player01TitlePlayerCanvas.TitleTrapCanvas_Init();
        player02TitlePlayerCanvas.TitleTrapCanvas_Init();

        // 2. UIの生成完了を1フレームだけ待つ（超重要）
        yield return null;

        // 3. UIの準備が完全に終わってからフォーカスを当てる
        if (player01TitlePlayerCanvas.chooseTrapButtons.Count > 0)
        {
            var button = player01TitlePlayerCanvas.chooseTrapButtons[0].gameObject.GetComponentInChildren<Button>();
            _player01.inputData.SetSelect(null); // 一度リセット
            _player01.inputData.SetSelect(button.gameObject);
        }

        if (player02TitlePlayerCanvas.chooseTrapButtons.Count > 0)
        {
            var button = player02TitlePlayerCanvas.chooseTrapButtons[0].gameObject.GetComponentInChildren<Button>();
            _player02.inputData.SetSelect(null); // 一度リセット
            _player02.inputData.SetSelect(button.gameObject);
        }

        // -----------------

        // 両プレイヤーが準備完了するまで待つ
        while (!IsEndChooseTrap())
        {
            yield return null;
        }

        // Trap を Backpack に登録
        player01TitlePlayerCanvas.ChoseTrapToBackpack(out bool player01Done);
        player02TitlePlayerCanvas.ChoseTrapToBackpack(out bool player02Done);

        // 登録失敗チェック
        if (!player01Done|| !player02Done)
        {
            Debug.LogWarning($"Player01 id chose Trap : {player01Done} , Player01 id chose Trap : {player02Done}");
        }

        // ゲー?シー?へ移動(Testing)
        GameManager.Instance.StartCoroutine(GameManager.Instance.ChangeScene(SceneState.InGame));
    }

    /// <summary>
    /// GameTitle ?期化
    /// </summary>
    private void TitleUI_Init()
    {
        // プ?イ?ー UI ?期化
        PlayerCanvas_Init();

        // プ?イ?ー UI ?期化
        StartCoroutine(TitleProcess());
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this);
    }

    private void Start()
    {
        TitleUI_Init();
    }
}
