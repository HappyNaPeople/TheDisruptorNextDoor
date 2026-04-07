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

    private readonly Vector2 colliderOffset = new Vector2(0, 0.4232323f);
    private readonly Vector2 colliderSize = new Vector2(1, 0.1535354f);

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
        boxPart.enabled = false;
    }

    public override IEnumerator FallAndSetUp()
    {
        yield return StartCoroutine(base.FallAndSetUp());

        ((BoxCollider2D)trapCollider).offset = colliderOffset;
        ((BoxCollider2D)trapCollider).size = colliderSize;

        boxPart.gameObject.layer = UseLayerName.platformLayer;
        boxPart.enabled = true;
    }


    /// <summary>
    public override void SetUp()
    {
        base.SetUp();
        // 設置指示を出す（フェードイン開始）
    }

    protected override void OnSetupComplete()
    {
        // 実体化完了後に落下開始
        StartCoroutine(FallAndSetUp());
    }

    /// <summary>
    /// 衝突判定
    /// </summary>
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetup) return;

        // Runner に当たった場合
        if (IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            animator.Play("JumpPad_GotActive");
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                runner.ExecuteJump(direction);
            }
        }
        // 地面または Trap に当たった場合
        // else if ((IsGameObjectLayer(collision, UseLayerName.trapLayer) || IsGameObjectLayer(collision, UseLayerName.platformLayer)) && !isFallDone)
        // {
        //     isFallDone = true;
        //     return;
        // }
    }



}
