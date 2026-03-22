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


    private bool isFallDown = false;
    /// <summary>
    /// Trap 発動条件
    /// </summary>
    public override bool Condition()=> directions == Directions.Down ? isFallDown : IsSetUpVector3();

    private IEnumerator Move()
    {
        Vector3 direction = Direction(directions);
        if (directions == Directions.Down) isFallDown = false;
        yield return new WaitForSeconds(fallCoolDown);

        while (!Condition())
        {
            Vector2Int nextPoint = new Vector2Int((int)(transform.position.x + direction.x), (int)(transform.position.y + direction.y));
            if (!StageGridManager.Instance.CanPlaceTrapDataDriven(nextPoint)) break;

            while(Mathf.Abs(transform.position.y - nextPoint.y) > 0.1f)
            {
                transform.position += direction * fallSpeed * Time.deltaTime;
                yield return null;
            }
            transform.position = new Vector3(nextPoint.x, nextPoint.y,transform.position.z);

        }

        directions = directions == Directions.Down? Directions.Up : Directions.Down;

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


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            // Runner に衝突
        }
        // 地面または Trap に衝突
        else if (IsGameObjectLayer(collision, UseLayerName.trapLayer) || IsGameObjectLayer(collision, UseLayerName.platformLayer))
        {

            //rb.bodyType = RigidbodyType2D.Static;
        }

    }


}

