using UnityEngine;
using System.Collections;

/// <summary>
/// イカ墨トラップ
/// 設置後に落下し、着地から2秒後に待機状態へ。
/// 検知範囲に1Pが入ると赤色になり、1Pの150%の速度で慣性付きで追尾する。
/// 追いつくとイカ墨UIを表示して消滅、または追跡開始から5秒後に消滅する。
/// </summary>
public class InkTrap : TiggerTrap
{
    [Header("イカ墨設定")]
    public float fallSpeed = 8f;
    public float acceleration = 15f; // 慣性のための加速度
    public float chaseDuration = 5f; // 追跡開始からの寿命
    [Tooltip("画面に表示するイカ墨のテクスチャ（スプライト）を指定してください")]
    public Sprite inkSprite;

    private Runner targetRunner;
    private float chaseSpeed = 0f;
    private float currentVelocityX = 0f;
    
    private SpriteRenderer[] renderers;
    private Color originalColor;

    private enum State { SetupWait, Searching, Chasing, Falling }
    private State currentState = State.Falling;

    private float stateTimer = 0f;
    private bool isSetupFinished = false;

    public override void Init()
    {
        base.Init();
        trapName = TrapName.InkTrap;
        cost = 3; 
    }

    public override void SetUp() => base.SetUp();

    protected override void OnSetupComplete()
    {
        StartCoroutine(TrapRule());
    }

    public override bool Condition() => currentState == State.Chasing;

