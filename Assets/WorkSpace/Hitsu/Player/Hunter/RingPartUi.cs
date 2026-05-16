using UnityEngine;

public class RingPartUi : MonoBehaviour
{
    public SpriteRenderer trapIcon;
    public SpriteRenderer trapCost_Number;

    public void Init(TrapName trapName)
    {
        gameObject.SetActive(true);

        if (trapIcon==null|| trapCost_Number == null)
        {
            Debug.LogError("RingPartUi___ SpriteRenderer == null");

        }


        if (trapName == TrapName.None)
        {
            gameObject.SetActive(false);
            return;
        }


        trapIcon.sprite = GameManager.allTrap[trapName].icon;
        trapCost_Number.sprite = GameManager.Instance.numberSprites[GameManager.allTrap[trapName].cost];

        bool correctIcon = trapIcon.sprite == GameManager.allTrap[trapName].icon;
        bool correctCost = trapCost_Number.sprite == GameManager.Instance.numberSprites[GameManager.allTrap[trapName].cost];

        if (!correctIcon || !correctCost)
        {
            Debug.LogError("Ui Set Up Error");
        }

    }



}
