using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class RingTrap
{
    public enum Stage
    {
        On,
        Off
    }
    public Stage stage;

    public TrapName trap;
    public int code;
    private int cost;
    public Vector3 turnAngle;


    public SpriteRenderer trap_icon;
    public Material m_ringPart;
    public Material m_insidePart;

    private bool canUse = false;

    /// <summary>
    /// (ring, insideCircle)
    /// </summary>
    ///private (string, string) canUseColor = ("#00FF00", "#FFFFFF");
    private (Color, Color) canUseColor;

    /// <summary>
    /// (ring, insideCircle)
    /// </summary>
    //private (string, string) cantUseColor = ("#FF0000", "#686868");
    private (Color, Color) cantUseColor;

    //  #2862FF
    private Color choseColor;

    public RingTrap(TrapName trapName, 
        int targetCode , float angle ,
        SpriteRenderer spriteRenderer,
        Material ring ,
        Material inside)
    {
        trap = trapName;
        code = targetCode;
        trap_icon = spriteRenderer;
        m_ringPart = ring;
        m_insidePart = inside;
        turnAngle = new Vector3(0, 0, angle);

        Init();
    }

    private void Init()
    {

        ColorUtility.TryParseHtmlString("#00FF00", out canUseColor.Item1);
        ColorUtility.TryParseHtmlString("#FFFFFF", out canUseColor.Item2);

        ColorUtility.TryParseHtmlString("#FF0000", out cantUseColor.Item1);
        ColorUtility.TryParseHtmlString("#686868", out cantUseColor.Item2);
        ColorUtility.TryParseHtmlString("#2862FF", out choseColor);

        cost = trap != TrapName.None ? GameManager.allTrap[trap].cost : 999999;
        trap_icon.sprite = trap != TrapName.None ? GameManager.allTrap[trap].icon : null;

    }


    public void UpdateUI()
    {
        (Color, Color) useColor;
        useColor = stage == Stage.On ? canUseColor : cantUseColor;
        m_ringPart.SetColor($"_color0{code}", useColor.Item1);
        m_insidePart.SetColor($"_color0{code}", useColor.Item2);
    }

    public void SetChoseColor()
    {
        m_insidePart.SetColor($"_color0{code}", choseColor);
    }
}


public class TrapRing : MonoBehaviour
{
    public enum RingTurn
    {
        Left = -1,
        Right = 1
    }


    [Header("Ring")]
    public GameObject ring;
    private Material m_ring;

    [Header("InsideCircle")]
    public GameObject insideCircle;
    private Material m_insideCircle;

    [Header("Sprite Renderer")]
    public SpriteRenderer[] spriteRenderers;

    [Header("Trap Button")]
    public List<RingTrap> ringTraps = new List<RingTrap>();
    public int choseTrap;
    public GameObject joyStick;

    int lastDir = -1;
    bool isInputActive = false;
    public void ControllerChoose(Vector2 input)
    {
        if (input.magnitude < 0.5f)
        {
            if (isInputActive && lastDir != -1 && ringTraps[lastDir].trap != TrapName.None)
            {
                RollTurn(lastDir);
            }
            else if(isInputActive && lastDir != -1 && ringTraps[lastDir].trap == TrapName.None)
            {
                UIUpdate(-1);
            }

            isInputActive = false;
            lastDir = -1;
            joyStick.transform.localPosition = Vector3.back;
            
            return;
        }
        isInputActive = true;

        Vector2 inputLocal = Quaternion.Inverse(transform.rotation) * input;
        float angle = Mathf.Atan2(inputLocal.x, inputLocal.y) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;

        int targetNumber = Mathf.RoundToInt(angle / 45f) % 8;
        joyStick.transform.localPosition = new Vector3(
            Mathf.Sin(targetNumber * 45f * Mathf.Deg2Rad) * 5,
            Mathf.Cos(targetNumber * 45f * Mathf.Deg2Rad) * 5, -1);
        if (targetNumber != lastDir)
        {
            lastDir = targetNumber;
            UIUpdate(targetNumber);
        }
    }

