using UnityEngine;

public class FillBar : MonoBehaviour
{
    public GameObject fillBar_Head;
    public SpriteRenderer fillBar_Front;
    private Material m_fillBar_Front => fillBar_Front.material;

    public Transform start;
    public Transform end;

    [Range(0, 1)] public float test;


    private float offset;
    private float HeadPosX(float distance) => start.localPosition.x + (offset * distance);
    private void HeadMove(float distance)
    {
        fillBar_Head.transform.localPosition = new Vector3(HeadPosX(distance), fillBar_Head.transform.localPosition.y, 1);
    }

    public void Init()
    {
        offset = Mathf.Abs(start.position.x - end.position.x);
    }
    public void Fill(float distance)
    {
        HeadMove(distance);
        m_fillBar_Front.SetFloat("_Fillup", distance);
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        Fill(test);
    }

    

}

