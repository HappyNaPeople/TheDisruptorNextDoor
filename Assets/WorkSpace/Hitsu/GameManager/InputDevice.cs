using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// 入力デバイスを管理するクラス。
/// 
/// 主な役割：
/// ・Mouse / Keyboard / Gamepad の取得
/// ・接続されている Gamepad を List で管理
/// 
/// InputSystem から現在接続されているデバイスを取得し、
/// Gamepad は複数接続に対応するため List に保存する。
/// </summary>
public class InputDevice
{
    // マウス
    public Mouse mouse { get; private set; }
    // キーボード
    public Keyboard keyboard { get; private set; }
    // 接続されている Gamepad 一覧
    public List<Gamepad> gamepad { get; private set; }


    /// <summary>
    /// 入力デバイスを初期化する
    /// InputSystem から Mouse / Keyboard / Gamepad を取得する
    /// </summary> 
    public void InputInit()
    {
        // マウスとキーボード取得
        mouse = InputSystem.GetDevice<Mouse>();
        keyboard = InputSystem.GetDevice<Keyboard>();

        // 接続されている Gamepad を検索
        gamepad = new List<Gamepad>();
        foreach (var device in InputSystem.devices)
        {
            switch (device)
            {
                case Gamepad gamepadDevice:

                    // Gamepad をリストに追加
                    gamepad.Add(gamepadDevice);
                    break;
            }
        }

    }
}
