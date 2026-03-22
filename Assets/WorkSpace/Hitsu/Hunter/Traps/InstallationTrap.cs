using UnityEngine;
using System.Collections;


/// <summary>
/// 設置型 Trap の基底クラス。
/// 
/// Trap を空中から落下させ、
/// 地面または他の Trap に接触したら設置を完了する。
/// </summary>
public abstract class InstallationTrap : Trap
{
    // 落下速度
    private const int fallSpeed = 5;
    // 落下完了フラグ
    public bool isFallDone = false;

    /// <summary>
    /// Trap を落下させて設置するコルーチン
    /// </summary>
    public virtual IEnumerator FallAndSetUp()
    {
        // 物理演算を有効化
        rb.simulated = true;
        // 落下処理
        yield return StartCoroutine(GridFallCoroutine(fallSpeed, () => isFallDone = true));
        // Trap レイヤー設定
        gameObject.layer = UseLayerName.trapLayer;
        if (gameObject.transform.childCount > 0)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.layer = UseLayerName.trapLayer;
            }
        }

        // Rigidbody を固定
        rb.bodyType = RigidbodyType2D.Static;
        // Rigidbody を削除
        Destroy(rb);
        gameObject.layer = UseLayerName.trapLayer;
        yield return null;
    }



}
