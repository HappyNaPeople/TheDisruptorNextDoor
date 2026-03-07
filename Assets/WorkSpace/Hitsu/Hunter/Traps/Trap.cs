using UnityEngine;
using System.Collections.Generic;
public enum TrapName
{
    Spikes,
    FallRock,
    Boom
}

public class Trap : MonoBehaviour
{
    public const string targetTag = "Runner";
    public const string tripTag = "Trap";
    public const string mapTag = "Map";


    public TrapName trapName;
    public int cost;
    public Rigidbody2D rb;
    public Collider2D trapCollider;
    public bool isSetup = false;


    public virtual void Init() 
    {
        trapCollider = GetComponent<Collider2D>();
        gameObject.tag = tripTag;

        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;

    }

    public virtual void SetUp()
    {
        trapCollider.isTrigger = false;
        isSetup = true;

    }



}
