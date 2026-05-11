using UnityEngine;
using static Runner;

enum HardTrigger { Dead, Punch }
enum HardBool { IsRun }

public class Fillbar_UI : MonoBehaviour
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
        offset = Mathf.Abs(start.localPosition.x - end.localPosition.x);
        _runner_Animator = _runner.gameObject.GetComponent<Animator>();
    }
    public void Fill(float distance)
    {
        HeadMove(distance);
        m_fillBar_Front.SetFloat("_Fillup", distance);
    }


    public Animator fillBar_Head_Animator;
    private Runner _runner => InGame.Instance.runner;
    private Animator _runner_Animator;
    private bool _isRunning => _runner_Animator != null && _runner_Animator.GetBool("IsRunning");
    private bool isDead = false;


    private void Update()
    {
        if (fillBar_Head_Animator == null || _runner == null) return;
        if (fillBar_Head_Animator.GetBool(HardBool.IsRun.ToString()) != _isRunning) fillBar_Head_Animator.SetBool(HardBool.IsRun.ToString(), _isRunning);

        if (_runner.currentState == PlayerState.Dead && !isDead)
        {
            isDead = true;
            fillBar_Head_Animator.SetTrigger(HardTrigger.Dead.ToString());
        }
        else if (_runner.currentState != PlayerState.Dead && isDead) isDead = false;


    }

}
