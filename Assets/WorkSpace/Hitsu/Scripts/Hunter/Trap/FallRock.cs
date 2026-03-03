using UnityEngine;
using System.Collections;
public class FallRock : TiggerTrap
{
    private const int fallCoolDown = 3;
    public int fallSpeed = 1;

    public override void Init()
    {
        cost = 1;
        base.Init();
    }

    public override void SetUp()
    {
        base.SetUp();
        StartCoroutine(TrapRule());
    }


    private bool fallDone = false;
    public override bool Condition() => fallDone;

    public override IEnumerator TrapRule()
    {
        yield return new WaitForSeconds(fallCoolDown);
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
        if (!isSetup) return;

        if (!Condition())
        {
            if (collision.gameObject.CompareTag(targetTag))
            {

                
            }
            else if ((collision.gameObject.CompareTag(tripTag) || collision.gameObject.CompareTag(mapTag)))
            {
                fallDone = true;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
    }




}

