using UnityEngine;

/// <summary>
/// トゲトラップ。
/// 設置後に空中から落下し、地面または他の Trap に接触すると設置完了となる。
/// Runner が接触するとダメージを与えるタイプの Trap を想定している。
/// </summary>
public class Spikes : InstallationTrap
{
    /// <summary>
    /// Trap 初期化
    /// </summary>
    public override void Init()
    {
        base.Init();
        trapName = TrapName.Spikes;
        // 設置コスト
        cost = 1;
    }
    /// <summary>
    /// Trap 設置処理
    /// </summary>
    public override void SetUp()
    {
        base.SetUp();
        // Spikes は壁/床にくっついて落下しないので FallAndSetUp は呼ばず、即座にLayerと物理設定を確定する
        gameObject.layer = UseLayerName.trapLayer;
        if (gameObject.transform.childCount > 0)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.layer = UseLayerName.trapLayer;
            }
        }
        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Static;
            Destroy(rb);
        }
        isFallDone = true;
    }

    /// <summary>
    /// 衝突判定
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        // Runner に当たった場合
        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {

            Debug.Log("Hit Runner");
        }
        // 地面または Trap に当たった場合
        else if ((IsGameObjectLayer(collision, UseLayerName.trapLayer) || IsGameObjectLayer(collision, UseLayerName.platformLayer)) && !isFallDone)
        {
            isFallDone = true;
            return;
        }
    }

}
