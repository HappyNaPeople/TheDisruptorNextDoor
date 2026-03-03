using UnityEngine;
using System.Collections;

public class Boom : TiggerTrap
{
    private const int flameExistTime = 2;
    private const float beforeBoom = 0.5f;
    private const float afterBoom = 1.0f;
    private CircleCollider2D circleCollider;

    public int fallSpeed = 1;
    public GameObject flame;


    private void Start()
    {
        Init();
    }
    public override void Init()
    {
        cost = 1;

        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.radius = beforeBoom;

        base.Init();


    }

    public override void SetUp()
    {
        base.SetUp();
    }

    private bool fallDone = false;
    public override bool Condition() => fallDone;
    
    private void Flame()
    {

    }

    public override IEnumerator TrapRule()
    {
        rb.simulated = true;

        while (!Condition())
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;

        }

        gameObject.tag = mapTag;
        Destroy(rb);
        this.enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!Condition())
        {
            if (collision.gameObject.CompareTag(targetTag))
            {

            }
            else if (collision.gameObject.CompareTag(mapTag)) fallDone = true;
        }
    }

}