    public override IEnumerator TrapRule()
    {
        // --- 設置完了処理 ---
        gameObject.layer = UseLayerName.trapLayer;
        if (gameObject.transform.childCount > 0)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.layer = UseLayerName.trapLayer;
            }
        }

        trapCollider.isTrigger = true; // 物理演算が走る前にTriggerをオンにしてすり抜けバグを防止

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; 
            rb.gravityScale = 0f; 
            rb.freezeRotation = true;
            rb.simulated = true;
        }

        isSetupFinished = true;

        renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length > 0)
        {
            originalColor = renderers[0].color;
            SetAlpha(0.5f); // 準備状態は半透明
        }

        targetRunner = FindObjectOfType<Runner>();
        if (targetRunner != null)
        {
            chaseSpeed = targetRunner.runSpeed * 1.5f;
        }

        // --- 落下と準備のシーケンスへ ---
        yield return StartCoroutine(FallAndSetupSequence());

        // --- 索敵と追尾のシーケンスへ ---
        yield return StartCoroutine(SearchAndChaseSequence());
    }

    private IEnumerator FallAndSetupSequence()
    {
        // --- 落下処理 ---
        currentState = State.Falling;
        bool fallDone = false;
        yield return StartCoroutine(GridFallCoroutine(fallSpeed, () => fallDone = true));

        // --- 着地・準備（SetupWait） ---
        currentState = State.SetupWait;
        float waitTimer = 0.1f; // 着地から2秒待機（元の仕様通り）
        while (waitTimer > 0f)
        {
            if (CheckGroundAndFall()) 
            {
                yield return new WaitUntil(() => currentState == State.SetupWait);
            }
            else
            {
                waitTimer -= Time.deltaTime;
            }
            yield return null;
        }
    }

    private IEnumerator SearchAndChaseSequence()
    {
        // --- センサー開始（Searching） ---
        currentState = State.Searching;
        SetAlpha(1.0f);

        while (currentState == State.Searching)
        {
            if (CheckGroundAndFall()) 
            {
                yield return new WaitUntil(() => currentState == State.Searching);
                continue;
            }

            if (targetRunner == null)
            {
                targetRunner = FindObjectOfType<Runner>();
            }
            else
            {
                float dx = Mathf.Abs(targetRunner.transform.position.x - transform.position.x);
                float dy = Mathf.Abs(targetRunner.transform.position.y - transform.position.y);

                if (dx <= 2.5f && dy <= 1.5f) 
                {
                    // プレイヤー発見 -> 追尾開始
                    currentState = State.Chasing;
                    SetColor(Color.red);
                    stateTimer = chaseDuration; 
                    break; // Searchingループを抜けてChasingへ
                }
            }
            yield return null;
        }

        // --- プレイヤー発見などの処理（Chasing） ---
        while (currentState == State.Chasing)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                Destroy(gameObject);
                yield break;
            }

            // プレイヤーの有無に関わらず、常に足元をチェックして落下する
            if (CheckGroundAndFall())
            {
                // 落下中は待機
                yield return new WaitUntil(() => currentState == State.Chasing);
                continue;
            }

            if (targetRunner != null)
            {
                // 1Pの方向へ加速 (直上の場合はガタつき防止のため目標速度ゼロ)
                float dx = targetRunner.transform.position.x - transform.position.x;
                float targetVelocityX = 0f;
                if (Mathf.Abs(dx) > 0.1f)
                {
                    targetVelocityX = (dx > 0) ? chaseSpeed : -chaseSpeed;
                }
                
                currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, acceleration * Time.deltaTime);

                // 壁のチェックと移動のクランプ
                int direction = currentVelocityX > 0 ? 1 : -1;
                Vector2Int nextSide = currentGridPos + (direction == 1 ? Vector2Int.right : Vector2Int.left);
                
                float nextX = transform.position.x + currentVelocityX * Time.deltaTime;
                
                if (StageGridManager.Instance != null && !StageGridManager.Instance.CanPlaceTrapDataDriven(nextSide))
                {
                    Vector3 currentCellCenter = StageGridManager.Instance.GridToWorld(currentGridPos);
                    float nextDistanceFromCenter = (nextX - currentCellCenter.x) * direction;
                    
                    if (nextDistanceFromCenter > 0.45f) // 壁の直前で止める
                    {
                        nextX = currentCellCenter.x + 0.45f * direction;
                        currentVelocityX = 0f;
                    }
                }

                // 横方向の移動を適用
                transform.position = new Vector3(nextX, transform.position.y, transform.position.z);
            }
            else
            {
                // 見失った場合は再検索
                targetRunner = FindObjectOfType<Runner>();
            }
            yield return null;
        }
    }

    private bool CheckGroundAndFall()
    {
        Vector2Int nextDown = currentGridPos + Vector2Int.down;
        
        if (StageGridManager.Instance != null)
        {
            // 画面外（奈落）の場合も落下処理を開始して、GridFallCoroutine側で消滅させる
            if (StageGridManager.Instance.CanPlaceTrapDataDriven(nextDown) || StageGridManager.Instance.IsOutOfBounds(nextDown))
            {
                State restoreState = currentState;
                currentState = State.Falling;
                currentVelocityX = 0f; // 落下時は横方向の慣性をリセット
                
                StartCoroutine(GridFallCoroutine(fallSpeed, () => {
                    currentState = restoreState;
                }));
                return true; // 落下を開始した
            }
        }
        return false;
    }

    private void SetAlpha(float alpha)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    private void SetColor(Color color)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
        {
            r.color = color;
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSetupFinished) return;

        // 追尾中（Chasing）にRunnerと接触した場合のみ発動
        if (currentState == State.Chasing && IsGameObjectLayer(collision, UseLayerName.runnerLayer))
        {
            if (collision.TryGetComponent<Runner>(out var runner))
            {
                // イカ墨UIを生成
                GameObject inkObj = new GameObject("InkEffectUI");
                var ui = inkObj.AddComponent<InkEffectUI>();
                ui.Setup(inkSprite);

                // 発動後は自身を破棄
                var hp = GetComponent<TrapHp>();
                if (hp != null) hp.Break();
                else Destroy(gameObject);
            }
        }
    }

    // 外部AIの提案に基づき、Updateメソッドを明示的にオーバーライドして座標更新を確実に行います
    protected override void Update()
    {
        // GridMovingTrapのUpdateを呼び出し、currentGridPosを同期させます
        base.Update();
    }
}
