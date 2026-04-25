using UnityEngine;
using System.Collections.Generic;

public class StickyAreaTrap : InstallationTrap
{
    private List<StickyAreaEffect> activeEffects = new List<StickyAreaEffect>();

    public override void Init()
    {
        base.Init();
        trapName = TrapName.StickyArea;
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
                    effect = runner.gameObject.AddComponent<StickyAreaEffect>();
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

    public void RemoveEffectRecord(StickyAreaEffect effect)
    {
        activeEffects.Remove(effect);
    }
}

public class StickyAreaEffect : MonoBehaviour, IPlayerMovementModifier
{
    public Runner Runner { get; private set; }
    private StickyAreaTrap trap;
    private bool isExited = false;

    public void Init(Runner runner, StickyAreaTrap trap)
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

    public float ModifyJumpHeight(float baseJumpHeight)
    {
        return baseJumpHeight * 0.5f;
    }

    public float ModifyRunSpeed(float baseSpeed)
    {
        return baseSpeed * 0.7f;
    }
}
