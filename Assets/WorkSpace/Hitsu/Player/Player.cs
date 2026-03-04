using System.Drawing;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Hunter hunter;

    public int displayCode {  get; private set; }
    public enum Job
    {
        Runner,
        Hunter
    }
    public Job jop {  get; private set; }
    public void SetJop(Job targetJop) => jop = targetJop;
    public void PlayerInit(Job targetJop, int targetDisplay)
    {
        jop = targetJop;
        displayCode = targetDisplay;



    }

    private void JopUpdate()
    {
        switch (jop)
        {
            case Job.Runner:

                break;

            case Job.Hunter:

                break;
        }
    }

    private void Update()
    {
        JopUpdate();
    }

}
