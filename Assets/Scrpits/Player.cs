using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public CharacterController2D Controller;
    public InputData inputData;

    private void Start()
    {
        inputData = Controller.GetInputData();
    }

    public void Update()
    {
        inputData.X = Input.GetAxis("Horizontal");
        inputData.Y = Input.GetAxis("Vertical");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            inputData.Jump = true;
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            inputData.Dash = true;
        }
        inputData.Grab = Input.GetKey(KeyCode.LeftShift);
    }
}

[Serializable]
public class InputData
{
    public float X;
    public float Y;
    public bool Jump;
    public bool Dash;
    public bool Grab;
}