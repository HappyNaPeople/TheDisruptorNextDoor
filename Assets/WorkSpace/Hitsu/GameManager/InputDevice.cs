using UnityEngine.InputSystem;
using System.Collections.Generic;
public class InputDevice
{
    public Mouse mouse { get; private set; }
    public Keyboard keyboard { get; private set; }
    public List<Gamepad> gamepad { get; private set; }


    public void InputInit()
    {
        gamepad = new List<Gamepad>();
        mouse = InputSystem.GetDevice<Mouse>();
        keyboard = InputSystem.GetDevice<Keyboard>();

        foreach (var device in InputSystem.devices)
        {
            switch (device)
            {
                case Gamepad gamepadDevice:

                    gamepad.Add(gamepadDevice);

                    break;
            }
        }

    }
}
