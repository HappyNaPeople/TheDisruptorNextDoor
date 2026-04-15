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
    WindTrap
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
    // フェードインにかかる時間
    public float fadeInDuration = 1.0f;

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

        // フェードイン開始
        StartCoroutine(FadeInCoroutine());
    }

    protected IEnumerator FadeInCoroutine()
    {
        gameObject.layer = UseLayerName.trapLayer;
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.layer = UseLayerName.trapLayer;
            }
        }
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float elapsedTime = 0f;

        // 初期アルファ値を0に設定
        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            color.a = 0f;
            renderer.color = color;
        }

        // フェードイン処理
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);

            foreach (var renderer in renderers)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
            yield return null;
        }

        // アルファ値を1に固定
        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            color.a = 1f;
            renderer.color = color;
        }

        // フェードイン完了時の処理を呼び出し
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
                runner.Death();
            }
        }
    }


    public virtual void PlaySfx(string targetSfxName)
    {
        TrapSfxData trapSfxData = Array.Find(trapSfxDates, x => x.sfxName == targetSfxName);
        AudioManager.Instance.PlayTrapSfx(trapSfxData);

    }

}