    public void Init(List<TrapName> choseTraps)
    {
        m_ring = ring.GetComponent<Renderer>().material;
        m_insideCircle = insideCircle.GetComponent<Renderer>().material;

        if (m_ring == null || m_insideCircle == null)
        {
            Debug.LogError("Ring references are NULL");
            return;
        }


        UISetUp(choseTraps);
    }

    private void UISetUp(List<TrapName> choseTraps)
    {
        if (choseTraps.Count> spriteRenderers.Length)
        {
            Debug.LogError("Trap count and SpriteRenderer count do not match");
            return;
        }
        ringTraps.Clear();

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            RingTrap ringTrap;
            float angle =  i * 45.0f;
            if (i < choseTraps.Count)
            {
                ringTrap = new RingTrap(choseTraps[i], i, angle, spriteRenderers[i], m_ring, m_insideCircle);
                ringTrap.stage = RingTrap.Stage.On;
            }
            else
            {
                ringTrap = new RingTrap(TrapName.None, i, angle, spriteRenderers[i], m_ring, m_insideCircle);
                ringTrap.stage = RingTrap.Stage.Off;
            }

            ringTraps.Add(ringTrap);
        }

        foreach (var trap in ringTraps)
        {
            trap.UpdateUI();
        }
    }
    private void UIUpdate(int targetNumber)
    {
        for (int i = 0; i < ringTraps.Count; i++)
        {
            if (i == targetNumber)
            {
                ringTraps[targetNumber].SetChoseColor();
            }
            else
            {
                ringTraps[i].UpdateUI();
            }
        }

    }


    Coroutine rolling;
    private void RollTurn(RingTurn ringTurn)
    {
        if(rolling != null)
        {
            StopCoroutine(rolling);
        }
        rolling = null;

        choseTrap = (choseTrap + (int)ringTurn + spriteRenderers.Length) % spriteRenderers.Length;
        int count = 0;
        while (ringTraps[choseTrap].trap == TrapName.None&& count < spriteRenderers.Length)
        {
            choseTrap = (choseTrap + (int)ringTurn + spriteRenderers.Length) % spriteRenderers.Length;
            count++;
        }

        if (count >= spriteRenderers.Length)
        {
            Debug.LogWarning("No valid trap found (all None)");
            return;
        }

        UIUpdate(choseTrap);
        rolling = StartCoroutine(RotateToAngle());
    }
    private void RollTurn(int targetNumber)
    {
        if (rolling != null)
        {
            StopCoroutine(rolling);
        }
        rolling = null;

        if (ringTraps[targetNumber].trap == TrapName.None)
        {
            Debug.LogWarning("Target is empty");
            return;
        }
        choseTrap = targetNumber;

        UIUpdate(choseTrap);
        rolling = StartCoroutine(RotateToAngle());
    }
    private IEnumerator RotateToAngle()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(ringTraps[choseTrap].turnAngle);

        float elapsed = 0f;
        float duration = 0.25f; 
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);

            foreach (var sR in spriteRenderers)
            {
                sR.transform.rotation = Quaternion.identity;
            }

            elapsed += Time.deltaTime;

            yield return null;
        }
        transform.rotation = endRotation;
        foreach (var sR in spriteRenderers)
        {
            sR.transform.rotation = Quaternion.identity;
        }

    }

    public void RingTurnTo(RingTurn ringTurn) => RollTurn(ringTurn);


    public bool test;

    private void Start()
    {
        List<TrapName> choseTraps = new List<TrapName>() { TrapName.Spikes, TrapName.FallRock, TrapName.Boom, TrapName.JumpPad, TrapName.Spikes };
        Init(choseTraps);
    }

    private void Update()
    {

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            RollTurn(RingTurn.Left);
        }
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            RollTurn(RingTurn.Right);
        }

        if (Gamepad.current != null) 
        {
            ControllerChoose(Gamepad.current.rightStick.value);

        }

        //if (test)
        //{
        //    test = false;

        //    RollTurn();
        //}
    }

}
