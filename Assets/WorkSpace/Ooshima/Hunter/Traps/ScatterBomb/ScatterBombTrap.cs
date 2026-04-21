using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScatterBombTrap : Trap
{
    [Header("Scatter Settings")]
    [Tooltip("ばらまく対象のプレハブ（氷床など）")]
    public GameObject prefabToScatter;
    
    [Tooltip("一度にばらまく最大数")]
    public int maxScatterCount = 5;

    [Tooltip("ばらまく範囲の半径（グリッド数）")]
    public int scatterRadius = 3;

    [Tooltip("起爆までの待機時間（秒）")]
    public float waitForBoom = 1.0f;

    [Header("Visual Effects")]
    public GameObject explosionEffect;
    public float effectLingerTime = 1.0f;

    public override void Init()
    {
        base.Init();
        cost = 2; // ボム相当
    }

    protected override void OnSetupComplete()
    {
        // 落下処理があればここで行うが、ボムとしてその場に留まるのでそのまま起爆
        // （もし落下させたい場合は InstallationTrap を継承して FallAndSetUp を呼ぶ）
        StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        yield return new WaitForSeconds(waitForBoom);

        // エフェクト生成
        if (explosionEffect != null)
        {
            var effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectLingerTime + 0.5f);
        }

        // 見た目だけを非表示に（判定も消す場合はColliderもdisable）
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
        if (trapCollider != null) trapCollider.enabled = false;

        // グリッドを用いたばらまき処理
        if (StageGridManager.Instance != null && prefabToScatter != null)
        {
            Vector2Int centerGrid = StageGridManager.Instance.WorldToGrid(transform.position);
            
            // 床・壁に接する空きマスの取得
            List<Vector2Int> validGrids = StageGridManager.Instance.GetEmptyGridsWithWallOrFloor(centerGrid, scatterRadius);
            
            if (validGrids.Count > 0)
            {
                // シャッフル
                for (int i = 0; i < validGrids.Count; i++)
                {
                    int rnd = Random.Range(i, validGrids.Count);
                    var temp = validGrids[i];
                    validGrids[i] = validGrids[rnd];
                    validGrids[rnd] = temp;
                }

                // 該当マスのうちの半分を対象にしつつ、最大数を制限
                int targetCount = Mathf.Min(maxScatterCount, Mathf.Max(1, validGrids.Count / 2));
                int spawnedCount = 0;

                foreach (var grid in validGrids)
                {
                    if (spawnedCount >= targetCount) break;

                    // 該当グリッドのワールド座標
                    Vector3 spawnPos = StageGridManager.Instance.GridToWorld(grid);
                    
                    // 向き（回転）を決定するために４方向チェック
                    Quaternion rotation = GetSurfaceRotationDirection(spawnPos);

                    // 生成
                    var trapObj = Instantiate(prefabToScatter, spawnPos, rotation);

                    // 罠として初期化（生成されたTrapコンポーネントがあれば設定・フェードインさせる）
                    if (trapObj.TryGetComponent<Trap>(out var childTrap))
                    {
                        childTrap.Init();
                        childTrap.SetUp(); // インスタンス化の時点でグリッド登録やフェードインが行われる
                    }

                    spawnedCount++;
                }
            }
        }

        if (effectLingerTime > 0)
        {
            yield return new WaitForSeconds(effectLingerTime);
        }

        BrakeTheTrap();
    }

    private Quaternion GetSurfaceRotationDirection(Vector3 pos)
    {
        float gridSize = StageGridManager.Instance != null ? StageGridManager.Instance.gridSize : 1f;
        int layerMask = 1 << UseLayerName.platformLayer;
        
        // WallTrapPlacerと同じ基準で向きを判定
        if (Physics2D.OverlapPoint(pos + Vector3.down * gridSize, layerMask))
            return Quaternion.Euler(0, 0, 0);       // 床
        if (Physics2D.OverlapPoint(pos + Vector3.up * gridSize, layerMask))
            return Quaternion.Euler(0, 0, 180);     // 天井
        if (Physics2D.OverlapPoint(pos + Vector3.right * gridSize, layerMask))
            return Quaternion.Euler(0, 0, 90);      // 右壁
        if (Physics2D.OverlapPoint(pos + Vector3.left * gridSize, layerMask))
            return Quaternion.Euler(0, 0, -90);     // 左壁

        return Quaternion.identity;
    }
}
