using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestInputSystem : MonoBehaviour
{
    private void Start()
    {
        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("PlayerActions");

        InputAction action = actionMap.FindAction("Move");
        action.started += Input_Started;
        action.performed += Input_Performed;
        action.canceled += Input_Canceled;
    }

    private void Input_Started(InputAction.CallbackContext context)
    {
        Debug.Log("Started Call " + context.ReadValue<Vector2>());
    }

    private void Input_Performed(InputAction.CallbackContext context)
    {
        Debug.Log("Performed Call " + context.ReadValue<Vector2>());
    }

    private void Input_Canceled(InputAction.CallbackContext context)
    {
        Debug.Log("Canceled Call " + context.ReadValue<Vector2>());
    }

}
