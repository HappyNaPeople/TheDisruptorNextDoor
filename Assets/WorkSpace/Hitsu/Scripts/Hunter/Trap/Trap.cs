using UnityEngine;
using System.Collections.Generic;

public class Trap : MonoBehaviour
{
    public const string targetTag = "Runner";
    public const string tripTag = "Trap";
    public const string mapTag = "Map";

    private int runnerCanSee;


    public int cost;
    //public Sprite sprite_Cover;
    public Rigidbody2D rb;
    public Collider2D trapCollider;
    public bool isSetup = false;


    public virtual void Init() 
    {
        trapCollider = GetComponent<Collider2D>();
        gameObject.tag = tripTag;

        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;

        runnerCanSee = LayerMask.NameToLayer("RunnerCanSee");
    }

    public virtual void SetUp()
    {
        trapCollider.isTrigger = false;
        isSetup = true;
        gameObject.layer = runnerCanSee;
    }



}
