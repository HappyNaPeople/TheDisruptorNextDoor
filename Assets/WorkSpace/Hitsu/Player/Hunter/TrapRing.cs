using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[System.Serializable]
/// <summary>
/// ・ラジアルUI上の1つの罠スロットを管理するクラス
/// ・罠の種類・必要コスト・表示角度・UI色を保持し、
/// ・現在のコスト状況に応じて使用可能 / 使用不可の表示を切り替える
/// </summary>
class RingTrap
{
    /// <summary> 罠スロットの使用可能状態 </summary>
    public enum Stage { On, Off }
    /// <summary> 現在のコスト状況から判定した使用可能状態 </summary>
    public Stage stage
    {
        get
        {
            // 必要コストを満たし、罠が設定されている場合は使用可能
            if (cost <= HunterConTrollerPad.Instance.nowCostCanUse && trap != TrapName.None) return Stage.On;
            else return Stage.Off;
        }
    }
    /// <summary> このスロットに設定されている罠 </summary>
    public TrapName trap;
    /// <summary> スロットの番号 </summary>
    public int code;
    /// <summary> この罠の必要コスト </summary>
    private int cost => GameManager.allTrap[trap].cost;

    /// <summary> 罠アイコンUI </summary>
    public RingPartUi ringPartUi;
    /// <summary> リング部分のマテリアル </summary>
    public Material m_ringPart;
    /// <summary> 内側部分のマテリアル </summary>
    public Material m_insidePart;


    /// <summary> 
    /// 使用可能時の色設定（リング色, 内側色）
    ///private (string, string) canUseColor = ("#00FF00", "#FFFFFF");
    /// </summary>
    private (Color, Color) canUseColor;

    /// <summary> 
    /// 使用不可時の色設定（リング色, 内側色） 
    ///private (string, string) cantUseColor = ("#FF0000", "#686868");
    /// </summary>
    private (Color, Color) cantUseColor;


    /// <summary> 選択中表示用の色　(#2862FF) </summary>
    private Color choseColor;

    /// <summary>
    /// RingTrapを生成し、罠情報とUI表示情報を初期化する
    /// </summary>
    /// <param name="trapName">設定する罠名</param>
    /// <param name="targetCode">マテリアル色変更用番号</param>
    /// <param name="angle">ラジアルUI上の回転角度</param>
    /// <param name="targetTrapUi">罠アイコンUI</param>
    /// <param name="ring">リング部分マテリアル</param>
    /// <param name="inside">内側部分マテリアル</param>
    public RingTrap(TrapName trapName, 
        int targetCode ,
        RingPartUi targetTrapUi,
        Material ring ,
        Material inside)
    {
        trap = trapName;                        // 罠情報設定
        code = targetCode;                      // 色変更用番号設定
        m_ringPart = ring;                      // マテリアル設定
        m_insidePart = inside;
        ringPartUi = targetTrapUi;              // UI参照設定

        ringPartUi.Init(trap);                  // 罠アイコンUI初期化
        Init();                                 // 色設定初期化

    }

    /// <summary> UI表示に使用する色を初期化する </summary>
    private void Init()
    {
        ColorUtility.TryParseHtmlString("#00FF00", out canUseColor.Item1);      // 使用可能時のリング色
        ColorUtility.TryParseHtmlString("#FFFFFF", out canUseColor.Item2);      // 使用可能時の内側色
        ColorUtility.TryParseHtmlString("#FF0000", out cantUseColor.Item1);     // 使用不可時のリング色
        ColorUtility.TryParseHtmlString("#686868", out cantUseColor.Item2);     // 使用不可時の内側色
        ColorUtility.TryParseHtmlString("#2862FF", out choseColor);             // 選択中の内側色
    }


    /// <summary> 現在の使用可能状態に応じてUI色を更新する </summary>
    public void UpdateUI()
    {
        (Color, Color) useColor;           
        useColor = stage == Stage.On ? canUseColor : cantUseColor;  // 使用可能 / 使用不可に応じた色を選択
        m_ringPart.SetColor($"_color0{code}", useColor.Item1);      // リング部分の色を更新
        m_insidePart.SetColor($"_color0{code}", useColor.Item2);    // 内側部分の色を更新
    }
    /// <summary> このスロットを選択中の色へ変更する </summary>
    public void SetChoseColor()
    {
        // リング部分は使用可能状態に応じた色を維持
        m_ringPart.SetColor($"_color0{code}", (stage == Stage.On ? canUseColor.Item1 : cantUseColor.Item1));
        // 内側部分のみ選択中色へ変更
        m_insidePart.SetColor($"_color0{code}", choseColor);
    }
}

/// <summary>
/// ・ハンターが使用する罠選択用ラジアルUIを管理するクラス
/// ・スティック入力による罠選択、選択中表示、
/// ・使用可能状態に応じたUI更新を行う
/// </summary>
public class TrapRing : MonoBehaviour
{
    [Header("Ring")]
    /// <summary> リング本体オブジェクト </summary>
    public GameObject ring;
    /// <summary> リング部分のマテリアル </summary>
    private Material m_ring;

    [Header("InsideCircle")]
    /// <summary> 内側円オブジェクト </summary>
    public GameObject insideCircle;
    /// <summary> 内側円部分のマテリアル </summary>
    private Material m_insideCircle;

