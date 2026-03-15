using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrapUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text count;
    public void SetTrap(ChoseTrapData data)
    {
        icon.sprite = CanUseTrap.allTrap[data.trapName].icon;
        count.text = $"x{data.trapCount}";
    }


}
