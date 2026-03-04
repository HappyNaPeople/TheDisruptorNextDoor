using UnityEngine;
using System.Collections;

public class Boom : TiggerTrap
{
    public float fallSpeed;
    public float waitForBoom;
    public float boomArea;

    public GameObject flame;

    private void Start()
    {
        Init();
        SetUp();
    }

    public override void Init()
    {
        cost = 1;
        base.Init();
        trapName = TrapName.Boom;

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
        rb.simulated = true;

        while (!Condition())
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;

        }
        yield return new WaitForSeconds(waitForBoom);
        gameObject.transform.localScale = new Vector3(boomArea * 2, boomArea * 2, 1);
        yield return new WaitForEndOfFrame();
        gameObject.transform.localScale = new Vector3(1, 1, 1);

        Destroy(gameObject);
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
