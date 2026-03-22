using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Trap の種類を表す列挙型。
/// </summary>
public enum TrapName
{
    Spikes,         // トゲトラップ
    FallRock,       // 落石トラップ
    Boom,           // 爆発トラップ
    JumpPad,
    ChaseEnemy,
    BlackHole
}
/// <summary>
/// すべての Trap の基底クラス。
/// 
/// 主な役割：
/// ・Trap の基本データ（種類 / コスト）の管理
/// ・Collider / Rigidbody の管理
/// ・Trap の設置状態の管理
/// 
/// 各 Trap（Spikes / FallRock / Boom）はこのクラスを継承して
/// 個別の挙動を実装する。
/// </summary>
public abstract class Trap : MonoBehaviour
{
    /// <summary>
    /// 衝突した GameObject が指定した Layer かどうかを判定する
    /// </summary>
    /// <param name="collision">衝突した Collision2D</param>
    /// <param name="targetLayer">判定する Layer</param>
    /// <returns>同じ Layer の場合 true</returns>
    public bool IsGameObjectLayer(Collider2D collision, int targetLayer) => collision.gameObject.layer == targetLayer;

    // Trap の種類
    public TrapName trapName;
    // Trap のコスト
    public int cost;
    // Rigidbody2D
    public Rigidbody2D rb;
    // Collider
    public Collider2D trapCollider;
    // 設置完了状態
    public bool isSetup = false;
    // 現在のグリッド座標
    public Vector2Int currentGridPos;
    // 初期の配置座標（上昇処理などで記憶しておく用）
    public Vector2Int originGridPos;



    /// <summary>
    /// Trap の初期化処理
    /// </summary>
    public virtual void Init() 
    {
        trapCollider = GetComponent<Collider2D>();

        rb = GetComponent<Rigidbody2D>();
        // 初期状態では物理演算を停止
        rb.simulated = false;

    }

    /// Trap を設置する
    /// Collider を有効化し、設置状態にする
    /// </summary>
    public virtual void SetUp()
    {
        trapCollider.isTrigger = true;
        isSetup = true;

        if (StageGridManager.Instance != null)
        {
            currentGridPos = StageGridManager.Instance.WorldToGrid(transform.position);
            originGridPos = currentGridPos;
            StageGridManager.Instance.RegisterTrap(currentGridPos);
        }
    }

    protected virtual void Update()
    {
        if (!isSetup || StageGridManager.Instance == null) return;

        Vector2Int newGridPos = StageGridManager.Instance.WorldToGrid(transform.position);
        if (newGridPos != currentGridPos)
        {
            StageGridManager.Instance.MoveTrap(currentGridPos, newGridPos);
            currentGridPos = newGridPos;
        }
    }

    /// <summary>
    /// StageGridManager を使用して安全に1マスずつ落下するコルーチン
    /// </summary>
    /// <param name="speed">落下速度</param>
    /// <param name="onLanded">着地時のコールバック関数</param>
    protected IEnumerator GridFallCoroutine(float speed, System.Action onLanded = null)
    {
        if (StageGridManager.Instance == null)
        {
            onLanded?.Invoke();
            yield break;
        }

        while (true)
        {
            // 1マス下の座標をチェック
            Vector2Int nextGridPos = currentGridPos + Vector2Int.down;
            if (StageGridManager.Instance.CanPlaceTrapDataDriven(nextGridPos))
            {
                // 空いている場合はそこに向かって移動
                Vector3 targetPos = StageGridManager.Instance.GridToWorld(nextGridPos);
                while (Vector3.Distance(transform.position, targetPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                    yield return null;
                }
                // 移動完了後、完全にスナップ
                transform.position = targetPos;
            }
            else
            {
                // ここから先は進めないため着地
                onLanded?.Invoke();
                break;
            }
        }
    }

    /// <summary>
    /// StageGridManager を使用して安全に上方向へマスを戻るコルーチン
    /// </summary>
    /// <param name="targetGridPos">目標とする上方向の座標</param>
    /// <param name="speed">上昇速度</param>
    /// <param name="onReached">目標到達、または上に障害物があって止まった時のコールバック</param>
    protected IEnumerator GridRiseCoroutine(Vector2Int targetGridPos, float speed, System.Action onReached = null)
    {
        if (StageGridManager.Instance == null)
        {
            onReached?.Invoke();
            yield break;
        }

        while (currentGridPos.y < targetGridPos.y)
        {
            // 1マス上の座標をチェック
            Vector2Int nextGridPos = currentGridPos + Vector2Int.up;
            if (StageGridManager.Instance.CanPlaceTrapDataDriven(nextGridPos))
            {
                // 空いている場合は上へ移動
                Vector3 targetWorld = StageGridManager.Instance.GridToWorld(nextGridPos);
                while (Vector3.Distance(transform.position, targetWorld) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetWorld, speed * Time.deltaTime);
                    yield return null;
                }
                transform.position = targetWorld;
            }
            else
            {
                // 何か上に障害物がある場合はそこで停止
                onReached?.Invoke();
                yield break;
            }
        }
        
        // 目標座標に到着
        onReached?.Invoke();
    }

    protected virtual void OnDestroy()
    {
        if (isSetup && StageGridManager.Instance != null)
        {
            StageGridManager.Instance.UnregisterTrap(currentGridPos);
        }
    }

    public virtual void BrakeTheTrap() => InGame.Instance.hunterConTrollerPad.DestroyTrap(this);

}
