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
    private enum Directions { Down, Up };
    private Directions directions;
    private Vector3 Direction(Directions direction)
    {
        switch (direction)
        {
            case Directions.Down: return Vector3.down;
            case Directions.Up: return Vector3.up;

        }
        return Vector3.zero;
    }
    private float setUpVector3Y;
    private bool IsSetUpVector3() => (Mathf.Abs(transform.position.y - setUpVector3Y) < 0.1f);

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
    public bool actionDone = false;
    /// <summary>
    /// Trap 発動条件
    /// </summary>
    public override bool Condition() => directions == Directions.Down ? actionDone : IsSetUpVector3();

    private IEnumerator Move()
    {
        Vector3 direction = Direction(directions);
        yield return new WaitForSeconds(fallCoolDown);
        while (!Condition())
        {
            transform.position += direction * fallSpeed * Time.deltaTime;
            yield return null;

        }
        directions = directions == Directions.Down? Directions.Up : Directions.Down;
        if(directions == Directions.Down) actionDone = false;

    }



    /// <summary>
    /// Trap の動作ルール
    /// </summary>
    public override IEnumerator TrapRule()
    {
        gameObject.layer = UseLayerName.trapLayer;
        setUpVector3Y = this.transform.position.y;
        directions = Directions.Down;
        rb.simulated = true;

        while (true)
        {
            yield return Move();
        }

        //// 落下まで待機
        //yield return new WaitForSeconds(fallCoolDown);

        //// 落下処理
        //while (!Condition())
        //{
        //    transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        //    yield return null;

        //}

        //// マップ扱いに変更
        //gameObject.layer = UseLayerName.platformLayer;
        //// Rigidbody 削除
        //Destroy(rb);
        //// このスクリプトを停止
        //this.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        //if (!Condition())
        //{
            if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
            {
                // Runner に衝突
            }
            // 地面または Trap に衝突
            else if (IsGameObjectLayer(collision, UseLayerName.trapLayer) || IsGameObjectLayer(collision, UseLayerName.platformLayer))
            {

                actionDone = true;
                //rb.bodyType = RigidbodyType2D.Static;
            }
        //}
    }


}

