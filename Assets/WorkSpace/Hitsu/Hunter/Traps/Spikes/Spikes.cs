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
        // 落下して設置
        StartCoroutine(FallAndSetUp());
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
