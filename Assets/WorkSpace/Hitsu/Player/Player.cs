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
    public Job job;
    public void SetJop(Job targetJop) => job = targetJop;
    public void PlayerInit(Job targetJop, int targetDisplay)
    {
        job = targetJop;
        displayCode = targetDisplay;
    }

    private void JopUpdate()
    {
        switch (job)
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
