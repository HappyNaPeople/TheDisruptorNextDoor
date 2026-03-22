using UnityEngine;
using System.Collections;

/// <summary>
/// 甲羅トラップ。
/// 設置後に落下し、着地後はプレイヤーの方向へ進む。
/// 地面や壁、他のトラップに当たると進行方向を反転する。
/// 移動中に足元の地面がなくなると再度落下する。
/// </summary>
public class ShellTrap : GridMovingTrap
{
    [Header("移動設定")]
    public float fallSpeed = 8f;
    public float moveSpeed = 3f;

    private int direction = 0; // 0 = not moving, 1 = right, -1 = left
    private bool isFalling = false;
    private bool isSetupFinished = false;

    public override void Init()
    {
        base.Init();
        trapName = TrapName.Shell; 
        cost = 2; // コストは仮です、よしなに変更してください
    }

    public override void SetUp()
    {
        base.SetUp();
        
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
            // 物理挙動は自前で行うため、Gravityは0。KinematicにするとTriggerが反応しづらいためDynamic
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; 
            rb.freezeRotation = true;
        }

        trapCollider.isTrigger = true;
        isSetupFinished = true;
        
        // 空中設置やブロック上設置に関わらず、最初は落下状態からスタートする
        StartFalling();
    }

    private void StartFalling()
    {
        if (isFalling) return;
        isFalling = true;
        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        bool fallDone = false;
        
        // Trap基底クラスのGridFallCoroutineを使って落下
        yield return StartCoroutine(GridFallCoroutine(fallSpeed, () => fallDone = true));

        // 着地時に進行方向を決定（初回のみプレイヤーの方向を参照）
        // 落下途中で方向が変わったりしないように初回着地時のみ判定
        if (direction == 0) 
        {
            Runner targetRunner = FindObjectOfType<Runner>();
            if (targetRunner != null)
            {
                direction = targetRunner.transform.position.x < transform.position.x ? -1 : 1;
            }
            else
            {
                direction = 1;
            }
        }

        isFalling = false;
    }

    protected override void Update()
    {
        // currentGridPos の更新
        base.Update();

        if (!isSetupFinished) return;

        if (!isFalling)
        {
            // Updateによって最新化された currentGridPos を使って足元が空いているかチェック
            Vector2Int nextDown = currentGridPos + Vector2Int.down;
            if (StageGridManager.Instance != null && StageGridManager.Instance.CanPlaceTrapDataDriven(nextDown))
            {
                // 地面がなくなったので再び落下状態へ
                StartFalling();
            }
            else
            {
                // 横方向の壁や他のトラップ検知（StageGridManagerを使用して次のマスをチェック）
                Vector2Int nextSide = currentGridPos + (direction == 1 ? Vector2Int.right : Vector2Int.left);
                
                if (StageGridManager.Instance != null && !StageGridManager.Instance.CanPlaceTrapDataDriven(nextSide))
                {
                    // 次のマスが塞がっている場合、現在マスの境界付近で跳ね返る
                    Vector3 currentCellCenter = StageGridManager.Instance.GridToWorld(currentGridPos);
                    
                    // 現在のセルの中心から進行方向へどれだけ進んでいるかを計算
                    float distanceFromCenter = (transform.position.x - currentCellCenter.x) * direction;
                    
                    // マスの境界（想定0.5f）より少し手前で反転させることで、壁へのめり込みを防ぐ
                    if (distanceFromCenter > 0.45f)
                    {
                        direction *= -1; // 進行方向を反転
                    }
                }

                // 左右連続移動
                transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;
            }
        }
    }
}
