using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// タイマーUIと進行状況（チェックポイント）の進捗バーを管理するクラス
/// ・時間の表示（数値＆ゲージ）
/// ・チェックポイント達成に応じたアニメーション制御
/// ・進行率（Slider）更新
/// </summary>
public class TimeAndProgressBar : MonoBehaviour
{
    //==============================
    // UI参照
    //==============================
    public Animator animator;          // 進捗バー用Animator
    public Image timerImage;           // タイマーゲージ
    public TMP_Text timerText;         // タイマー数値
    public Slider progressBar;         // 進行度スライダー
    public TMP_Text passed;         // タイマー数値


    //==============================
    // チェックポイント
    //==============================
    private List<Transform> _checkPoints => InGame.Instance.checkPoints;

    //==============================
    // タイマー設定
    //==============================
    private const float fullImageFill = 0.25f;  // ゲージ最大値（UI設計に依存）
    private const float emptyImageFill = 0.0f;  // ゲージ最小値

    /// <summary>
    /// 現在のタイマーに応じた FillAmountを計算
    /// </summary>
    private float NowImageFill()
    {
        // timerStartが0の場合は計算できないため emptyImageFill を返す
        if (InGame.timerStart <= 0.001f) return emptyImageFill;

        // 現在時間をUI用の fill値に変換
        float value = InGame.Instance.timer * (fullImageFill / InGame.timerStart);

        // UIの範囲内（min〜max）に制限
        return Mathf.Clamp(value, emptyImageFill, fullImageFill);
    }

    //==============================
    // Coroutine管理
    //==============================
    private Coroutine timerUI;
    private Coroutine progressBarUI;
    private Coroutine checkPointUI;

    //==============================
    // Timer UI
    //==============================
    private IEnumerator TimerUI()
    {
        // 初期の秒数を取得
        int recodedTimer = InGame.Instance.NowTimeToInt();
        // 初期表示
        timerText.text = recodedTimer.ToString("D3");
        while (true)
        {
            // 毎フレーム：ゲージ（fillAmount）更新
            timerImage.fillAmount = NowImageFill();
            // 秒数が変わった場合のみテキスト更新（無駄な更新を防ぐ）
            if (recodedTimer != InGame.Instance.NowTimeToInt())
            {
                recodedTimer = InGame.Instance.NowTimeToInt();
                timerText.text = recodedTimer.ToString("D3");
            }
            // 次のフレームまで待機
            yield return null;
        }


    }

    //==============================
    // CheckPoint UI
    //==============================
    private IEnumerator CheckPointUI()
    {
        // 初期状態：全てのチェックポイントを未達成（false）に設定
        for (int i = 0; i < _checkPoints.Count; i++)
        {
            animator.SetBool($"isPassCheckPoint0{i + 1}", false);
        }
        // 1フレーム待機（初期化反映）
        yield return null;

        // チェックポイントを順番に監視
        for (int i = 0; i < _checkPoints.Count; i++)
        {
            Transform point = _checkPoints[i];

            // 条件：
            // ・Dictionaryにキーが存在する
            // ・かつ、そのチェックポイントがtrue（到達済み）
            yield return new WaitUntil(() => InGame.Instance.checkPointsDict.ContainsKey(point) && InGame.Instance.checkPointsDict[point]);

            // 到達したチェックポイントのアニメーションをON
            animator.SetBool($"isPassCheckPoint0{i + 1}", true);

            // 次の処理へ（1フレーム待機）
            yield return null;
        }
    }

    //==============================
    // ProgressBar UI
    //==============================

    private void PassedText()
    {
        passed.text = $"Passed : {InGame.Instance.passedDistance} / {InGame.startToGoalMeter}";
    }

    private IEnumerator ProgressBarUI()
    {
        float progress = 0;
        while (true)
        {
            // 進行率（0〜1）を取得
            if (InGame.Instance.percentOfPassedDistance> progress)
            {
                progress = InGame.Instance.percentOfPassedDistance;
                progressBar.value = progress;
                PassedText();
            }
            // 次のフレームへ
            yield return null;
        }

    }

    //==============================
    // 初期化
    //==============================
    /// <summary>
    /// UIコルーチンを初期化して再起動する
    /// </summary>
    public void ProgressBarInit()
    {
        // 全コルーチン停止（安全にリセット）
        StopAllCoroutines();

        // 各UI処理を再スタート
        timerUI = StartCoroutine(TimerUI());

        // InGameで startingPoint と goal が未設定の場合、進行率が正しく計算できないため使用不可
        progressBarUI = StartCoroutine(ProgressBarUI());

        // InGameで checkPoints が正しく設定されていない場合、チェックポイントUIが正常に動作しないため使用不可
        checkPointUI = StartCoroutine(CheckPointUI());
    }



}
