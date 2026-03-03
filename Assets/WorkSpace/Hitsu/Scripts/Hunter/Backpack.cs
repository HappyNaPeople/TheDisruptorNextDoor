using System.Collections.Generic;

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
