using UnityEngine;
using System.Collections;

/// <summary>
/// 爆弾（Boom）の起動タイミングの種類
/// </summary>
public enum BombActivationType
{
    AfterSetup,     // 設置完了直後にカウント開始（落下中もカウントが進み、空中で爆発可能）
    AfterLanding,   // 着地後にカウント開始
    Proximity       // 着地後、検知範囲内にランナーが入ったらカウント開始
}

/// <summary>
/// 爆発トラップ。
/// 設定されたタイミングで起動し、カウントダウン後に範囲内のランナーを即死させる。
/// </summary>
public class Boom : TiggerTrap
{
    [Header("Bomb Settings")]
    public BombActivationType activationType = BombActivationType.AfterLanding;

    // 落下速度
    public float fallSpeed;

    // 爆発までの待機時間（秒）
    public float waitForBoom = 3.0f;

    // 爆発の即死判定範囲（半径）
    public float boomArea = 2.0f;

    [Header("Proximity Settings")]
    // 起動検知範囲（Proximity の場合のみ有効）
    public float detectionRadius = 3.0f;

    [Header("Visual Effects")]
    // 爆発エフェクト
    public GameObject flame;
    
    // エフェクト開始から実際のダメージ判定までの遅延時間
    public float explosionHitboxDelay = 0.0f;

    // 落下完了フラグ
    private bool fallDone = false;

    // 爆発済みフラグ（落下中の無駄な判定を防ぐ用）
    private bool exploded = false;

    /// <summary>
    /// Trap 初期化
    /// </summary>
    public override void Init()
    {
        base.Init();
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

    /// <summary>
    /// Trap 発動条件 (今回は独自の TrapRule 内で制御するため、基本は fallDone を返す)
    /// </summary>
    public override bool Condition() => fallDone;

    /// <summary>
    /// 徐々に白くなるカウントダウン演出
    /// </summary>
    private IEnumerator FadeToWhiteCountdown()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color originalColor = sr.color;
        float time = 0;

        while (time < waitForBoom)
        {
            time += Time.deltaTime;
            // 徐々に白に近づける
            sr.color = Color.Lerp(originalColor, Color.white, time / waitForBoom);
            yield return null;
        }

        sr.color = Color.white;
    }

    /// <summary>
    /// Trap の動作ルール
    /// </summary>
    public override IEnumerator TrapRule()
    {
        gameObject.layer = UseLayerName.trapLayer;
        rb.simulated = true;

        // 落下処理を開始（待機はしない）
        StartCoroutine(GridFallCoroutine(fallSpeed, () => fallDone = true));

        // 起動タイプ別の待機処理
        if (activationType == BombActivationType.AfterLanding)
        {
            // 着地するまで待機
            yield return new WaitUntil(() => fallDone);
        }
        else if (activationType == BombActivationType.Proximity)
        {
            // 着地するまで待機
            yield return new WaitUntil(() => fallDone);
            // ランナーが検知範囲に入るまで待機
            while (true)
            {
                Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, 1 << UseLayerName.runnerLayer);
                if (hit != null) break;
                yield return null;
            }
        }
        // AfterSetup の場合は即座にここへ到達する

        // カウントダウン演出開始
        yield return StartCoroutine(FadeToWhiteCountdown());

        // 爆発シーケンスへ
        yield return StartCoroutine(ExplosionSequence());
    }

    /// <summary>
    /// 爆発・ダメージ判定処理
    /// </summary>
    private IEnumerator ExplosionSequence()
    {
        exploded = true;

        // 見た目を消す
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // エフェクトの生成
        if (flame != null)
        {
            Instantiate(flame, transform.position, Quaternion.identity);
        }

        // アニメーションとのタイミング調整用待機
        if (explosionHitboxDelay > 0)
        {
            yield return new WaitForSeconds(explosionHitboxDelay);
        }

        // 範囲内のランナーを即死させる
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, boomArea, 1 << UseLayerName.runnerLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<Runner>(out var runner))
            {
                runner.Death();
            }
        }

        yield return new WaitForEndOfFrame();
        BrakeTheTrap();
    }


    /// <summary>
    /// 衝突判定（落下中の直撃用）
    /// </summary>
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;
        if (exploded) return; // すでに爆発処理に入っている場合は無視

        if (!Condition())
        {
            // まだ落下中の場合、直接ぶつかったランナーを倒す
            if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
            {
                base.OnTriggerEnter2D(collision);
            }
        }
    }
}
