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

}
