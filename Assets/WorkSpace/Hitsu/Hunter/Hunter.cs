using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hunter が使用するバックパック。
/// Trap を保存し、使用できる Trap のコストを管理する。
/// </summary>
public class Backpack
{
    // バックパックの最大コスト
    public const int maxCost = 30;
    // 現在使用しているコスト
    public int nowCost = 0;
    // 所持している Trap 一覧
    public List<TrapName> trapsPack = new List<TrapName>();
    /// <summary>
    /// Trap をバックパックに追加する
    /// 最大コストを超える場合は追加しない
    /// </summary>
    public void AddToBackpack(TrapName targetTrapName)
    {
        if ((nowCost + CanUseTrap.allTrap[targetTrapName].cost) > maxCost) return;
        trapsPack.Add(targetTrapName);
        nowCost += CanUseTrap.allTrap[targetTrapName].cost;
    }

}

/// <summary>
/// Hunter プレイヤーのクラス。
/// Trap を管理する Backpack を持つ。
/// </summary>
public class Hunter
{

    // Hunter が所持する Trap バッグ
    public Backpack backpack = new Backpack();
    
    public void Hunter_Init()
    {

    }


}
