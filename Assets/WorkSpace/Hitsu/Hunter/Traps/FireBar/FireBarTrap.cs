using UnityEngine;
using System.Collections;

/// <summary>
/// 固定型トラップ：ファイヤーバー (FireBar)
/// 中央のブロック（台座）はプラットフォーム扱いで固定され、反時計回りに炎の柱を回転させる。
/// </summary>
public class FireBarTrap : Trap // InstallationTrapではなくTrapを継承し、落下させない
{
    [Header("FireBar Settings")]
    [Tooltip("回転速度（度/秒）※正の値で反時計回り")]
    public float rotationSpeed = 90f; 
    
    [Tooltip("回転させる火柱の親オブジェクト（台座ではなく子オブジェクトを設定してください）")]
    public Transform firePillar; 

    private void Awake()
    {
        // プレビュー中に勝手に回ってしまうのを防ぐため、生成直後は強制的に未設置状態にする
        isSetup = false;
    }

    public override void Init()
    {
        base.Init();
        trapName = TrapName.FireBar;
        cost = 3; // 仮のコスト設定
        
        // 台座自身（自分自身）がセットされている場合、台座が回ってしまうためリセットする
        if (firePillar == transform)
        {
            Debug.LogWarning("FireBarTrap: 台座自身が回転するように設定されてしまっています！子オブジェクトの『FirePillar』を探します。");
            firePillar = null;
        }

        // firePillarが未設定の場合、子オブジェクトから自動検索する
        if (firePillar == null)
        {
            Transform found = transform.Find("FirePillar");
            if (found != null)
            {
                firePillar = found;
            }
            else
            {
                Debug.LogWarning("FireBarTrap: FirePillar Transform object is missing.");
            }
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        
        // --- 台座（ルートオブジェクト）の設定 ---
        // 落下しない固定トラップのため、台座をプラットフォーム（足場）レイヤーに変更
        gameObject.layer = UseLayerName.platformLayer;
        // 台座には乗れるようにするため、トリガーを解除する（Trap.csのbase.SetUpでtrueにされるのを上書き）
        if (trapCollider != null) trapCollider.isTrigger = false;

        // 物理演算（重力等）の影響を受けないようにStaticにする
        // ※Destroy(rb)をすると子オブジェクトのトリガー判定が親に伝わらなくなるため、Staticのまま残す
        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // 強制的にグリッドの中央にスナップさせて、わずかなズレを修正する
        if (StageGridManager.Instance != null)
        {
            transform.position = StageGridManager.Instance.GridToWorld(currentGridPos);
        }

        // --- 火柱（FirePillar）とその子供（炎の丸スプライト達）の設定 ---
        if (firePillar != null)
        {
            firePillar.gameObject.layer = UseLayerName.trapLayer;
            foreach (Transform child in firePillar)
            {
                child.gameObject.layer = UseLayerName.trapLayer;
                
                // 炎の当たり判定はダメージ用なのでトリガーにする
                Collider2D col = child.GetComponent<Collider2D>();
                if (col != null) col.isTrigger = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        // Runner に当たった場合（Spikes等と同様の処理）
        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            Debug.Log("FireBar Hit Runner");
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // 設置前（ドラッグ中のプレビュー状態など）には回転させない
        if (!isSetup) return;

        // 反時計回り (Z軸プラス方向) に回転させる
        if (firePillar != null)
        {
            firePillar.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }
}
