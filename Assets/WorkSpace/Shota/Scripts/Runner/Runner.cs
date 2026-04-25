using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(CharacterController2D))]
public class Runner : MonoBehaviour
{
    [Header("デバッグ用")]
    public PlayerInputData inputData;
    public bool isInvincible = false;
    public bool isVisibleGizmos = false;


    [Header("プレイヤーのステータス")]
    public float runSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.8f;
    public float groundDamping = 20f;
    public float inAirDamping = 5f;
    public Vector2 attackBoxOffset = Vector2.zero;
    public Vector2 attackBoxSize = Vector2.zero;
    
    [Tooltip("パンチの威力（罠の寿命を何秒削るか）")]
    public float punchDamage = 5f;


    public enum PlayerState
    {
        Dead = 0,
        Normal = 1,     // 通常（移動・ジャンプ可能）
        Jumping = 2,    // ジャンプ中（移動可能）
        Attacking = 3,  // 攻撃中（操作不能）
        Knockback = 4,  // ノックバック中（操作不能）
        Grabbed = 5,    // 捕獲・拘束中（操作不能、干渉により移動）
    }
    [Header("プレイヤーの状態")]
    public PlayerState currentState = PlayerState.Normal;

    public bool IsGrounded => _controller != null && _controller.isGrounded;

    // --- モディファイア（干渉物）リスト ---
    public System.Collections.Generic.List<IPlayerMovementModifier> activeModifiers = new System.Collections.Generic.List<IPlayerMovementModifier>();

    // --- 加工済み入力データ (モディファイアによる変更を適用したもの) ---
    public Vector2 CurrentMoveInput { get; private set; }
    public bool CurrentJumpPressed { get; private set; }

    public void AddModifier(IPlayerMovementModifier modifier)
    {
        if (!activeModifiers.Contains(modifier))
        {
            activeModifiers.Add(modifier);
        }
    }

    public void RemoveModifier(IPlayerMovementModifier modifier)
    {
        if (activeModifiers.Contains(modifier))
        {
            activeModifiers.Remove(modifier);
        }
    }


    [Header("リスポーン設定")]
    public Transform respawnPoint = null;


    // private
    InputDevice _inputDevice;
    CharacterController2D _controller;
    Animator _animator;
    Vector2 _velocity = new Vector2();
    bool _isPhysicsReserved = false;

    public void SetPlayerInputData(PlayerInputData data)
    {
        inputData = data;
    }

    public void ChangeState(PlayerState state)
    {
        currentState = state;
    }

    private void Start()
    {
        RunnerInit();
    }

    public void RunnerInit()
    {
        _controller = GetComponent<CharacterController2D>();
        _inputDevice = GameManager.inputDevice;
        _animator = GetComponent<Animator>();
        
        activeModifiers.Clear();

        _controller.onTriggerEnterEvent += OnControllerTriggerEnter;
        _controller.onTriggerExitEvent += OnControllerTriggerExit;
    }

    void Update()
    {
        UpdateMove();
    }

    void OnControllerTriggerEnter(Collider2D col)
    {
        // 罠などの判定は罠側かモディファイアで行うため、Runner側では感知不要
    }
    void OnControllerTriggerExit(Collider2D col)
    {

    }

    public void ExecuteJump(Vector2 vector)
    {
        _velocity = CalculateInitialVelocity(vector);
        _animator.SetTrigger("Jump");
        ChangeState(PlayerState.Jumping);

        _isPhysicsReserved = true;
    }

    public void ExecuteKnockback(Vector2 vector)
    {
        _velocity = CalculateInitialVelocity(vector);
        _animator.SetTrigger("Hit");

        ChangeState(PlayerState.Knockback);

        _isPhysicsReserved = true;
    }

    public void ExecutePunch()
    {
        var hitTraps = Physics2D.OverlapBoxAll(transform.position + GetAttackOffset(), attackBoxSize, 0f, 1 << UseLayerName.trapLayer);

        foreach (var hit in hitTraps)
        {
            if (hit.TryGetComponent<TrapHp>(out var trapHp))
            {
                trapHp.TakeDamage(punchDamage, hit.ClosestPoint(transform.position));
            }
        }
    }

    public void Death()
    {
        _animator.SetTrigger("Hurt");
        if (isInvincible) return;

        ChangeState(PlayerState.Dead);
        _velocity = Vector2.zero;
        activeModifiers.Clear();
    }

    public void Respawn()
    {
        if (isInvincible) return;

        ChangeState(PlayerState.Normal);
        _animator.SetTrigger("Respawn");
        if (respawnPoint != null) transform.position = respawnPoint.position;
        else transform.position = Vector2.zero;

        var scale = transform.localScale;
        transform.localScale = new Vector2(Mathf.Abs(scale.x), scale.y);
        activeModifiers.Clear();
    }

    #region move

