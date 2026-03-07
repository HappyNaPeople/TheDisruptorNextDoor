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

    private IEnumerator Shaking()
    {
        Vector3 originalPos = transform.position;

        float time = 0;

        while (time < waitForBoom)
        {
            float x = Random.Range(-1f, 1f) * 0.1f;
            float y = Random.Range(-1f, 1f) * 0.1f;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;

        yield return null;

    }
    private void Explosion()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<CircleCollider2D>().radius = boomArea;

    }

    public override IEnumerator TrapRule()
    {

        rb.simulated = true;

        while (!Condition())
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;

        }
        yield return StartCoroutine(Shaking());
        Explosion();
        yield return new WaitForEndOfFrame();

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
