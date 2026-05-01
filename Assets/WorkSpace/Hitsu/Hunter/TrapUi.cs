using UnityEngine;

public class TrapUi : MonoBehaviour
{
    public SpriteRenderer trapIcon;
    public SpriteRenderer trapCost_Number;

    public void Init(TrapName trapName)
    {
        if(trapName == TrapName.None)
        {
            gameObject.SetActive(false);
            return;
        }
        trapIcon.sprite = GameManager.allTrap[trapName].icon;
        trapCost_Number.sprite = GameManager.Instance.numberSprites[GameManager.allTrap[trapName].cost];
    }

}
