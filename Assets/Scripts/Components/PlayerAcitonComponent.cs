using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]

public class PlayerAcitonComponent : MonoBehaviour
{
    private ComboComponent inputQeueSys;

    private void Awake()
    {
        inputQeueSys = GetComponent<ComboComponent>();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");
    }    


}
