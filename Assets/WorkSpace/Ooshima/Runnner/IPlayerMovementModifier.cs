using UnityEngine;

public interface IPlayerMovementModifier
{
    float ModifyRunSpeed(float baseSpeed) => baseSpeed;
    float ModifyDamping(float baseDamping) => baseDamping;
    float ModifyJumpHeight(float baseJumpHeight) => baseJumpHeight;
    float ModifyGravity(float baseGravity) => baseGravity;

    void ModifyVelocity(ref Vector2 expectedVelocity, float deltaTime) { }
}
