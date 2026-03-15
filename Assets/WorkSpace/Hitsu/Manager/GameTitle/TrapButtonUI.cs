using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrapButtonUI : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TMP_Text cost;
    public TMP_Text information;

    public void SetTrap(TrapInformation trapInformation)
    {
        icon.sprite = trapInformation.icon;
        cost.text = $"{trapInformation.cost}";
        information.text = trapInformation.information;
    }
}
