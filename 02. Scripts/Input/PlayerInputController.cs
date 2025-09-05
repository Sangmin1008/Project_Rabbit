using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public InputSystem_Actions PlayerInputs { get; private set; }
    public InputSystem_Actions.PlayerActions PlayerActions { get; private set; }

    private void Awake()
    {
        PlayerInputs = new InputSystem_Actions();
        PlayerActions = PlayerInputs.Player;
    }

    private void OnEnable()
    {
        PlayerInputs.Enable();
    }

    private void OnDisable()
    {
        PlayerInputs.Disable();
    }
}