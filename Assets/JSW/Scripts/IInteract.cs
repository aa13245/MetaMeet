using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteract
{
    void Interact(Vector3 pos, KeyCode keyCode, KeyState keyState, float value = 0);
    public enum KeyState
    {
        Down, Held, Up, Idle
    }
}
