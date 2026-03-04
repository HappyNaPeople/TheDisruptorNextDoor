using UnityEngine;
using System.Collections;

public class TiggerTrap : Trap
{

    public virtual bool Condition() { return false; }
    public virtual IEnumerator TrapRule() { yield return null; }


}
