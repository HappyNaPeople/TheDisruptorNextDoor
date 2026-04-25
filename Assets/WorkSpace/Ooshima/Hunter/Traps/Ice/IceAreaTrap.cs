using UnityEngine;
using System.Collections.Generic;

public class IceAreaTrap : InstallationTrap
{
    private List<IceAreaEffect> activeEffects = new List<IceAreaEffect>();

    public override void Init()
    {
        base.Init();
        trapName = TrapName.IceArea;
        cost = 1;
        if (trapCollider != null)
        {
            trapCollider.isTrigger = true;
        }
    }

    protected override void OnSetupComplete()
    {
        gameObject.layer = UseLayerName.trapLayer;
        if (gameObject.transform.childCount > 0)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.layer = UseLayerName.trapLayer;
            }
        }
        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Static;
            Destroy(rb);
        }
        isFallDone = true;
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
                    effect = runner.gameObject.AddComponent<IceAreaEffect>();
                    effect.Init(runner, this);
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
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i] != null)
            {
                activeEffects[i].NotifyExit();
            }
        }
    }

    public void RemoveEffectRecord(IceAreaEffect effect)
    {
        activeEffects.Remove(effect);
    }
}

public class IceAreaEffect : MonoBehaviour, IPlayerMovementModifier
{
    public Runner Runner { get; private set; }
    private IceAreaTrap trap;
    private bool isExited = false;

    public void Init(Runner runner, IceAreaTrap trap)
    {
        this.Runner = runner;
        this.trap = trap;
        runner.AddModifier(this);
    }

    public void NotifyEnter()
    {
        isExited = false;
    }

    public void NotifyExit()
    {
        // 氷床の場合、ジャンプ中（空中にいる）なら着地まで滑りを継続させるか、
        // もしくは一度エリアから離れて地面にいたら効果を切る。
        // （仕様が未定のため、即時に切る設定。ジャンプ慣性はRunnerのinAirDampingがあるため一旦標準のままで対応）
        if (!isExited)
        {
            isExited = true;
            RemoveEffect();
        }
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

    public float ModifyDamping(float baseDamping)
    {
        // 摩擦を 0.3 相当などに設定（通常の groundDamping が 20 なら 0.3 は非常に滑る）
        // ユーザー指定「床面の摩擦係数を0.3に設定する」
        return 0.3f;
    }
}
