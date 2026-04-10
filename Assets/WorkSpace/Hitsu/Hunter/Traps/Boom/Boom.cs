using UnityEngine;
using System.Collections;

public enum BombActivationType
{
    AfterSetup,
    AfterLanding,
    Proximity
}

public class Boom : TiggerTrap
{
    [Header("Bomb Settings")]
    public BombActivationType activationType = BombActivationType.AfterLanding;
    public float fallSpeed;
    public float waitForBoom = 3.0f;
    public float boomArea = 2.0f;

    [Header("Proximity Settings")]
    public float detectionRadius = 3.0f;

    [Header("Visual Effects")]
    [Tooltip("予兆から爆発までがセットになったエフェクト")]
    public GameObject explosionEffect; // ★1つにまとめました

    [Tooltip("爆発後、エフェクトの余韻が終わるまでの待機時間（秒）")]
    public float effectLingerTime = 1.5f;

    public float explosionHitboxDelay = 0.0f;

    private GameObject spawnedEffect; // 生成したエフェクトの保持用
    private bool fallDone = false;
    private bool exploded = false;

    public override void Init() => base.Init();

    public override void SetUp() => base.SetUp();

    protected override void OnSetupComplete()
    {
        StartCoroutine(TrapRule());
    }

    public override bool Condition() => fallDone;

    public override IEnumerator TrapRule()
    {
        gameObject.layer = UseLayerName.trapLayer;
        rb.simulated = true;

        StartCoroutine(GridFallCoroutine(fallSpeed, () => fallDone = true));

        // 起動条件待ち
        if (activationType == BombActivationType.AfterLanding)
        {
            yield return new WaitUntil(() => fallDone);
        }
        else if (activationType == BombActivationType.Proximity)
        {
            yield return new WaitUntil(() => fallDone);
            while (true)
            {
                Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, 1 << UseLayerName.runnerLayer);
                if (hit != null) break;
                yield return null;
            }
        }
        Debug.Log("ba-ka");
        // --- カウントダウン＆エフェクト開始 ---

        // ★予兆から爆発までセットになったエフェクトをここで1回だけ生成
        if (explosionEffect != null)
        {
            spawnedEffect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            spawnedEffect.transform.SetParent(this.transform);
        }

        // 爆発の瞬間（ダメージ判定）までの待機時間
        yield return new WaitForSeconds(waitForBoom);

        // 爆発シーケンスへ
        yield return StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        exploded = true;

        // ★トラップ本体のスプライト（見た目）だけをオフにする
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }

        if (explosionHitboxDelay > 0)
        {
            yield return new WaitForSeconds(explosionHitboxDelay);
        }

        // ダメージ判定
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, boomArea, 1 << UseLayerName.runnerLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<Runner>(out var runner))
            {
                runner.Death();
            }
        }

        // エフェクトの余韻が終わるまで待機
        if (effectLingerTime > 0)
        {
            yield return new WaitForSeconds(effectLingerTime);
        }
        else
        {
            yield return new WaitForEndOfFrame();
        }

        // トラップ本体の破棄（ここで子になっているspawnedEffectも一緒に消滅します）
        BrakeTheTrap();
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;
        if (exploded) return;

        if (!Condition())
        {
            if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
            {
                base.OnTriggerEnter2D(collision);
            }
        }
    }

    // 万が一、途中でトラップが破壊された時のためのクリーンアップ

}