    public void UpdateMove()
    {
        if (inputData == null)
        {
            return;
        }
        if (currentState == PlayerState.Dead) return;

        // --- 入力の取得とモディファイアの適用 ---
        CurrentMoveInput = inputData.moveInput;
        CurrentJumpPressed = inputData.isJumpPressed;

        foreach (var mod in activeModifiers)
        {
            Vector2 mMove = CurrentMoveInput;
            bool mJump = CurrentJumpPressed;
            mod.ModifyInput(ref mMove, ref mJump);
            CurrentMoveInput = mMove;
            CurrentJumpPressed = mJump;
        }

        float dt = Time.deltaTime;

        // 1. 接地状態の更新とステート遷移
        HandleGroundedState();

        // 2. 現在のステートに基づいて X 速度を決定する
        CheckMoveX(dt);
        CheckJump();
        CheckPunch();

        // 3. 重力を適用
        float currentGravity = gravity;
        foreach (var mod in activeModifiers)
        {
            currentGravity = mod.ModifyGravity(currentGravity);
        }
        _velocity.y += currentGravity * dt;

        // 4. モディファイアによる最終速度の上書き・ベクトル干渉（引き寄せやくっつき等）
        foreach (var mod in activeModifiers)
        {
            mod.ModifyVelocity(ref _velocity, dt);
        }

        // 5. 最終的な移動実行（ここだけが「動かす」処理）
        _controller.move(_velocity * dt);
        _velocity = _controller.velocity;

        // 6. アニメーター更新
        _animator.SetBool("IsGrounded", _controller.isGrounded);
        _isPhysicsReserved = false;
    }

    // 接地時のリセット処理
    void HandleGroundedState()
    {
        if (_isPhysicsReserved) return;
        if (_controller.isGrounded)
        {
            // ノックバック中なら通常状態に戻す
            if (currentState == PlayerState.Knockback || currentState == PlayerState.Jumping)
            {
                currentState = PlayerState.Normal;
                return;
            }

            _velocity.y = 0f;
        }
    }

    // ステートごとに X 速度をどう決めるか
    void CheckMoveX(float dt)
    {
        switch (currentState)
        {
            case PlayerState.Normal:
            case PlayerState.Jumping:
                // Move()の中で水平入力を _velocity.x に代入する形にする
                ApplyInputMovement();
                break;

            case PlayerState.Knockback:
                // 操作不能だが、空中での慣性などは適用しても良い
                ApplyAirDamping(dt);
                break;

            case PlayerState.Attacking:
                // 攻撃中は急に止まるか、少しだけ滑らせるか
                ApplyFriction(dt);
                break;
                
            case PlayerState.Grabbed:
                // 捕獲中（自力移動不可・モディファイアが勝手に velocity を書き換える）
                break;
        }
    }

    void CheckJump()
    {
        if (_isPhysicsReserved) return;
        if (currentState == PlayerState.Grabbed) return; // 捕獲中はジャンプ不可
        if (!CurrentJumpPressed) return;
        // ジャンプ処理
        if (_controller.isGrounded)
        {
            float currentJumpHeight = jumpHeight;
            foreach (var mod in activeModifiers)
            {
                currentJumpHeight = mod.ModifyJumpHeight(currentJumpHeight);
            }

            _velocity += CalculateInitialVelocity(new Vector2(0f, currentJumpHeight));
            _animator.SetTrigger("Jump");

            ChangeState(PlayerState.Jumping);
        }
    }

    void CheckPunch()
    {
        if (_isPhysicsReserved) return;
        if (currentState == PlayerState.Grabbed) return; // 捕獲中はパンチ不可
        if (!inputData.isPunchPressed) return;
        if (currentState == PlayerState.Attacking) return;

        // パンチ処理
        if (_controller.isGrounded)
        {
            _animator.SetTrigger("Punch");

            ChangeState(PlayerState.Attacking);
        }
    }

    void ApplyInputMovement()
    {
        // 入力値（-1, 0, 1）を取得
        float horizontalInput = CurrentMoveInput.x;

        float currentSpeed = runSpeed;
        float currentDamping = _controller.isGrounded ? groundDamping : inAirDamping;

        foreach (var mod in activeModifiers)
        {
            currentSpeed = mod.ModifyRunSpeed(currentSpeed);
            currentDamping = mod.ModifyDamping(currentDamping);
        }

        // 加減速を滑らかにする（Lerpを使用）
        float targetSpeed = horizontalInput * currentSpeed;

        _velocity.x = Mathf.Lerp(_velocity.x, targetSpeed, Time.deltaTime * currentDamping);

        if (horizontalInput == 0f)
        {
            _animator.SetBool("IsRunning", false);
        }
        else if (horizontalInput > 0f)
        {
            Vector2 scale = transform.localScale;
            transform.localScale = new Vector2(Mathf.Abs(scale.x), scale.y);
            _animator.SetBool("IsRunning", true);
        }
        else
        {
            Vector2 scale = transform.localScale;
            transform.localScale = new Vector2(-Mathf.Abs(scale.x), scale.y);
            _animator.SetBool("IsRunning", true);
        }
    }

    void ApplyFriction(float dt)
    {
        // 攻撃中などは急激に速度を0に近づける
        // 摩擦係数を大きく設定（例: 20f）
        _velocity.x = Mathf.Lerp(_velocity.x, 0, dt * groundDamping * 2f);
    }

    void ApplyAirDamping(float dt)
    {
        // 操作不能なのでtargetSpeedは0（自然に止まるのを待つ）
        // ただし、ApplyInputMovementよりは緩やかに減速させる
        _velocity.x = Mathf.Lerp(_velocity.x, 0, dt * inAirDamping);
    }

    // 目的地（ベクトル）から必要な初速を計算する共通関数
    Vector2 CalculateInitialVelocity(Vector2 vector)
    {
        float height = Mathf.Max(0.1f, vector.y);
        float vy = Mathf.Sqrt(2f * height * -gravity);
        float timeInAir = (vy / -gravity) * 2f;
        float vx = vector.x / timeInAir;
        return new Vector2(vx, vy);
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (!isVisibleGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + GetAttackOffset(), attackBoxSize);
    }

    Vector3 GetAttackOffset()
    {
        var offset = new Vector3(attackBoxOffset.x * Mathf.Sign(transform.localScale.x), attackBoxOffset.y);
        return offset;
    }

    
}
