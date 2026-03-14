using UnityEngine;
using System.Collections;

public class JumpPad : InstallationTrap
{
    /// <summary>
    /// 押し出す力
    /// </summary>
    public int pushForce;

    /// <summary>
    /// Trap の向きに応じた押し出し方向
    /// transform.up に pushForce を掛けたベクトル
    /// </summary>
    public Vector3 direction => (Vector2)transform.up * pushForce;

    public Collider2D boxPart;
    /// <summary>
    /// Animator コンポーネント
    /// </summary>
    private Animator animator;

    /// <summary>
    /// Trap 初期化
    /// </summary>
    public override void Init()
    {
        base.Init();
        trapName = TrapName.JumpPad;
        // 設置コスト
        cost = 1;
        animator = GetComponent<Animator>();
    }

    public override IEnumerator FallAndSetUp()
    {
        yield return StartCoroutine(base.FallAndSetUp());

        boxPart.gameObject.layer = UseLayerName.platformLayer;
    }


    /// <summary>
    /// Trap 設置処理
    /// </summary>
    public override void SetUp()
    {
        base.SetUp();
        // 落下して設置
        StartCoroutine(FallAndSetUp());
    }

    /// <summary>
    /// 衝突判定
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        // Runner に当たった場合
        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            animator.Play("JumpPad_GotActive");
        }
        // 地面または Trap に当たった場合
        else if ((IsGameObjectLayer(collision, UseLayerName.trapLayer) || IsGameObjectLayer(collision, UseLayerName.platformLayer)) && !isFallDone)
        {
            isFallDone = true;
            return;
        }
    }



}
