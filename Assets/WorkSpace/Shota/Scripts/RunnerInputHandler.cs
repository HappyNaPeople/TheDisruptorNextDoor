using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class RunnerInputHandler : MonoBehaviour
{
    CharacterController _controller;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
