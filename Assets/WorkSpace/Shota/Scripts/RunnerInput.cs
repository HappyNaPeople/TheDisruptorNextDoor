using UnityEngine;

public static class RunnerInput
{
    public static float GetHorizontalInput(InputDevice inputDevice, int controllerCode)
    {
        if (inputDevice == null) return 0f;

        float horizontalInput = 0f;
        if (inputDevice.gamepad != null && inputDevice.gamepad.Count > controllerCode)
        {
            horizontalInput = inputDevice.gamepad[controllerCode].leftStick.x.ReadValue();
        }


#if UNITY_EDITOR
        if (inputDevice.keyboard != null)
        {
            float aKey = inputDevice.keyboard.aKey.ReadValue();
            float dKey = inputDevice.keyboard.dKey.ReadValue();

            float kbInput = dKey - aKey;
            if (Mathf.Abs(kbInput) > Mathf.Abs(horizontalInput)) horizontalInput = kbInput;
        }
#endif

        return horizontalInput;
    }

    public static bool GetJumpInput(InputDevice inputDevice, int controllerCode)
    {
        if (inputDevice == null) return false;

        if (inputDevice.gamepad != null && inputDevice.gamepad.Count > controllerCode)
        {
            if (inputDevice.gamepad[controllerCode].buttonSouth.wasPressedThisFrame)
                return true;
        }

#if UNITY_EDITOR
        if (inputDevice.keyboard != null)
        {
            if (inputDevice.keyboard.spaceKey.wasPressedThisFrame)
                return true;
            if (inputDevice.keyboard.wKey.wasPressedThisFrame)
                return true;
        }
#endif

        return false;
    }

    public static bool GetPunchInput(InputDevice inputDevice, int controllerCode)
    {
        if (inputDevice == null) return false;

        if (inputDevice.gamepad != null && inputDevice.gamepad.Count > controllerCode)
        {
            if (inputDevice.gamepad[controllerCode].buttonEast.wasPressedThisFrame)
                return true;
        }

#if UNITY_EDITOR
        if (inputDevice.keyboard != null)
        {
            if (inputDevice.keyboard.fKey.wasPressedThisFrame)
                return true;
            if (inputDevice.keyboard.wKey.wasPressedThisFrame)
                return true;
        }
#endif

        return false;
    }
}
