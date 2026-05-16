using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// イカ墨のエフェクトUI
/// トラップがRunnerに接触した際に生成され、画面中央にイカ墨を表示します。
/// </summary>
public class InkEffectUI : MonoBehaviour
{
    public float showDuration = 3f;
    public float fadeDuration = 2f;

    private Image inkImage;

    private void Awake()
    {
        // Canvasの生成
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 最前面に表示

        // Runnerのディスプレイを取得して設定する
        if (InGame.Instance != null)
        {
            Player runnerPlayer = InGame.Instance._player01.job == Player.Job.Runner ? InGame.Instance._player01 : InGame.Instance._player02;
            canvas.targetDisplay = (int)runnerPlayer.displayCode;
        }

        // UIイベントブロッカーとして機能させない場合はRaycasterは不要ですが、念のため
        // gameObject.AddComponent<GraphicRaycaster>(); 

        // イカ墨Imageの生成
        GameObject imageObj = new GameObject("InkImage");
        imageObj.transform.SetParent(this.transform, false);

        inkImage = imageObj.AddComponent<Image>();
        inkImage.raycastTarget = false; // クリック等をブロックしないように
        
        // 画面の中心に配置
        RectTransform rt = inkImage.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        // 指定された「縦横比 1:1」を再現するため正方形のサイズを設定
        // 画面高さなどを基準にしてもよいですが、固定で十分大きめにします
        rt.sizeDelta = new Vector2(1600f, 1600f);
    }

    /// <summary>
    /// 画像をセットしてアニメーションを開始します
    /// </summary>
    public void Setup(Sprite sprite)
    {
        if (sprite != null)
        {
            inkImage.sprite = sprite;
            inkImage.color = Color.white; // スプライトがある場合は白（元の色）
        }
        else
        {
            // スプライトが未設定の場合は黒のベタ塗りになる
            inkImage.color = Color.black; 
        }

        StartCoroutine(InkRoutine());
    }

    private IEnumerator InkRoutine()
    {
        // 3秒間そのまま表示
        yield return new WaitForSeconds(showDuration);

        // 2秒かけてフェードアウト
        float timer = 0f;
        Color color = inkImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            color.a = alpha;
            inkImage.color = color;
            yield return null;
        }

        // フェードアウト完了後に破棄
        Destroy(gameObject);
    }
}
