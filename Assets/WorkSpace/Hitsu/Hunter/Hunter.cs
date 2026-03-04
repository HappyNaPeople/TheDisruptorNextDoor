using System.Collections.Generic;
using UnityEngine;

public class Backpack
{
    public const int maxCost = 30;
    public int nowCost = 0;

    public List<Trap> trapsPack;
    public void AddToBackpack(Trap target)
    {
        if ((nowCost + target.cost) > maxCost) return;
        else trapsPack.Add(target);
    }

}

public class Hunter : MonoBehaviour
{
    public Backpack backpack = new Backpack();




    //public bool choseTrap = false;

    //public enum NowStage
    //{
    //    None,
    //    Building,
    //    Waiting,
    //}
    //public NowStage nowStage = NowStage.None;
    //public void ChooseTrap()
    //{

    //}

    //public void Building()
    //{

    //}

    //public void Waiting()
    //{

    //} 

}