    [Header("Trap Button")]
    /// <summary> 罠アイコンUI一覧 </summary>
    public RingPartUi[] ringPartUis;
    /// <summary> ラジアルUI上の罠スロット一覧 </summary>
    private List<RingTrap> ringTraps = new List<RingTrap>();
    /// <summary> 現在選択中の罠名 </summary>
    public TrapName chooseTrapName
    {
        get
        {
            // 選択番号が範囲外の場合はNoneを返す
            if (choseTrap<0|| choseTrap> ringTraps.Count)return TrapName.None;

            return ringTraps[choseTrap].trap;
        }
    }
    /// <summary> スティック表示用オブジェクト </summary>
    public GameObject joyStick;
    /// <summary> スティック入力中かどうか </summary>
    private bool isInputActive = false;
    /// <summary> 最後に入力された方向番号 </summary>
    private int lastDir = -1;
    /// <summary> 現在選択中の罠番号 </summary>
    public int choseTrap = -1;

    /// <summary>
    /// ・スティック入力から罠選択を行う
    /// ・入力方向を8方向に変換し、スティックを離した時に選択を確定する
    /// </summary>
    /// <param name="input">スティック入力値</param>
    public void ControllerChoose(Vector2 input)
    {
        // スティックを離した時に選択を確定
        if (input.magnitude < 0.1f && isInputActive)
        {
            // 前回と違う方向かつ有効範囲内の場合
            if (choseTrap != lastDir && lastDir >= 0 && lastDir < ringTraps.Count)
            {
                choseTrap = lastDir;                                                // 選択中番号を更新
                UIUpdate();                                                         // UI更新
                HunterConTrollerPad.Instance.CreateTrap(ringTraps[choseTrap].trap); // 選択した罠の設置処理を開始
            }
            else
            {
                UIUpdate(-1);                                                       // 無効選択時は選択表示を解除
            }

            isInputActive = false;                                                  // 入力状態をリセット
            lastDir = -1;
            joyStick.transform.localPosition = Vector3.back;                        // スティック表示位置を初期位置へ戻す

            return;
        }

        // スティックが十分倒された場合
        if (input.magnitude > 0.9f)
        {
            isInputActive = true;

            Vector2 inputLocal = Quaternion.Inverse(transform.rotation) * input;    // リングの回転を考慮してローカル入力へ変換
            float angle = Mathf.Atan2(inputLocal.x, inputLocal.y) * Mathf.Rad2Deg;  // 入力方向の角度を取得
            if (angle < 0) angle += 360;                                            // 角度を0～360度に補正
            int targetNumber = Mathf.RoundToInt(angle / 45f) % 8;                   // 角度を8方向の番号に変換
            joyStick.transform.localPosition = new Vector3(                         // スティック表示位置を更新
                Mathf.Sin(targetNumber * 45f * Mathf.Deg2Rad) * 5,
                Mathf.Cos(targetNumber * 45f * Mathf.Deg2Rad) * 5, -1);

            // 選択方向が変わった時のみUI更新
            if (targetNumber != lastDir)                    
            {
                lastDir = targetNumber;
                UIUpdate(lastDir);
            }
        }
    }

    /// <summary>
    /// ラジアルUIを初期化する
    /// </summary>
    /// <param name="choseTraps">使用可能な罠一覧</param>
    public void Init(List<TrapName> choseTraps)
    {
        m_ring = ring.GetComponent<Renderer>().material;                    // リング部分のマテリアル取得
        m_insideCircle = insideCircle.GetComponent<Renderer>().material;    // 内側円部分のマテリアル取得

        // マテリアル参照チェック
        if (m_ring == null || m_insideCircle == null)                       
        {
            Debug.LogError("Ring references are NULL");
            return;
        }

        UISetUp(choseTraps);                                                // UI初期設定
    }

    /// <summary>
    /// 使用可能な罠一覧をもとに罠スロットを生成し、UIを初期化する
    /// </summary>
    /// <param name="choseTraps">使用可能な罠一覧</param>
    private void UISetUp(List<TrapName> choseTraps)
    {
        // 罠数がUI数を超えている場合
        if (choseTraps.Count > ringPartUis.Length)
        {
            Debug.LogError("Trap count and SpriteRenderer count do not match");
            return;
        }
        ringTraps.Clear();                                              // 既存スロットをクリア

        for (int i = 0; i < ringPartUis.Length; i++)
        {
            RingTrap ringTrap;

            if (i < choseTraps.Count)                                   // 使用可能な罠がある場合               
            {
                ringTrap = new RingTrap(choseTraps[i], i, ringPartUis[i], m_ring, m_insideCircle);
            }
            else　　　　　　　　　　　　　　　　　　　　　　　　　　　　// 罠がないスロットはNoneとして登録
            {
                ringTrap = new RingTrap(TrapName.None, i, ringPartUis[i], m_ring, m_insideCircle);
            }

            ringTraps.Add(ringTrap);                                    // スロット一覧に追加
        }

        foreach (var trap in ringTraps)                                 // 全スロットのUIを更新
        {
            trap.UpdateUI();                                    
        }
    }

    /// <summary>
    /// 現在選択中の罠番号に合わせてUIを更新する
    /// </summary>
    public void UIUpdate()
    {
        for (int i = 0; i < ringTraps.Count; i++)
        {
            if (i == choseTrap)
            {
                ringTraps[choseTrap].SetChoseColor();
            }
            else
            {
                ringTraps[i].UpdateUI();
            }
        }
    }

    /// <summary>
    /// 現在選択中の罠番号に合わせてUIを更新する
    /// </summary>
    private void UIUpdate(int targetNumber)
    {
        for (int i = 0; i < ringTraps.Count; i++)
        {
            if (i == targetNumber)  // 選択中表示
            {
                ringTraps[targetNumber].SetChoseColor();    
            }
            else　　　　　　　　　　// 通常表示
            {
                ringTraps[i].UpdateUI();
            }
        }

    }

}
