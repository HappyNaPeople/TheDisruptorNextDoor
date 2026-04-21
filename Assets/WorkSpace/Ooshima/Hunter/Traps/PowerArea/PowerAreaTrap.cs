using UnityEngine;
using System.Collections.Generic;

public class PowerAreaTrap : Trap
{
    [Header("Power Area Settings")]
    [Tooltip("移動速度の倍率 (例: 3.0で300%)")]
    public float speedMultiplier = 3.0f;
    [Tooltip("Damping(慣性)の値 (小さいほど滑る)")]
    public float dampingValue = 1.0f;
    [Tooltip("エリアを出た後、効果が持続する時間(秒)")]
    public float durationAfterExit = 3.0f;

    private List<PowerAreaEffect> activeEffects = new List<PowerAreaEffect>();

    public override void Init()
    {
        base.Init();
        // Area系なのでColliderをTriggerにする（念のため）
        if (trapCollider != null)
        {
            trapCollider.isTrigger = true;
            trapCollider.enabled = false; // 設置前・フェードイン中の誤検知防止
        }
    }

    protected override void OnSetupComplete()
    {
        base.OnSetupComplete();
        
        // 物理計算(Trigger判定)を復活させるため、RigidbodyをStatic化してONにする
        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Static; // 落ちないように固定
        }

        // 設置完了（フェードイン完了）後に判定をオンにする
        if (trapCollider != null)
        {
            trapCollider.enabled = true;
        }
    }


    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                var effect = activeEffects.Find(e => e.Runner == runner);
                if (effect == null)
                {
                    effect = runner.gameObject.AddComponent<PowerAreaEffect>();
                    effect.Init(runner, this, durationAfterExit, speedMultiplier, dampingValue);
                    activeEffects.Add(effect);
                }
                effect.NotifyEnter();
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!isSetup) return;

        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                var effect = activeEffects.Find(e => e.Runner == runner);
                if (effect != null)
                {
                    effect.NotifyExit();
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // トラップが寿命で消えるか壊された場合、中立のプレイヤーには効果終了のカウントダウンを始めさせる
        foreach (var effect in activeEffects)
        {
            if (effect != null)
            {
                effect.NotifyExit();
            }
        }
    }

    public void RemoveEffectRecord(PowerAreaEffect effect)
    {
        activeEffects.Remove(effect);
    }
}

public class PowerAreaEffect : MonoBehaviour, IPlayerMovementModifier
{
    public Runner Runner { get; private set; }
    private PowerAreaTrap trap;
    private float durationAfterExit;
    private float speedMultiplier;
    private float dampingValue;

    private bool isExited = false;
    private float exitTimer = 0f;

    public void Init(Runner runner, PowerAreaTrap trap, float durationAfterExit, float speedMultiplier, float dampingValue)
    {
        this.Runner = runner;
        this.trap = trap;
        this.durationAfterExit = durationAfterExit;
        this.speedMultiplier = speedMultiplier;
        this.dampingValue = dampingValue;

        runner.AddModifier(this);
    }

    public void NotifyEnter()
    {
        isExited = false;
    }

    public void NotifyExit()
    {
        if (!isExited)
        {
            isExited = true;
            exitTimer = durationAfterExit;
        }
    }

    private void Update()
    {
        if (isExited)
        {
            exitTimer -= Time.deltaTime;
            if (exitTimer <= 0)
            {
                RemoveEffect();
            }
        }
    }

    private void OnDestroy()
    {
        RemoveEffect();
    }

    private void RemoveEffect()
    {
        if (Runner != null)
        {
            Runner.RemoveModifier(this);
        }
        if (trap != null)
        {
            trap.RemoveEffectRecord(this);
        }
        
        // Destroy()を繰り返さないようチェック
        if (this != null && gameObject != null)
        {
            Destroy(this);
        }
    }

    // --- IPlayerMovementModifier parameters ---

    public float ModifyRunSpeed(float baseSpeed)
    {
        return baseSpeed * speedMultiplier;
    }

    public float ModifyDamping(float baseDamping)
    {
        // 慣性を強く働かせるため、低い値を返す
        return dampingValue;
    }
}
