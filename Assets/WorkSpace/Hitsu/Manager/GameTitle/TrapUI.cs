using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーが選択した Trap を表示する UI クラス。
///
/// 主な役割：
/// ・Trap のアイコン表示
/// ・選択された Trap の個数表示
///
/// TitleCanvas から呼び出され、
/// ChoseTrapData の情報を UI に反映する。
/// </summary>
public class TrapUI : MonoBehaviour
{
    /// <summary>
    /// Trap アイコン表示
    /// </summary>
    public Image icon;

    /// <summary>
    /// Trap 個数表示
    /// </summary>
    public TMP_Text count;


    /// <summary>
    /// Trap 情報を UI に反映する
    /// </summary>
    /// <param name="data">選択された Trap データ</param>
    //public void SetTrap(ChoseTrapData data)
    //{
    //    // Trap アイコン設定
    //    icon.sprite = GameManager.allTrap[data.trapName].icon;

    //    // Trap 数量表示
    //    //count.text = $"x{data.trapCount}";
    //    count.text = $"";

    //}
    public void SetTrap(TrapName trapName)
    {
        // Trap アイコン設定
        icon.sprite = GameManager.allTrap[trapName].icon;

        // Trap 数量表示
        //count.text = $"x{data.trapCount}";
        count.text = $"";

    }


}
