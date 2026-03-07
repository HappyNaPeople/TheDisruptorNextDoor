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
    private bool isFallDone = false;
    /// <summary>
    /// Trap を落下させて設置するコルーチン
    /// </summary>
    public virtual IEnumerator FallAndSetUp()
    {
        // 物理演算を有効化
        rb.simulated = true;
        // 落下処理
        while (!isFallDone)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;
        }
        // Trap レイヤー設定
        gameObject.layer = UseLayerName.trapLayer;
        // Rigidbody を固定
        rb.bodyType = RigidbodyType2D.Static;
        // Rigidbody を削除
        Destroy(rb);
        yield return null;
    }

    /// <summary>
    /// 衝突判定
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // まだ設置されていない場合は処理しない
        if (!isSetup) return;

        // Runner に当たった場合
        if (IsGameObjectLayer(collision,UseLayerName.runnerLayer))
        {

            Debug.Log("Hit Runner");
        }
        // 地面または Trap に当たった場合
        else if ((IsGameObjectLayer(collision, UseLayerName.trapLayer) || IsGameObjectLayer(collision, UseLayerName.mapLayer)) && !isFallDone)
        {
            isFallDone = true;
            rb.bodyType = RigidbodyType2D.Static;
            return;
        }
    }

}
