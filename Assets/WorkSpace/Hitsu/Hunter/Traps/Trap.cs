using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// Trap の種類を表す列挙型。
/// </summary>
public enum TrapName
{
    Spikes,         // トゲトラップ
    FallRock,       // 落石トラップ
    Boom,           // 爆発トラップ
    JumpPad,
    ChaseEnemy,
    BlackHole,
    Shell,
    FireBar,
    SensorBoom,
    WindTrap,
    PowerArea,
    ReverseArea,
    InvisibleArea,
    IceArea,
    StickyArea,
    ScatterBombSpike,
    ScatterBombIce,
    ScatterBombSticky,
    InkTrap,

    None = -1
}

[System.Serializable]
public class TrapSfxData
{
    /// <summary>
    /// 音效の識別用キー
    /// 将来的には byte や enum による識別に変更することを検討している
    /// </summary>
    public string sfxName;
    /// <summary>
    /// 再生する効果音のAudioClip
    /// </summary>
    public AudioClip clip;

    /// <summary>
    /// 音量（Inspector上で1～100の範囲で調整可能）
    /// </summary>
    [Range(1f, 100f)] public float volume;

}

/// <summary>
/// すべての Trap の基底クラス。
/// 
/// 主な役割：
/// ・Trap の基本データ（種類 / コスト）の管理
/// ・Collider / Rigidbody の管理
/// ・Trap の設置状態の管理
/// 
/// 各 Trap（Spikes / FallRock / Boom）はこのクラスを継承して
/// 個別の挙動を実装する。
/// </summary>
public abstract class Trap : MonoBehaviour
{
    [Header("Trap Basic")]
    // Trap の種類
    public TrapName trapName;
    // Trap のコスト
    public int cost;
    [Header("Trap Sfxs")]
    public TrapSfxData[] trapSfxDates;
    [Header("Physics Components")]
    // Rigidbody2D
    public Rigidbody2D rb;
    // Collider
    public Collider2D trapCollider;
    [Header("Setting Trap")]
    // 設置完了状態
    public bool isSetup = false;
    // 現在のグリッド座標
    public Vector2Int currentGridPos;
    // 初期の配置座標（上昇処理などで記憶しておく用）
    public Vector2Int originGridPos;
    // 出現から発動までの待機時間（旧：フェードインにかかる時間）
    [UnityEngine.Serialization.FormerlySerializedAs("fadeInDuration")]
    public float setupDelay = 1.0f;

    [Header("Setup Effect")]
    [Tooltip("出現〜発動までの間に表示するエフェクト（任意）")]
    public GameObject setupEffectPrefab;
    [Tooltip("エフェクト消滅までのバッファ時間")]
    public float setupEffectBuffer = 1.0f;
    /// <summary>
    /// Trap の初期化処理
    /// </summary>
    public virtual void Init()
    {
        trapCollider = GetComponent<Collider2D>();

        rb = GetComponent<Rigidbody2D>();
        // 初期状態では物理演算を停止
        rb.simulated = false;

    }

    /// Trap を設置する
    /// Collider を有効化し、設置状態にする
    /// </summary>
    public virtual void SetUp()
    {
        //trapCollider.isTrigger = true;
        isSetup = true;

        if (StageGridManager.Instance != null)
        {
            currentGridPos = StageGridManager.Instance.WorldToGrid(transform.position);
            originGridPos = currentGridPos;
            StageGridManager.Instance.RegisterTrap(currentGridPos);
        }

        // 発動前ディレイ開始
        StartCoroutine(SetupDelayCoroutine());
    }

    protected IEnumerator SetupDelayCoroutine()
    {
        gameObject.layer = UseLayerName.trapLayer;
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.layer = UseLayerName.trapLayer;
            }
        }

        // 召喚中（セットアップ遅延中）はスプライトを非表示にする
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            r.enabled = false;
        }

        // セットアップ用エフェクトの生成
        GameObject spawnedEffect = null;
        if (setupEffectPrefab != null)
        {
            spawnedEffect = Instantiate(setupEffectPrefab, transform.position, Quaternion.identity, transform);
        }

        // 指定時間待機
        yield return new WaitForSeconds(setupDelay);

        // 待機完了後、スプライトを再表示する
        foreach (var r in renderers)
        {
            if (r != null)
            {
                r.enabled = true;
            }
        }

        // セットアップ完了時にエフェクトを削除（バッファ時間を設ける）
        if (spawnedEffect != null)
        {
            Destroy(spawnedEffect, setupEffectBuffer);
        }

        // セットアップ完了時の処理を呼び出し
        OnSetupComplete();
    }

    /// <summary>
    /// フェードインを含む設置処理が完全に完了した際に呼ばれる
    /// 各トラップで必要な物理挙動の開始などを実装する
    /// </summary>
    protected virtual void OnSetupComplete()
    {
        // デフォルトでは何もしない（子クラスでオーバーライド）
    }

    protected virtual void Update() { }


    protected virtual void OnDestroy()
    {
        if (isSetup && StageGridManager.Instance != null)
        {
            StageGridManager.Instance.UnregisterTrap(currentGridPos);
            InGame.Instance.RemoveTrap(this.gameObject);
            //HunterConTrollerPad.Instance.UpdateSetupTrapText();
        }
    }

    public virtual void BrakeTheTrap() => Destroy(gameObject);

    /// <summary>
    /// 衝突した GameObject が指定した Layer かどうかを判定する
    /// </summary>
    /// <param name="collision">衝突した Collision2D</param>
    /// <param name="targetLayer">判定する Layer</param>
    /// <returns>同じ Layer の場合 true</returns>
    public bool IsGameObjectLayer(Collider2D collision, int targetLayer) => collision.gameObject.layer == targetLayer;

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        // Runnerに当たった場合のデフォルト動作：死亡させる
        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                runner.Death(collision.ClosestPoint(transform.position));
            }
        }
    }


    public virtual void PlaySfx(string targetSfxName)
    {
        TrapSfxData trapSfxData = Array.Find(trapSfxDates, x => x.sfxName == targetSfxName);
        AudioManager.Instance.PlayTrapSfx(trapSfxData);

    }

}
