using Mono.Cecil;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class RunnerController : MonoBehaviour
{
    [Min(0)]public int ControllerNumber = 0;

    [Header("プレイヤーのステータス")]
    public float runSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.8f;
    public float groundDamping = 20f;
    public float inAirDamping = 5f;


    // private
    InputDevice _inputDevice;
    CharacterController2D _controller;
    Animator _animator;
    Vector3 _velocity = new Vector3();

    void Start()
    {
        _controller = GetComponent<CharacterController2D>();
        _inputDevice = GameManager.inputDevice;
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
    }


    void Move()
    {
        float dt = Time.deltaTime;

        if (_controller.isGrounded)
            _velocity.y = 0f;

        // 重力加算
        _velocity.y += gravity * dt;


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


        // 位置更新
        _controller.move(_velocity * dt);
        _velocity = _controller.velocity;

        _animator.SetBool("IsGrounded", _controller.isGrounded);
    }

    float GetHorizontalInput()
    {
        if (_inputDevice == null) return 0f;

        float horizontalInput = 0f;
        if (_inputDevice.gamepad != null && _inputDevice.gamepad.Count > ControllerNumber)
        {
            horizontalInput = _inputDevice.gamepad[ControllerNumber].leftStick.x.ReadValue();
        }
        else
        {
            Debug.Log($"Controller_{ControllerNumber} is not founded");
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

        if (_inputDevice.gamepad != null && _inputDevice.gamepad.Count > ControllerNumber)
        {
            if (_inputDevice.gamepad[ControllerNumber].buttonSouth.wasPressedThisFrame)
                return true;
        }
        else
        {
            Debug.Log($"Controller_{ControllerNumber} is not founded");
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
}
