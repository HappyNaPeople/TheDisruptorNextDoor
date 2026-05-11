using UnityEngine;
using System.Collections;

/// <summary>
/// 障害物トラップ（木箱など）。
/// 台座（本体）を Platform レイヤーに設定することで物理的にプレイヤーを遮り、
/// 子オブジェクトの Hitbox を Trap レイヤーに設定することで攻撃（パンチ）を検知可能にする。
/// </summary>
public class ObstacleTrap : GridMovingTrap
{
    [Header("障害物設定")]
    public float fallSpeed = 8f;

    private bool isFalling = false;
    private bool isSetupFinished = false;

    public override void Init()
    {
        base.Init();
        trapName = TrapName.Obstacle;
        if (cost == 0) cost = 1; 
    }

    public override void SetUp()
    {
        // base.SetUp() を呼ぶとレイヤーが trapLayer に強制変更されるため、
        // 必要な処理（isSetupフラグ、グリッド登録、遅延演出）を独自に実装する
        isSetup = true;

        if (StageGridManager.Instance != null)
        {
            currentGridPos = StageGridManager.Instance.WorldToGrid(transform.position);
            originGridPos = currentGridPos;
            StageGridManager.Instance.RegisterTrap(currentGridPos);
        }

        // --- 物理的な衝突の設定 ---
        // プレイヤーをブロックするために Platform レイヤーに固定
        gameObject.layer = UseLayerName.platformLayer;
        
        if (trapCollider != null)
        {
            trapCollider.isTrigger = false;
        }

        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; 
            rb.freezeRotation = true;
        }

        // --- 攻撃検知用の子オブジェクト設定 ---
        SetupHitbox();

        // 演出用のコルーチン（レイヤー変更を行わない独自版）を開始
        StartCoroutine(ObstacleSetupDelayCoroutine());

        isSetupFinished = true;
        CheckGroundAndFall();
    }

    /// <summary>
    /// 基底クラスの SetupDelayCoroutine の代わり。
    /// レイヤーを Platform のまま維持しつつ、フェードイン演出のみを行う。
    /// </summary>
    private IEnumerator ObstacleSetupDelayCoroutine()
    {
        // 出現時は非表示にする
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers) r.enabled = false;

        // 演出用エフェクト
        if (setupEffectPrefab != null)
        {
            Instantiate(setupEffectPrefab, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(setupDelay);

        // 表示
        foreach (var r in renderers) if (r != null) r.enabled = true;

        OnSetupComplete();
    }

    private void SetupHitbox()
    {
        // 既存の Hitbox を探す
        Transform hitbox = transform.Find("Hitbox");
        if (hitbox == null)
        {
            GameObject hbObj = new GameObject("Hitbox");
            hbObj.transform.SetParent(this.transform);
            hbObj.transform.localPosition = Vector3.zero;
            hbObj.transform.localScale = Vector3.one;
            hitbox = hbObj.transform;
        }

        // 子オブジェクトを Trap レイヤーに設定（パンチが当たるように）
        hitbox.gameObject.layer = UseLayerName.trapLayer;

        // 子オブジェクトにコライダーを設定
        BoxCollider2D col = hitbox.GetComponent<BoxCollider2D>();
        if (col == null) col = hitbox.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        
        if (trapCollider is BoxCollider2D parentBox)
        {
            col.size = parentBox.size;
            col.offset = parentBox.offset;
        }

        // TrapHp を子オブジェクトへ移動
        TrapHp rootHp = GetComponent<TrapHp>();
        if (rootHp != null)
        {
            TrapHp childHp = hitbox.gameObject.GetComponent<TrapHp>();
            if (childHp == null) childHp = hitbox.gameObject.AddComponent<TrapHp>();
            
            childHp.hp = rootHp.hp;
            childHp.defenseMultiplier = rootHp.defenseMultiplier;
            childHp.consumeHpOverTime = rootHp.consumeHpOverTime;
            
            // ルートの TrapHp は不要（またはあっても良いが、重複を避けるため削除）
            Destroy(rootHp);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (!isSetupFinished || isFalling) return;
        CheckGroundAndFall();
    }

    private bool CheckGroundAndFall()
    {
        if (isFalling) return false;
        Vector2Int nextDown = currentGridPos + Vector2Int.down;

        if (StageGridManager.Instance != null)
        {
            if (StageGridManager.Instance.CanPlaceTrapDataDriven(nextDown) || StageGridManager.Instance.IsOutOfBounds(nextDown))
            {
                isFalling = true;
                StartCoroutine(GridFallCoroutine(fallSpeed, () => {
                    isFalling = false;
                }));
                return true;
            }
        }
        return false;
    }

    public override void OnTriggerEnter2D(Collider2D collision) { }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            float size = 1.0f;
            Vector2 offset = Vector2.zero;
            StageGridManager manager = StageGridManager.Instance;
            if (manager == null) manager = FindFirstObjectByType<StageGridManager>();
            if (manager != null)
            {
                size = manager.gridSize;
                offset = manager.gridOffset;
            }
            Vector3 pos = transform.position;
            pos.x = Mathf.Floor((pos.x - offset.x) / size + 0.5f) * size + offset.x;
            pos.y = Mathf.Floor((pos.y - offset.y) / size + 0.5f) * size + offset.y;
            transform.position = pos;
        }
    }
#endif
}
