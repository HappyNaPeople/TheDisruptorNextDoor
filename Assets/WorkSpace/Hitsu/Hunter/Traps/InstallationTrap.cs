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



}
