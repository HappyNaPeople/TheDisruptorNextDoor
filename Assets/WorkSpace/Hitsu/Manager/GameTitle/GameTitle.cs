using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 参考用のタイトル
/// タイトルシーンを管理するクラス。
///
/// 主な役割：
/// ・プレイヤーごとの TitleCanvas 初期化
/// ・カメラの表示ディスプレイ設定
/// ・Trap 選択の完了監視
/// ・Trap 選択完了後に InGame シーンへ遷移
///
/// 2人のプレイヤーが Trap を選択し終わると、
/// 自動的にゲームシーンへ移動する。
/// </summary>
public class GameTitle : MonoBehaviour
{
    /// <summary>
    /// Singleton インスタンス
    /// </summary>
    public static GameTitle Instance;

    // ==============================
    // Player01
    // ==============================

    /// <summary>
    /// Player01 取得（GameManager から）
    /// </summary>
    public Player _player01 => GameManager.Instance.player01;

    /// <summary>
    /// Player01 用カメラ
    /// </summary>
    public Camera player01Camera;

    /// <summary>
    /// Player01 Trap 選択 UI
    /// </summary>
    public TitleCanvas player01Canvas;


    // ==============================
    // Player02
    // ==============================

    /// <summary>
    /// Player02 取得
    /// </summary>
    public Player _player02 => GameManager.Instance.player02;

    /// <summary>
    /// Player02 用カメラ
    /// </summary>
    public Camera player02Camera;

    /// <summary>
    /// Player02 Trap 選択 UI
    /// </summary>
    public TitleCanvas player02Canvas;


    /// <summary>
    /// プレイヤーごとの Canvas 初期化
    /// </summary>
    private void PlayerCanvas_Init()
    {
        // Player01 Canvas 設定
        player01Canvas.targetPlayer = _player01;
        player01Camera.targetDisplay = (int)_player01.displayCode;

        // Player02 Canvas 設定
        player02Canvas.targetPlayer = _player02;
        player02Camera.targetDisplay = (int)_player02.displayCode;

        // TitleCanvas 初期化
        player01Canvas.TitleCanvas_Init();
        player02Canvas.TitleCanvas_Init();
    }

    /// <summary>
    /// 両プレイヤーが Trap 選択完了したか判定
    /// </summary>
    private bool IsEndChooseTrap() => player01Canvas.isPlayerReady && player02Canvas.isPlayerReady;

    /// <summary>
    /// Trap 選択終了処理 Coroutine
    /// </summary>
    private IEnumerator EndProcess()
    {
        // 両プレイヤーが準備完了するまで待つ
        while (!IsEndChooseTrap())
        {
            yield return null;
        }

        // Trap を Backpack に登録
        player01Canvas.ChoseTrapToBackpack(out bool player01Done);
        player02Canvas.ChoseTrapToBackpack(out bool player02Done);

        // 登録失敗チェック
        if (!player01Done|| !player02Done)
        {
            Debug.LogWarning($"Player01 id chose Trap : {player01Done} , Player01 id chose Trap : {player02Done}");
        }

        // ゲームシーンへ移動(Testing)
        SceneManager.LoadScene("Test_InGame");

    }

    /// <summary>
    /// GameTitle 初期化
    /// </summary>
    private void GameTitle_Init()
    {
        // プレイヤー UI 初期化
        PlayerCanvas_Init();

        // プレイヤー UI 初期化
        StartCoroutine(EndProcess());
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
        GameTitle_Init();
    }


}
