using UnityEngine;
using System.Collections;

/// <summary>
/// 条件発動型 Trap の基底クラス。
/// 
/// InstallationTrap と違い、
/// 一定の条件を満たしたときに Trap を発動するタイプの Trap。
/// 
/// 派生クラスでは Condition() で発動条件を定義し、
/// TrapRule() で実際の Trap 挙動を実装する。
/// </summary>
public abstract class TiggerTrap : GridMovingTrap
{

    /// <summary>
    /// Trap の発動条件を判定する
    /// </summary>
    /// <returns>発動可能な場合 true</returns>
    public abstract bool Condition();
    /// <summary>
    /// Trap の動作ルール（コルーチン）
    /// 派生クラスで Trap の挙動を実装する
    /// </summary>
    public abstract IEnumerator TrapRule();


}
