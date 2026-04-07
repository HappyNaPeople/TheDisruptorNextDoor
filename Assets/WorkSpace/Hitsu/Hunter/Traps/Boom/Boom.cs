using UnityEngine;
using System.Collections;

/// <summary>
/// 爆発トラップ。
/// 地面に落下し、接触後に一定時間振動してから爆発する Trap。
/// </summary>
public class Boom : TiggerTrap
{
    // 落下速度
    public float fallSpeed;
    // 爆発までの待機時間
    public float waitForBoom;
    // 爆発範囲
    public float boomArea;
    // 爆発エフェクト
    public GameObject flame;
    /// <summary>
    /// Trap 初期化
    /// </summary>
    public override void Init()
    {
        cost = 1;
        base.Init();
        trapName = TrapName.Boom;
    }
    public override void SetUp()
    {
        base.SetUp();
        // Trap 動作開始の指示（フェードイン開始）
    }

    protected override void OnSetupComplete()
    {
        // 実体化完了後に稼働開始
        StartCoroutine(TrapRule());
    }
    // 落下完了フラグ　 
    private bool fallDone = false;
    /// <summary>
    /// Trap 発動条件
    /// </summary>
    public override bool Condition() => fallDone;
    /// <summary>
    /// 爆発前の振動演出
    /// </summary>
    private IEnumerator Shaking()
    {
        Vector3 originalPos = transform.position;

        float time = 0;

        while (time < waitForBoom)
        {
            float x = Random.Range(-1f, 1f) * 0.1f;
            float y = Random.Range(-1f, 1f) * 0.1f;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;

        yield return null;

    }
    /// <summary>
    /// 爆発処理
    /// </summary>
    private void Explosion()
    {
        // 見た目を消す
        GetComponent<SpriteRenderer>().enabled = false;
        // 当たり判定を爆発範囲に変更
        GetComponent<CircleCollider2D>().radius = boomArea;

    }
    /// <summary>
    /// Trap の動作ルール
    /// </summary>
    public override IEnumerator TrapRule()
    {
        gameObject.layer = UseLayerName.trapLayer;
        rb.simulated = true;
        // 落下処理
        yield return StartCoroutine(GridFallCoroutine(fallSpeed, () => fallDone = true));
        // 爆発前演出
        yield return StartCoroutine(Shaking());
        // 爆発
        Explosion();
        yield return new WaitForEndOfFrame();

        BrakeTheTrap();
    }


    /// <summary>
    /// 衝突判定
    /// </summary>
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (!isSetup) return;

    //    if (!Condition())
    //    {
    //        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
    //        {
    //            // Runner に衝突

    //        }
    //        // 地面に接触 → 落下完了
    //        else if (IsGameObjectLayer(collision, UseLayerName.platformLayer)) fallDone = true;
    //    }
    //}

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        if (!Condition())
        {
            if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
            {
                // Runner に衝突したら死亡させる
                base.OnTriggerEnter2D(collision);
            }
            //  GridFallCoroutine で着地判定を行っています
            // else if (IsGameObjectLayer(collision, UseLayerName.platformLayer)) fallDone = true;
        }
    }


}
