using UnityEngine;
using TMPro;

/// <summary>
/// リザルト画面の各プレイヤー用Canvas制御
/// ・勝敗表示
/// ・自分 / 相手の記録表示
/// ・Replay / Title の選択保持
/// </summary>
public class Release_Canvas : MonoBehaviour
{
    /// <summary>
    /// プレイヤーの選択状態
    /// </summary>
    public enum Option
    {
        None,
        Replay,
        BackToTitle
    }
    /// <summary> 現在の選択状態 </summary>
    public Option option {  get; private set; }

    /// <summary> 勝敗表示用テキスト </summary>
    public TMP_Text winnerResult;
    /// <summary> 相手の結果表示用テキスト </summary>
    public TMP_Text othersResult;
    /// <summary> 自分の結果表示用テキスト </summary>
    public TMP_Text yourResult;

    private const string winnerText = "Winner : ";
    private const string otherPlayer = "Others";
    private const string you = "You";


    /// <summary> このCanvasが担当するプレイヤー </summary>
    private Winner thisPlayer;
    /// <summary> このプレイヤーが勝者かどうか </summary>
    private bool isWin => thisPlayer == Release.Instance.winner;
    /// <summary>
    /// リザルト表示更新
    /// ・勝者表示
    /// ・自分と相手の走行距離表示
    /// </summary>
    private void ResultShow()
    {
        // 最新の結果を取得
        string _player01Result = GameManager.Instance.player01RanValue.ToString("F1"); 
        string _player02Result = GameManager.Instance.player02RanValue.ToString("F1");

        // Winner 表示
        string result = isWin ? $"{you}" : $"{otherPlayer}";
        winnerResult.text = winnerText + result;

        // このCanvasが Player01 用かどうかで表示内容を切り替える
        if (thisPlayer == Winner.Player01)
        {
            othersResult.text = $"{otherPlayer} : \n {_player02Result} meter";
            yourResult.text = $"{you} : \n {_player01Result} meter";
        }
        else
        {
            othersResult.text = $"{otherPlayer} : \n {_player01Result} meter";
            yourResult.text = $"{you} : \n {_player02Result} meter";
        }

    }

    /// <summary>
    /// 初期化
    /// ・担当プレイヤー設定
    /// ・選択リセット
    /// ・結果表示更新
    /// </summary>
    public void Init(Winner targetPlayer)
    {
        thisPlayer = targetPlayer;

        ResultShow();

    }
    /// <summary>
    /// 選択状態をリセット
    /// </summary>
    public void ResetOption() => option = Option.None;
    /// <summary>
    /// タイトルへ戻るボタン
    /// ・未選択時のみ有効
    /// </summary>
    public void Button_BackToTitle()
    {
        if (option!= Option.None) return;
        option = Option.BackToTitle;

    }
    /// <summary>
    /// リプレイボタン
    /// ・未選択時のみ有効
    /// </summary>
    public void Button_RePlay()
    {
        if (option != Option.None) return;
        option = Option.Replay;

    }
}
