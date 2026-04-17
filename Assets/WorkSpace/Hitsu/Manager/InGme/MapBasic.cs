using UnityEngine;

public class MapBasic : MonoBehaviour
{
    public Transform startingTs;
    public Transform goalTs;

    public CheckPoints[] checkPoints;

    public Transform[] CheckPointsTs()
    {
        Transform[] result = new Transform[checkPoints.Length];
        for(int  i=0; i< result.Length; i++)
        {
            checkPoints[i].AnimationControl(false);
            result[i] = checkPoints[i].gameObject.transform;
        }
        return result;
    }

    public void ResetCheckPoints()
    {
        foreach(var checkPoint in checkPoints)
        {
            checkPoint.AnimationControl(false);
        }
    }
}
