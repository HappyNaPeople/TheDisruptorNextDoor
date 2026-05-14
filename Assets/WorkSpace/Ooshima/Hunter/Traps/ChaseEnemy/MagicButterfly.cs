using UnityEngine;

/// <summary>
/// プレイヤーを追いかけるチェイスエネミートラップ。
/// 浮いていて壁をすり抜け、徐々に加速しながらドリフトするように曲がる。
/// </summary>
public class MagicButterfly : Trap
{
    [Header("移動設定")]
    public float maxSpeed = 5f;          // 最大速度
    public float acceleration = 2f;      // 加速度
    public float turnSpeed = 90f;        // 旋回速度（度/秒）

    private Runner targetRunner;         // ターゲットのプレイヤー
    private float currentSpeed = 0f;     // 現在の速度
    private Vector2 currentVelocity = Vector2.zero; // 現在の移動ベクトル

    [Header("エフェクト")]
    public ParticleSystem[] particles;

    public override void Init()
    {
        base.Init();
        trapName = TrapName.MagicButterfly;
        cost = 5; // 配置コスト（仮）
    }
    public override void SetUp()
    {
        base.SetUp();

    }

    protected override void OnSetupComplete()
    {
        // 物理演算（重力や壁との衝突）を行わず手動で移動させるため、Rigidbodyを無効化または削除
        if (rb != null)
        {
            Destroy(rb); // 浮いてすり抜けるためRigidbodyは不要
        }
        foreach (var p in particles)
        {
            if (p != null) p.Play();
        }

        targetRunner = FindObjectOfType<Runner>();
    }

    protected override void Update()
    {
        // 基底クラスのUpdateはGridへのスナップ・登録用。
        // このトラップは空間を自由に移動するため base.Update() は呼ばない。

        if (!isSetup) return;

        // ターゲットがいなければ再度探す
        if (targetRunner == null)
        {
            targetRunner = FindObjectOfType<Runner>();
            if (targetRunner == null) return;
        }

        ChaseTarget();
    }

    private void ChaseTarget()
    {
        // ターゲットへの方向ベクトルを計算
        Vector2 dirToTarget = (targetRunner.transform.position - transform.position).normalized;

        if (dirToTarget.sqrMagnitude == 0) return;

        var scale = transform.localScale;
        var scaleX = dirToTarget.x < 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x); 

        transform.localScale = new Vector3(scaleX, scale.y, scale.z);

        // 目標の角度（向き）
        float targetAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;

        // 現在の進行方向の角度
        float currentAngle;
        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            currentAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
        }
        else
        {
            currentAngle = targetAngle; // 初期状態ではターゲットの方を向く
        }

        // 角度を滑らかに変化（ドリフトのような曲がり方）
        float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.deltaTime);

        // 新しい進行方向ベクトルを作成
        Vector2 moveDir = new Vector2(Mathf.Cos(nextAngle * Mathf.Deg2Rad), Mathf.Sin(nextAngle * Mathf.Deg2Rad));

        // 加速
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);

        // 速度を更新
        currentVelocity = moveDir * currentSpeed;

        // 位置を更新（壁をすり抜けるため transform を直接移動）
        transform.position += (Vector3)currentVelocity * Time.deltaTime;

        // 進行方向に画像の向きを合わせる場合（必要に応じて有効化）
        // transform.rotation = Quaternion.Euler(0, 0, nextAngle);
    }
}
