using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScatterBombTrap : Trap
{
    [Header("Scatter Settings")]
    [Tooltip("ばらまく対象のプレハブ（氷床など）")]
    public GameObject prefabToScatter;
    
    [Tooltip("ばらまく対象マスの割合（1.0で範囲内の全マス、0.5で半分）")]
    [Range(0f, 1f)]
    public float scatterRatio = 0.5f;

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

        // ボム自身があるグリッドを先に「空き（登録解除）」にしておくことで、
        // ボムのあった中心位置にも子トラップを置けるようにする
        if (StageGridManager.Instance != null)
        {
            StageGridManager.Instance.UnregisterTrap(transform.position);
        }

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

                // 該当マスのうち指定割合を対象にしつつ、最大数を制限
                int calculatedCount = Mathf.FloorToInt(validGrids.Count * scatterRatio);
                int targetCount = Mathf.Min(maxScatterCount, Mathf.Max(1, calculatedCount));
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        StageGridManager gridManager = StageGridManager.Instance;
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<StageGridManager>();
        }

        if (gridManager != null)
        {
            Vector2Int centerGrid = gridManager.WorldToGrid(transform.position);

            for (int x = centerGrid.x - scatterRadius; x <= centerGrid.x + scatterRadius; x++)
            {
                for (int y = centerGrid.y - scatterRadius; y <= centerGrid.y + scatterRadius; y++)
                {
                    Vector2Int gridCoord = new Vector2Int(x, y);
                    if (Vector2.Distance((Vector2)centerGrid, (Vector2)gridCoord) <= scatterRadius)
                    {
                        Vector3 gridPos = gridManager.GridToWorld(gridCoord);
                        
                        // 枠線を描画
                        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
                        Gizmos.DrawWireCube(gridPos, new Vector3(gridManager.gridSize, gridManager.gridSize, 0));
                        
                        // 塗りつぶしを描画
                        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
                        Gizmos.DrawCube(gridPos, new Vector3(gridManager.gridSize, gridManager.gridSize, 0));
                    }
                }
            }
        }
        else
        {
            // GridManagerがない場合のフォールバック
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, scatterRadius);
        }
    }
#endif
}
