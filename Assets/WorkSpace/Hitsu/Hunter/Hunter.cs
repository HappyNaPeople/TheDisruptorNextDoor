using System.Collections.Generic;
using UnityEngine;


public class TrapCanUse
{
    public TrapName trap {  get; private set; }
    public int trapCount { get; private set; }

    public TrapCanUse(TrapName trapName, int trapCount)
    {
        this.trap = trapName;
        this.trapCount = trapCount;
    }
}

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
    public List<TrapCanUse> trapsPack = new List<TrapCanUse>();
    /// <summary>
    /// Trap をバックパックに追加する
    /// 最大コストを超える場合は追加しない
    /// </summary>
    public void AddToBackpack(TrapName targetTrapName, int count) => trapsPack.Add(new TrapCanUse(targetTrapName, count));

}

/// <summary>
/// Hunter プレイヤーのクラス。
/// Trap を管理する Backpack を持つ。
/// </summary>
public class Hunter
{
    // Hunter が所持する Trap バッグ
    public Backpack backpack = new Backpack();

}
