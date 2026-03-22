using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

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
    // 上昇速度
    public float riseSpeed = 3f;
    // 着地後に留まる時間
    public float stayBottomTime = 1f;

    /// <summary>
    /// Trap の動作ルール
    /// </summary>
    public override IEnumerator TrapRule()
    {
        gameObject.layer = UseLayerName.trapLayer;
        rb.simulated = true;

        while (true)
        {
            fallDone = false;
            
            // 落下まで待機
            yield return new WaitForSeconds(fallCoolDown);

            // 落下処理
            yield return StartCoroutine(GridFallCoroutine(fallSpeed, () => fallDone = true));

            // 着地後の待機
            yield return new WaitForSeconds(stayBottomTime);

            // 上昇処理 (最初の設置位置 originGridPos へ戻る。上に障害物があれば途中で止まる)
            bool riseDone = false;
            yield return StartCoroutine(GridRiseCoroutine(originGridPos, riseSpeed, () => riseDone = true));
        }
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
    //        // 地面または Trap に衝突
    //        else if (IsGameObjectLayer(collision, UseLayerName.trapLayer) || IsGameObjectLayer(collision, UseLayerName.platformLayer))
    //        {
    //            fallDone = true;
    //            rb.bodyType = RigidbodyType2D.Static;
    //        }
    //    }
    //}

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
      //  if (!isSetup) return;

     //   if (!Condition())
     //   {
     //       if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
            //{
      //          // Runner に衝突
         //   }
            // 地面または Trap に衝突
       //     else if (IsGameObjectLayer(collision, UseLayerName.trapLayer) || IsGameObjectLayer(collision, UseLayerName.platformLayer))
       //     {

       //         fallDone = true;
         //       rb.bodyType = RigidbodyType2D.Static;
                // `GridFallCoroutine` で着地判定を行っているため、ここでは着地フラグのみを操作せず、将来の処理追加用として残しています
                // fallDone = true;
                // rb.bodyType = RigidbodyType2D.Static;
          //  }
      //  }
  //  }

}