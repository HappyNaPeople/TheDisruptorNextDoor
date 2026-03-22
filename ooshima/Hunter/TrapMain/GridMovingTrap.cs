using UnityEngine;
using System.Collections;

/// <summary>
/// グリッドベースの移動（落下、上昇、横移動時のグリッド更新など）を行うトラップの基底クラス。
/// 
/// Trap.cs からグリッド移動関連の処理（Updateでの移動検知、GridFallCoroutineなど）を分離し
/// グリッド移動が必要なトラップ（FallRock や ShellTrap など）だけが継承して使用する。
/// </summary>
public abstract class GridMovingTrap : Trap
{
    protected override void Update()
    {
        if (!isSetup || StageGridManager.Instance == null) return;

        Vector2Int newGridPos = StageGridManager.Instance.WorldToGrid(transform.position);
        if (newGridPos != currentGridPos)
        {
            StageGridManager.Instance.MoveTrap(currentGridPos, newGridPos);
            currentGridPos = newGridPos;
        }
    }

    /// <summary>
    /// StageGridManager を使用して安全に1マスずつ落下するコルーチン
    /// </summary>
    /// <param name="speed">落下速度</param>
    /// <param name="onLanded">着地時のコールバック関数</param>
    protected IEnumerator GridFallCoroutine(float speed, System.Action onLanded = null)
    {
        if (StageGridManager.Instance == null)
        {
            onLanded?.Invoke();
            yield break;
        }

        while (true)
        {
            // 1マス下の座標をチェック
            Vector2Int nextGridPos = currentGridPos + Vector2Int.down;
            if (StageGridManager.Instance.CanPlaceTrapDataDriven(nextGridPos))
            {
                // 空いている場合はそこに向かって移動
                Vector3 targetPos = StageGridManager.Instance.GridToWorld(nextGridPos);
                while (Vector3.Distance(transform.position, targetPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                    yield return null;
                }
                // 移動完了後、完全にスナップ
                transform.position = targetPos;
            }
            else
            {
                // ここから先は進めないため着地
                onLanded?.Invoke();
                break;
            }
        }
    }

    /// <summary>
    /// StageGridManager を使用して安全に上方向へマスを戻るコルーチン
    /// </summary>
    /// <param name="targetGridPos">目標とする上方向の座標</param>
    /// <param name="speed">上昇速度</param>
    /// <param name="onReached">目標到達、または上に障害物があって止まった時のコールバック</param>
    protected IEnumerator GridRiseCoroutine(Vector2Int targetGridPos, float speed, System.Action onReached = null)
    {
        if (StageGridManager.Instance == null)
        {
            onReached?.Invoke();
            yield break;
        }

        while (currentGridPos.y < targetGridPos.y)
        {
            // 1マス上の座標をチェック
            Vector2Int nextGridPos = currentGridPos + Vector2Int.up;
            if (StageGridManager.Instance.CanPlaceTrapDataDriven(nextGridPos))
            {
                // 空いている場合は上へ移動
                Vector3 targetWorld = StageGridManager.Instance.GridToWorld(nextGridPos);
                while (Vector3.Distance(transform.position, targetWorld) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetWorld, speed * Time.deltaTime);
                    yield return null;
                }
                transform.position = targetWorld;
            }
            else
            {
                // 何か上に障害物がある場合はそこで停止
                onReached?.Invoke();
                yield break;
            }
        }
        
        // 目標座標に到着
        onReached?.Invoke();
    }
}
