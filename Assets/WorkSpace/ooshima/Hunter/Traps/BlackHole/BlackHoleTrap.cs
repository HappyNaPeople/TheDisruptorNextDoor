using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ブラックホールトラップ。
/// 範囲内のランナーを引き寄せ、中心付近に到達すると即死させる。
/// </summary>
[RequireComponent(typeof(CircleCollider2D))] // 範囲検知用のコライダーを必須にする
public class BlackHoleTrap : TiggerTrap
{
    [Header("ブラックホール設定")]
    [Tooltip("中心での最大引力（速度に加算される力）")]
    public float maxPullForce = 15f;

    [Tooltip("接地時の引力倍率（0.5にすると空中の半分の強さになる）")]
    public float groundResistanceMultiplier = 0.4f;

    [Tooltip("即死判定となる中心からの距離")]
    public float killRadius = 0.3f;

    // トラップの吸い込み範囲の半径（コライダーから自動取得）
    private float pullRadius;

    // 現在範囲内にいて、モディファイアを付与しているランナーの管理リスト
    private Dictionary<Runner, BlackHoleModifier> activeVictims = new Dictionary<Runner, BlackHoleModifier>();

    private CircleCollider2D pullCollider; // 追加：コライダーをキャッシュする用

    public override void Init()
    {
        cost = 3;
        base.Init();

        pullCollider = GetComponent<CircleCollider2D>();
        if (pullCollider != null)
        {
            // 半径を計算
            pullRadius = pullCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

            // ★配置システムに干渉しないように、実体化するまではコライダーをオフにしておく！
            pullCollider.enabled = false;
        }
    }

    protected override void OnSetupComplete()
    {
        // ★設置完了（実体化）したら、吸い込み判定をオンにする！
        if (pullCollider != null)
        {
            pullCollider.enabled = true;
        }

        // 稼働開始
        StartCoroutine(TrapRule());
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        // ランナーが吸い込み範囲に入ったかチェック
        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                // まだリストにいなければ、専用のモディファイアを生成してRunnerに付与
                if (!activeVictims.ContainsKey(runner))
                {
                    var modifier = new BlackHoleModifier(runner, transform, maxPullForce, groundResistanceMultiplier, killRadius, pullRadius);
                    runner.AddModifier(modifier);
                    activeVictims.Add(runner, modifier);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // ランナーが吸い込み範囲から出たらモディファイアを解除
        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                if (activeVictims.TryGetValue(runner, out var modifier))
                {
                    runner.RemoveModifier(modifier);
                    activeVictims.Remove(runner);
                }
            }
        }
    }

    // トラップが破壊・消滅した際に、ランナーへの干渉を確実に解除する
    private void OnDisable()
    {
        foreach (var kvp in activeVictims)
        {
            if (kvp.Key != null && kvp.Value != null)
            {
                kvp.Key.RemoveModifier(kvp.Value);
            }
        }
        activeVictims.Clear();
    }

    /// <summary>
    /// Trap 発動条件
    /// </summary>
    public override bool Condition()
    {
        // ブラックホールは設置されたら常に吸い込み続けるので、常に true を返します
        return true;
    }

    /// <summary>
    /// Trap の動作ルール
    /// </summary>
    /// <summary>
    /// Trap の動作ルール
    /// </summary>
    public override IEnumerator TrapRule()
    {
        gameObject.layer = UseLayerName.trapLayer;

        // ★落下を防ぐための最重要ポイント！
        if (rb != null)
        {
            rb.simulated = true;
            // 物理演算はオンにするが、重力の影響を受けない「Static(静的)」状態に変更して空中に固定する
            rb.bodyType = RigidbodyType2D.Static;
        }

        // ブラックホールはモディファイア側で吸い込み処理を行うため、
        // ここではトラップが壊れるまで待機し続けます。
        while (true)
        {
            yield return null;
        }
    }
}

/// <summary>
/// ブラックホールの引力を Runner に適用するモディファイア
/// （BlackHoleTrap.cs の下にそのまま記述してOKです）
/// </summary>
public class BlackHoleModifier : IPlayerMovementModifier
{
    private Runner runner;
    private Transform blackHoleTransform;
    private float maxPullForce;
    private float groundResistanceMultiplier;
    private float killRadius;
    private float maxDistance;

    public BlackHoleModifier(Runner runner, Transform blackHoleTransform, float maxPullForce, float groundResistanceMultiplier, float killRadius, float maxDistance)
    {
        this.runner = runner;
        this.blackHoleTransform = blackHoleTransform;
        this.maxPullForce = maxPullForce;
        this.groundResistanceMultiplier = groundResistanceMultiplier;
        this.killRadius = killRadius;
        this.maxDistance = Mathf.Max(maxDistance, 0.1f); // 0除算防止
    }

    // 毎フレーム Runner の移動処理の中で呼ばれる
    public void ModifyVelocity(ref Vector2 expectedVelocity, float deltaTime)
    {
        // ランナーがすでに死んでいる、またはブラックホールが消えている場合は何もしない
        if (runner == null || blackHoleTransform == null || runner.currentState == Runner.PlayerState.Dead) return;

        Vector2 toCenter = (Vector2)blackHoleTransform.position - (Vector2)runner.transform.position;
        float distance = toCenter.magnitude;

        // 中心（事象の地平線）に到達したら即死
        if (distance <= killRadius)
        {
            runner.Death();
            return;
        }

        // 引力の計算: 中心に近いほど1.0に近づき、縁に近いほど0.0に近づく
        float t = Mathf.Clamp01(1f - (distance / maxDistance));

        // 中心付近で急激に引力が強くなるようにカーブをかける（2乗）
        float intensity = t * t;

        // 基準となる引力
        float currentForce = maxPullForce * intensity;

        // 接地している場合は引力を弱める（踏ん張っている表現）
        if (runner.IsGrounded)
        {
            currentForce *= groundResistanceMultiplier;
        }

        // 引力ベクトルを計算し、ランナーの移動予定速度(Velocity)に加算する
        Vector2 pullVelocity = toCenter.normalized * currentForce;
        expectedVelocity += pullVelocity * deltaTime;

        /* * ※Tips:
         * もし吸い込みが弱すぎる（またはフワフワしすぎる）と感じた場合は、
         * 上の式の `* deltaTime` を外して直接速度を加算するか、
         * maxPullForce の値を大きく調整してみてください。
         */
    }
}