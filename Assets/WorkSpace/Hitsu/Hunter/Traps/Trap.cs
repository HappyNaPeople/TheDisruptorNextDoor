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
    BlackHole,
    Shell,
    FireBar
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
        //trapCollider.isTrigger = true;
        isSetup = true;

        if (StageGridManager.Instance != null)
        {
            currentGridPos = StageGridManager.Instance.WorldToGrid(transform.position);
            originGridPos = currentGridPos;
            StageGridManager.Instance.RegisterTrap(currentGridPos);
        }
    }

    protected virtual void Update() { }


    protected virtual void OnDestroy()
    {
        if (isSetup && StageGridManager.Instance != null)
        {
            StageGridManager.Instance.UnregisterTrap(currentGridPos);
            InGame.Instance.RemoveTrap(this.gameObject);
            //HunterConTrollerPad.Instance.UpdateSetupTrapText();
        }
    }

    public virtual void BrakeTheTrap() => InGame.Instance.hunterConTrollerPad.DestroyTrap(this);

    /// <summary>
    /// 衝突した GameObject が指定した Layer かどうかを判定する
    /// </summary>
    /// <param name="collision">衝突した Collision2D</param>
    /// <param name="targetLayer">判定する Layer</param>
    /// <returns>同じ Layer の場合 true</returns>
    public bool IsGameObjectLayer(Collider2D collision, int targetLayer) => collision.gameObject.layer == targetLayer;

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        // Runnerに当たった場合のデフォルト動作：死亡させる
        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                runner.Death();
            }
        }
    }
}
