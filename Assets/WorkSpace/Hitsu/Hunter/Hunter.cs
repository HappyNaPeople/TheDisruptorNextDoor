using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Hunter が使用するバックパック。
/// Trap を保存し、使用できる Trap のコストを管理する。
/// </summary>
public class Backpack
{
    // バックパックの最大コスト
    public const int maxTrapCount = 30;
    // 所持している Trap 一覧
    public List<TrapName> trapsPack = new List<TrapName>();
    // 現在使用しているコスト
    public int nowTrapCount => trapsPack.Count;

    /// <summary>
    /// Trap をバックパックに追加する
    /// 最大コストを超える場合は追加しない
    /// </summary>
    public void AddToBackpack(TrapName targetTrapName) => trapsPack.Add(targetTrapName);


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
