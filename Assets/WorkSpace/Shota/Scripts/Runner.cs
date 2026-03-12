using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class Runner : MonoBehaviour
{
    [Min(0)] public int ControllerCode = 0;

    [Header("プレイヤーのステータス")]
    public float runSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.8f;
    public float groundDamping = 20f;
    public float inAirDamping = 5f;

    [Header("プレイヤーの状態")]
    public bool canMove { get; private set; } = true;
    public bool isInKnockback { get; private set; } = false;
    Vector2 _knockbackVector = Vector2.zero;
    public bool isJumping { get; private set; } = false;
    Vector2 _jumpVector = Vector2.zero;


    // private
    InputDevice _inputDevice;
    CharacterController2D _controller;
    Animator _animator;
    Vector2 _velocity = new Vector2();


    public void SetControllerCode(int code)
    {
        ControllerCode = code;
    }
    public void SwitchController()
    {
        ControllerCode = (ControllerCode + 1) % 2;
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
    }

    public IEnumerator SleepMove(float duration)
    {
        canMove = false;
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

    void Start()
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
        Move();
    }

    void OnControllerTriggerEnter(Collider2D col)
    {
        if (col.TryGetComponent<Trap>(out var trap))
        {
            CheckTrap(trap);
        }
    }

    void CheckTrap(Trap trap)
    {
        switch (trap.trapName)
        {
            case TrapName.JumpPad:
                if (trap.TryGetComponent<JumpPad>(out var jumpPad))
                {
                    ReserveJump(jumpPad.direction);
                }
                break;

            default:
                Death();
                break;
        }
    }

    void OnControllerTriggerExit(Collider2D col)
    {

    }

    // vector.x = 横に飛ばしたいマス目（距離） 例: 左に10マスなら -10f
    // vector.y = 浮かせたい高さ（必ず0より大きい値にする） 例: 3f
    void ReserveKnockback(Vector2 vector)
    {
        // 高さ(Y)が0以下だとゼロ除算エラーになるため防ぐ
        float height = Mathf.Max(0.1f, vector.y);

        // 1. Y方向の初速を計算
        float vy = Mathf.Sqrt(2f * height * -gravity);

        // 2. 空中にいる合計時間（頂点まで行く時間 vy / -gravity の2倍）
        float timeInAir = (vy / -gravity) * 2f;

        // 3. X方向の速度（距離 ÷ 時間）
        float vx = vector.x / timeInAir;

        Debug.Log($"Knockback:{vector}");
        // 求めた速度を代入
        _knockbackVector = new Vector2(vx, vy);
    }

    void ReserveJump(Vector2 vector)
    {
        // 高さ(Y)が0以下だとゼロ除算エラーになるため防ぐ
        float height = Mathf.Max(0.1f, vector.y);

        // 1. Y方向の初速を計算
        float vy = Mathf.Sqrt(2f * height * -gravity);

        // 2. 空中にいる合計時間（頂点まで行く時間 vy / -gravity の2倍）
        float timeInAir = (vy / -gravity) * 2f;

        // 3. X方向の速度（距離 ÷ 時間）
        float vx = vector.x / timeInAir;
        _jumpVector = new Vector2(vx, vy);
    }

    void Death()
    {
        Debug.Log("Runner Dead");
        _animator.SetTrigger("Hurt");
        StartCoroutine(SleepMove(0.25f));
    }

    #region move
    public void Move()
    {
        float dt = Time.deltaTime;

        if (_controller.isGrounded)
        {
            if (isInKnockback) isInKnockback = false;
            if (isJumping) isJumping = false;

            _velocity.y = 0f;
        }

        if (Vector3.Dot(_knockbackVector, _knockbackVector) != 0f)
        {
            _velocity = Vector2.zero;
            _velocity += _knockbackVector;

            isInKnockback = true;

            _animator.SetTrigger("Hit");
            _knockbackVector = Vector2.zero;
        }
        if (Vector3.Dot(_jumpVector, _jumpVector) != 0f)
        {
            _velocity = Vector2.zero;
            _velocity += _jumpVector;

            isJumping = true;

            _animator.SetTrigger("Jump");
            _jumpVector = Vector2.zero;
        }

        // 重力加算
        _velocity.y += gravity * dt;

        if (canMove)
        {
            InputMove();
        }
        else if (!isInKnockback)
        {
            var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping;
            _velocity.x = Mathf.Lerp(_velocity.x, 0, Time.deltaTime * smoothedMovementFactor);
        }

        // 位置更新
        _controller.move(_velocity * dt);
        _velocity = _controller.velocity;

        _animator.SetBool("IsGrounded", _controller.isGrounded);
    }

    void InputMove()
    {
        // 左右移動処理
        float horiInput = GetHorizontalInput();
        if (horiInput > 0f)
        {
            if (transform.localScale.x < 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            if (_controller.isGrounded)
                _animator.SetBool("IsRunning", true);
        }
        else if (horiInput < 0f)
        {
            if (transform.localScale.x > 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            if (_controller.isGrounded)
                _animator.SetBool("IsRunning", true);
        }
        else
        {
            _animator.SetBool("IsRunning", false);
        }


        if (!isInKnockback)
        {
            var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
            _velocity.x = Mathf.Lerp(_velocity.x, horiInput * runSpeed, Time.deltaTime * smoothedMovementFactor);
        }


        // we can only jump whilst grounded
        if (_controller.isGrounded && GetJumpInput() && !isInKnockback)
        {
            _velocity.y += Mathf.Sqrt(2f * jumpHeight * -gravity);
            _animator.SetTrigger("Jump");
        }
    }

    float GetHorizontalInput()
    {
        if (_inputDevice == null) return 0f;

        float horizontalInput = 0f;
        if (_inputDevice.gamepad != null && _inputDevice.gamepad.Count > ControllerCode)
        {
            horizontalInput = _inputDevice.gamepad[ControllerCode].leftStick.x.ReadValue();
        }


#if UNITY_EDITOR
        if (_inputDevice.keyboard != null)
        {
            float aKey = _inputDevice.keyboard.aKey.ReadValue();
            float dKey = _inputDevice.keyboard.dKey.ReadValue();

            float kbInput = dKey - aKey;
            if (Mathf.Abs(kbInput) > Mathf.Abs(horizontalInput)) horizontalInput = kbInput;
        }
#endif

        return horizontalInput;
    }

    bool GetJumpInput()
    {
        if (_inputDevice == null) return false;

        if (_inputDevice.gamepad != null && _inputDevice.gamepad.Count > ControllerCode)
        {
            if (_inputDevice.gamepad[ControllerCode].buttonSouth.wasPressedThisFrame)
                return true;
        }

#if UNITY_EDITOR
        if (_inputDevice.keyboard != null)
        {
            if (_inputDevice.keyboard.spaceKey.wasPressedThisFrame)
                return true;
            if (_inputDevice.keyboard.wKey.wasPressedThisFrame)
                return true;
        }
#endif

        return false;
    }

    #endregion
}
