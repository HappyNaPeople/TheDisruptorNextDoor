using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using static Release_Canvas;
using UnityEngine.UI;

/// <summary>
/// 勝者の種類
/// </summary>
public enum Winner
{
    Player01,
    Player02
}

/// <summary>
/// リザルト画面管理クラス
/// ・勝者判定
/// ・プレイヤーの選択同期
/// ・シーン遷移制御
/// </summary>
public class Release : MonoBehaviour
{
    /// <summary> シングルトンインスタンス </summary>
    public static Release Instance;
    [Header("Audio")]
    public BgmData bgmData;
    [Header("Winner")]
    /// <summary> 勝者 </summary>
    public Winner winner;
    [Header("Player 01 ")]
    /// <summary> プレイヤー1のUI </summary>
    public Release_Canvas player01Canvas;
    public Button player01FirstSelect;
    public Player _player01 => GameManager.Instance.player01;

    [Header("Player 02 ")]
    /// <summary> プレイヤー2のUI </summary>
    public Release_Canvas player02Canvas;
    public Button player02FirstSelect;
    public Player _player02 => GameManager.Instance.player02;

    /// <summary>
    /// 勝者判定
    /// ・走行距離を比較して決定
    /// </summary>
    private void FoundWinner()
    {
        PlayerData player01Result = _player01.playerData;
        PlayerData player02Result = _player02.playerData;
        bool isValueDiff = _player01.playerData.passDistance != _player02.playerData.passDistance;
        if (isValueDiff)
        {
            winner = _player01.playerData.passDistance > _player02.playerData.passDistance ? Winner.Player01 : Winner.Player02;
            return;
        }
        else
        {
            winner = _player01.playerData.passTime < _player02.playerData.passTime ? Winner.Player01 : Winner.Player02;

        }

    }


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

    }

    /// <summary>
    /// 両プレイヤーの選択をリセット
    /// </summary>
    private void ResetAllOption()
    {
        player01Canvas.ResetOption();
        player02Canvas.ResetOption();

    }
    /// <summary> Coroutine管理用 </summary>
    Coroutine releaseProcess;
    private IEnumerator ReleaseProcess()
    {
        // UI初期化のため1フレーム待機
        yield return null;

        while (true)
        {
            // 両プレイヤーが選択するまで待機
            yield return new WaitUntil(() => player01Canvas.option != Option.None && player02Canvas.option != Option.None);
            Option player01Option = player01Canvas.option;
            Option player02Option = player02Canvas.option;

            // -------------------------
            // ❌ 選択が不一致 → リセット
            // -------------------------
            if (player01Option != player02Option)
            {
                ResetAllOption();
                // UI更新待ち（1フレーム）
                yield return null;
                continue;
            }

            // -------------------------
            // ✅ 両方 Title
            // -------------------------
            if (player01Option == Option.BackToTitle)
            {
                Debug.Log("Back To Title");
                // タイトル画面へ遷移
                GameManager.Instance.StartCoroutine(GameManager.Instance.ChangeScene(SceneState.GameTitle));
                yield break;
            }
            // -------------------------
            // ✅ 両方 Replay
            // -------------------------
            else if (player01Option == Option.Replay)
            {
                Debug.Log("Replay Start");

                // ゲームシーン再読み込み
                GameManager.Instance.StartCoroutine(GameManager.Instance.ChangeScene(SceneState.InGame));

                yield break;
            }
        }
    }


    /// <summary>
    /// 初期化処理
    /// ・Coroutine停止
    /// ・勝者判定
    /// ・UI初期化
    /// ・選択処理開始
    /// </summary>
    private void Init()
    {
        // 念のため全Coroutine停止
        StopAllCoroutines();
        // 勝者決定
        FoundWinner();
        // 各プレイヤーUI初期化
        player01Canvas.Init(Winner.Player01);
        player02Canvas.Init(Winner.Player02);
        // 選択処理開始
        releaseProcess = StartCoroutine(ReleaseProcess());

    }

    private void Start()
    {
        Init();
    }



}
