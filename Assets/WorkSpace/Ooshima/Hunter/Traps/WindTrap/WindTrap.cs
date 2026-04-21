using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 風トラップ。
/// 範囲内のランナーを一定方向に押し流す。
/// </summary>
[RequireComponent(typeof(BoxCollider2D))] // 風の範囲は四角い方が設定しやすいのでBoxに変更
public class WindTrap : TiggerTrap
{
    [Header("風トラップ設定")]
    [Tooltip("風の力（マイナス値にすると左向きに吹き飛ばす）")]
    public float windForce = -10f;

    [Tooltip("接地時の風の効きやすさ（0.4なら空中の40%の力しか受けない＝踏ん張れる）")]
    public float groundResistanceMultiplier = 0.4f;

    [Tooltip("エフェクト生成から実際に風が吹き始めるまでの遅延時間")]
    public float activationDelay = 1.0f;

    [Header("Visual Effects")]
    [Tooltip("風トラップが稼働を開始した時に表示するエフェクト")]
    public GameObject activeEffectPrefab;
    private GameObject spawnedEffect;

    // 現在範囲内にいて、モディファイアを付与しているランナーの管理リスト
    private Dictionary<Runner, WindModifier> activeVictims = new Dictionary<Runner, WindModifier>();

    private BoxCollider2D windAreaCollider; // 範囲コライダー

    public override void Init()
    {
        cost = 2; // コストは適宜調整してください
        base.Init();
        // trapName = TrapName.Wind; // 必要に応じてTrapNameを設定

        windAreaCollider = GetComponent<BoxCollider2D>();
        if (windAreaCollider != null)
        {
            // 配置システムに干渉しないように、実体化するまではコライダーをオフにしておく
            windAreaCollider.isTrigger = true;
            windAreaCollider.enabled = false;
        }
    }

    protected override void OnSetupComplete()
    {
        // 稼働開始（エフェクト生成など）
        StartCoroutine(TrapRule());

        // エフェクトが大きくなるまでの遅延待機
        StartCoroutine(ActivateHitboxWithDelay());
    }

    private IEnumerator ActivateHitboxWithDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        // 遅延後に風の判定をオンにする！
        if (windAreaCollider != null)
        {
            windAreaCollider.enabled = true;
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        // ランナーが風の範囲に入ったかチェック
        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                if (!activeVictims.ContainsKey(runner))
                {
                    // 風専用のモディファイアを付与
                    var modifier = new WindModifier(runner, windForce, groundResistanceMultiplier);
                    runner.AddModifier(modifier);
                    activeVictims.Add(runner, modifier);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // ランナーが風の範囲から出たらモディファイアを解除
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

    private void OnDisable()
    {
        // トラップ消滅時に確実に解除
        foreach (var kvp in activeVictims)
        {
            if (kvp.Key != null && kvp.Value != null)
            {
                kvp.Key.RemoveModifier(kvp.Value);
            }
        }
        activeVictims.Clear();


    }

    public override bool Condition()
    {
        return true; // 常に風を吹き続ける
    }

    public override IEnumerator TrapRule()
    {
        gameObject.layer = UseLayerName.trapLayer;

        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Static; // 空中に固定
        }

        // スプライト（本体の画像）を非表示にする
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // 発動エフェクトの生成
        if (activeEffectPrefab != null)
        {
            spawnedEffect = Instantiate(activeEffectPrefab, transform.position, Quaternion.identity);
            spawnedEffect.transform.SetParent(this.transform);
        }

        while (true)
        {
            yield return null;
        }
    }
}

/// <summary>
/// 風の力を Runner に適用するモディファイア
/// </summary>
public class WindModifier : IPlayerMovementModifier
{
    private Runner runner;
    private float windForce;
    private float groundResistanceMultiplier;

    public WindModifier(Runner runner, float windForce, float groundResistanceMultiplier)
    {
        this.runner = runner;
        this.windForce = windForce;
        this.groundResistanceMultiplier = groundResistanceMultiplier;
    }

    // 毎フレーム Runner の移動処理の中で呼ばれる
    public void ModifyVelocity(ref Vector2 expectedVelocity, float deltaTime)
    {
        if (runner == null || runner.currentState == Runner.PlayerState.Dead) return;

        // 基準となる風の力 (X軸方向のみ)
        float currentForce = windForce;

        // 接地している場合は風の力を弱める（踏ん張っている表現）
        if (runner.IsGrounded)
        {
            currentForce *= groundResistanceMultiplier;
        }

        // 速度(Velocity)に加算する（X軸方向のみに力を加える）
        expectedVelocity.x += currentForce * deltaTime;
    }
}