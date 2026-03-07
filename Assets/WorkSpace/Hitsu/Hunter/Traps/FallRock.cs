using UnityEngine;
using System.Collections;

/// <summary>
/// 落石トラップ。
/// 一定時間待機した後に落下し、
/// 地面または他の Trap に接触すると落下を停止する。
/// </summary>
public class FallRock : TiggerTrap
{
    // 落下開始までの待機時間
    private const int fallCoolDown = 3;
    // 落下速度　
    public int fallSpeed = 1;
    /// <summary>
    /// Trap 初期化
    /// </summary>
    public override void Init()
    {
        cost = 1;
        base.Init();
        trapName = TrapName.FallRock;
    }
    /// <summary>
    /// Trap 設置処理
    /// </summary>
    public override void SetUp()
    {
        base.SetUp();
        StartCoroutine(TrapRule());
    }
    // 落下完了フラグ
    private bool fallDone = false;
    /// <summary>
    /// Trap 発動条件
    /// </summary>
    public override bool Condition() => fallDone;
    /// <summary>
    /// Trap の動作ルール
    /// </summary>
    public override IEnumerator TrapRule()
    {
        gameObject.layer = UseLayerName.trapLayer;
        // 落下まで待機
        yield return new WaitForSeconds(fallCoolDown);
        rb.simulated = true;

        // 落下処理
        while (!Condition())
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;

        }

        // マップ扱いに変更
        gameObject.layer = UseLayerName.mapLayer;
        // Rigidbody 削除
        Destroy(rb);
        // このスクリプトを停止
        this.enabled = false;
    }
    /// <summary>
    /// 衝突判定
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isSetup) return;

        if (!Condition())
        {
            if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
            {
                // Runner に衝突
            }
            // 地面または Trap に衝突
            else if (IsGameObjectLayer(collision, UseLayerName.trapLayer) || IsGameObjectLayer(collision, UseLayerName.mapLayer))
            {

                fallDone = true;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
    }




}

