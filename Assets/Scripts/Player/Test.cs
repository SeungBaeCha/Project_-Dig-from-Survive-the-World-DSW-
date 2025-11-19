using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    public void OnTest(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("ют╥б");
        }
    }

}
