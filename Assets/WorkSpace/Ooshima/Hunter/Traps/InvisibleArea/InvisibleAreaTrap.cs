using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// インビジブルエリア（Invisible Area）トラップ
/// 指定した範囲内にランナーが入ると不可視化効果を付与する。
/// このトラップ自体の寿命は8秒。
/// 退出してから5秒間、効果が継続する。
/// </summary>
public class InvisibleAreaTrap : Trap
{
    [Header("Invisible Area Settings")]
    [Tooltip("トラップ自体の持続時間")]
    public float trapDuration = 8f;
    
    [Tooltip("エリアから出てからの不可視化継続時間")]
    public float effectDurationAfterExit = 5f;

    private float _timer = 0f;
    
    // エリア内にいるランナーにアタッチしたModifierを保持しておくためのリスト
    private List<InvisibleEffectModifier> activeModifiers = new List<InvisibleEffectModifier>();

    public override void Init()
    {
        base.Init();
        
        trapName = TrapName.InvisibleArea;
        
        // 当たり判定の設定（幅7、高さ3の長方形）
        if (trapCollider is BoxCollider2D boxCol)
        {
            boxCol.size = new Vector2(7f, 3f);
            boxCol.isTrigger = true;
        }
    }

    protected override void OnSetupComplete()
    {
        base.OnSetupComplete();
        _timer = 0f;

        // トラップ配置システムで無効化されていたコライダーや物理演算を設置完了後に有効化する
        if (trapCollider != null) trapCollider.enabled = true;
        if (rb != null) 
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Static; // 空中に固定 (落下させない)
        }
        
        // エリアのSpriteなどを変更する（「エリアの色を変えることで効果範囲であることを示唆」）
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            Color c = sr.color;
            c.a = 0.5f; // 仮の半透明でエリア表示
            sr.color = c;
        }
    }

    protected override void Update()
    {
        base.Update();
        
        if (!isSetup) return;

        // トラップの寿命をカウント
        _timer += Time.deltaTime;
        if (_timer >= trapDuration)
        {
            // 寿命がきたので消滅
            BrakeTheTrap();
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // ※このトラップではRunnerをキルしないため base.OnTriggerEnter2D は呼ばない
        if (!isSetup) return;

        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                // RunnerにInvisibleEffectModifierがついているかチェック。なければ付ける
                InvisibleEffectModifier modifier = runner.GetComponent<InvisibleEffectModifier>();
                if (modifier == null)
                {
                    modifier = runner.gameObject.AddComponent<InvisibleEffectModifier>();
                }
                
                modifier.EnterArea();
                
                if (!activeModifiers.Contains(modifier))
                {
                    activeModifiers.Add(modifier);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isSetup) return;

        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                InvisibleEffectModifier modifier = runner.GetComponent<InvisibleEffectModifier>();
                if (modifier != null)
                {
                    modifier.ExitArea(effectDurationAfterExit);
                    activeModifiers.Remove(modifier);
                }
            }
        }
    }

    public override void BrakeTheTrap()
    {
        // トラップが壊れる際、まだエリアにいるランナーがいれば退出扱いにしてカウントダウンを開始させる
        foreach (var modifier in activeModifiers)
        {
            if (modifier != null)
            {
                modifier.ExitArea(effectDurationAfterExit);
            }
        }
        activeModifiers.Clear();

        base.BrakeTheTrap();
    }
}
