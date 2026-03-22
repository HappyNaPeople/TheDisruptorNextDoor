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


    public enum PlayerState
    {
        Dead = 0,
        Normal = 1,     // 通常（移動・ジャンプ可能）
        Jumping = 2,    // ジャンプ中（移動可能）
        Attacking = 3,  // 攻撃中（操作不能）
        Knockback = 4,  // ノックバック中（操作不能）
    }
    [Header("プレイヤーの状態")]
    public PlayerState currentState = PlayerState.Normal;


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

        _controller.onTriggerEnterEvent += OnControllerTriggerEnter;
        _controller.onTriggerExitEvent += OnControllerTriggerExit;
    }

    void Update()
    {
        UpdateMove();
    }

    void OnControllerTriggerEnter(Collider2D col)
    {
        if (col.TryGetComponent<Trap>(out var trap))
        {
            CheckTrap(trap);
        }
    }
    void OnControllerTriggerExit(Collider2D col)
    {

    }

    void CheckTrap(Trap trap)
    {
        switch (trap.trapName)
        {
            case TrapName.JumpPad:
                if (trap.TryGetComponent<JumpPad>(out var jumpPad))
                {
                    ExecuteJump(jumpPad.direction);
                }
                break;

            default:
                Death();
                break;
        }
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
                trapHp.TakeDamage(1, hit.ClosestPoint(transform.position));
            }
        }
    }

    public void Death()
    {
        _animator.SetTrigger("Hurt");
        if (isInvincible) return;

        ChangeState(PlayerState.Dead);
        _velocity = Vector2.zero;
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
    }

    #region move

    public void UpdateMove()
    {
        if (inputData == null)
        {
            return;
        }
        if (currentState == PlayerState.Dead) return;

        float dt = Time.deltaTime;

        // 1. 接地状態の更新とステート遷移
        HandleGroundedState();

        // 2. 現在のステートに基づいて X 速度を決定する
        CheckMoveX(dt);
        CheckJump();
        CheckPunch();

        // 3. 重力を適用
        _velocity.y += gravity * dt;

        // 4. 最終的な移動実行（ここだけが「動かす」処理）
        _controller.move(_velocity * dt);
        _velocity = _controller.velocity;

        // 5. アニメーター更新
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
        }
    }

    void CheckJump()
    {
        if (_isPhysicsReserved) return;
        if (!inputData.isJumpPressed) return;
        // ジャンプ処理
        if (_controller.isGrounded)
        {
            _velocity += CalculateInitialVelocity(new Vector2(0f, jumpHeight));
            _animator.SetTrigger("Jump");

            ChangeState(PlayerState.Jumping);
        }
    }

    void CheckPunch()
    {
        if (_isPhysicsReserved) return;
        if (!inputData.isPunchPressed) return;
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
        float horizontalInput = inputData.moveInput.x;

        // 加減速を滑らかにする（Lerpを使用）
        float targetSpeed = horizontalInput * runSpeed;
        float acceleration = _controller.isGrounded ? groundDamping : inAirDamping;

        _velocity.x = Mathf.Lerp(_velocity.x, targetSpeed, Time.deltaTime * acceleration);

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
