using UnityEngine;

public class BlackHoleTrap : Trap
{
    [Header("Black Hole Settings")]
    [Tooltip("吸い込みの影響を受ける範囲 (Radius of the pull effect)")]
    public float pullRadius = 15f;
    [Tooltip("最大吸い込み力 (Maximum pull strength when at center)")]
    public float maxPullForce = 50f;
    [Tooltip("右方向への加速度 (Acceleration in +X direction)")]
    public float acceleration = 2f; // 遅くした
    [Tooltip("中心のダメージ判定の半径 (Radius of the damage hitbox at center)")]
    public float damageHitboxRadius = 1f;
    [Tooltip("消滅するまでの移動距離 (Travel distance before disappearing)")]
    public float maxTravelDistance = 70f;

    [Header("Visuals (Placeholder)")]
    public ParticleSystem warningParticles;
    public ParticleSystem coreParticles;

    // The Runner programmer will read this property to apply the movement
    public Vector2 CurrentPullForce { get; private set; }

    private float currentSpeed = 0f;
    private Runner targetRunner;
    private float startX;

    public override void Init()
    {
        base.Init();
        currentSpeed = 0f;
        CurrentPullForce = Vector2.zero;
        startX = transform.position.x;

        if (InGame.Instance != null && InGame.Instance.runner != null)
        {
            targetRunner = InGame.Instance.runner;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!isSetup) return;

        // Move the trap in +X direction
        currentSpeed += acceleration * Time.deltaTime;
        transform.position += Vector3.right * currentSpeed * Time.deltaTime;

        // 超過したら消滅させる
        if (Mathf.Abs(transform.position.x - startX) >= maxTravelDistance)
        {
            BrakeTheTrap();
            return;
        }

        // Calculate pull force affecting the Runner
        CurrentPullForce = Vector2.zero;
        if (targetRunner != null)
        {
            Vector3 diff = transform.position - targetRunner.transform.position;
            float distance = diff.magnitude;

            if (distance <= pullRadius && distance > 0.1f)
            {
                // Exponential pull: closer = stronger
                // Normalizing distance: 1 at center, 0 at edge
                float t = 1f - (distance / pullRadius);
                float pullStrength = Mathf.Pow(t, 3) * maxPullForce; // Exponential mapping
                
                CurrentPullForce = diff.normalized * pullStrength;
            }
            
            // Note:
            // プレイヤーへのダメージ判定は、
            // このプレハブに付けられた CircleCollider2D (IsTrigger=true, Radius=1f等) と
            // Runner側の CheckTrap(Trap trap) によって自動的に処理されます。
        }
    }
}
