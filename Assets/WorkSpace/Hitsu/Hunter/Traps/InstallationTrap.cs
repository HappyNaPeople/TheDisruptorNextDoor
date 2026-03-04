using UnityEngine;
using System.Collections;

public class InstallationTrap : Trap
{
    private const int fallSpeed = 5;
    private bool isFallDone = false;
    public virtual IEnumerator FallAndSetUp()
    {
        rb.simulated = true;

        while (!isFallDone)
        {
            //rb.linearVelocity = Vector2.down * fallSpeed;

            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;
        }

        //rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        Destroy(rb);
        yield return null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isSetup) return;

        if (collision.gameObject.CompareTag(targetTag))
        {

            Debug.Log("Hit Runner");
        }
        else if ((collision.gameObject.CompareTag(tripTag)||collision.gameObject.CompareTag(mapTag)) && !isFallDone)
        {
            isFallDone = true;
            rb.bodyType = RigidbodyType2D.Static;
            return;
        }
    }

}
