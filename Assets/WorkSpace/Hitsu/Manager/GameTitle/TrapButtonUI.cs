using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Trap 選択ボタ? UI を管?するク?ス。
///
/// 主な役?：
/// ・Trap のアイコ?表示
/// ・Trap のコスト表示
/// ・Trap の説明文表示
/// ・ボタ?コ?ポーネ?トの保?
///
/// TitleCanvas から呼び出され、
/// TrapInformation のデータを UI に反映する。
/// </summary>
public class TrapButtonUI : MonoBehaviour
{
    /// <summary>
    /// Trap を選択するボタ?
    /// </summary>
    public Button button;

    /// <summary>
    /// Trap アイコ?画?
    /// </summary>
    public Image icon;

    /// <summary>
    /// Trap コスト表示
    /// </summary>
    public TMP_Text cost;

    /// <summary>
    /// Trap の説明文
    /// </summary>
    public TMP_Text information;

    /// <summary>
    /// Trap 情報を UI に反映する
    /// </summary>
    /// <param name="trapInformation">表示する Trap の情報</param>
    public void SetTrap(TrapInformation trapInformation)
    {
        // Trap アイコ?設定
        icon.sprite = trapInformation.icon;
        // Trap コスト表示
        cost.text = $"{trapInformation.cost}";
        // Trap 説明文表示
        information.text = trapInformation.information;
    }
}
