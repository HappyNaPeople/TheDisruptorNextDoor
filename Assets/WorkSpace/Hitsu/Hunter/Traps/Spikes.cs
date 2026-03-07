using UnityEngine;

public class Spikes : InstallationTrap
{

    public override void Init()
    {
        base.Init();
        trapName = TrapName.Spikes;
        cost = 1;
    }
    public override void SetUp()
    {
        base.SetUp();
        StartCoroutine(FallAndSetUp());
    }

}
