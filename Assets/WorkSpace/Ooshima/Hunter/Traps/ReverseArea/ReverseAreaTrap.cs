using UnityEngine;
using System.Collections.Generic;

public class ReverseAreaTrap : Trap
{
    [Header("Reverse Area Settings")]
    [Tooltip("エリアを出た後、効果が持続する時間(秒)")]
    public float durationAfterExit = 0f; // デフォルトはエリア内のみなど、必要に応じて調整

    private List<ReverseAreaEffect> activeEffects = new List<ReverseAreaEffect>();

    public override void Init()
    {
        base.Init();
        // Area系なのでColliderをTriggerにする
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
                    effect = runner.gameObject.AddComponent<ReverseAreaEffect>();
                    effect.Init(runner, this, durationAfterExit);
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

    public void RemoveEffectRecord(ReverseAreaEffect effect)
    {
        activeEffects.Remove(effect);
    }
}

public class ReverseAreaEffect : MonoBehaviour, IPlayerMovementModifier
{
    public Runner Runner { get; private set; }
    private ReverseAreaTrap trap;
    private float durationAfterExit;

    private bool isExited = false;
    private float exitTimer = 0f;

    public void Init(Runner runner, ReverseAreaTrap trap, float durationAfterExit)
    {
        this.Runner = runner;
        this.trap = trap;
        this.durationAfterExit = durationAfterExit;

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
            // 効果が残る時間を指定していない・もしくは過ぎた場合は消滅
            if (exitTimer <= 0)
            {
                RemoveEffect();
            }
            else
            {
                exitTimer -= Time.deltaTime;
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
        
        if (this != null && gameObject != null)
        {
            Destroy(this);
        }
    }

    // --- IPlayerMovementModifier implementation ---

    public void ModifyInput(ref Vector2 moveInput, ref bool isJumpPressed)
    {
        // 左右移動反転
        moveInput.x = -moveInput.x;

        // -------------------------------------------------------------
        // ジャンプとしゃがみの反転
        // 注: 現在Runner.cs側に「しゃがみ(Crouch)処理」は存在しませんが、
        // 将来的に moveInput.y の下入力などで実装された場合に備え、入力を入れ替えます。
        // -------------------------------------------------------------
        bool originalJump = isJumpPressed;
        bool originalCrouch = moveInput.y < -0.5f;

        // しゃがみ入力されていたならジャンプにする
        isJumpPressed = originalCrouch;
        
        // ジャンプ入力されていたならしゃがみ（下入力）にする
        if (originalJump)
        {
            moveInput.y = -1.0f;
        }
        else if (originalCrouch)
        {
            // ジャンプに変換したので、元のしゃがみ(下入力)は打ち消す
            moveInput.y = 0f;
        }
    }
}
