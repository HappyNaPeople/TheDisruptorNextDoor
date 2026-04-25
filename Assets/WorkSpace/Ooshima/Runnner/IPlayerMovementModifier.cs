using UnityEngine;

public interface IPlayerMovementModifier
{
    float ModifyRunSpeed(float baseSpeed) => baseSpeed;
    float ModifyDamping(float baseDamping) => baseDamping;
    float ModifyJumpHeight(float baseJumpHeight) => baseJumpHeight;
    float ModifyGravity(float baseGravity) => baseGravity;

    void ModifyVelocity(ref Vector2 expectedVelocity, float deltaTime) { }

    // 入力を書き換えるためのフック (デフォルトは何もしない)
    void ModifyInput(ref Vector2 moveInput, ref bool isJumpPressed) { }
}
