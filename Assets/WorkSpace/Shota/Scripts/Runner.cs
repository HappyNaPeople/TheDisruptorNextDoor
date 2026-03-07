using Mono.Cecil;
using Unity.VisualScripting;
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

    // private
    InputDevice _inputDevice;
    CharacterController2D _controller;
    Animator _animator;
    Vector3 _velocity = new Vector3();


    public void SwitchController()
    {
        ControllerCode = (ControllerCode + 1) % 2;
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
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
            default:
                Death();
                break;
        }
    }

    void OnControllerTriggerExit(Collider2D col)
    {

    }

    void Death()
    {
        Debug.Log("Runner Dead");
        _animator.SetTrigger("Hurt");
    }

    #region move
    public void Move()
    {
        float dt = Time.deltaTime;

        if (_controller.isGrounded)
            _velocity.y = 0f;

        // 重力加算
        _velocity.y += gravity * dt;

        if (canMove)
        {
            InputMove();
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

        var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
        _velocity.x = Mathf.Lerp(_velocity.x, horiInput * runSpeed, Time.deltaTime * smoothedMovementFactor);


        // we can only jump whilst grounded
        if (_controller.isGrounded && GetJumpInput())
        {
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
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
