using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch_JSW : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Click();
    }
    void Click()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Interact(KeyCode.Mouse0, IInteract.KeyState.Down);
        }
        else if (Input.GetMouseButton(0))
        {
            Interact(KeyCode.Mouse0, IInteract.KeyState.Held);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Interact(KeyCode.Mouse0, IInteract.KeyState.Up);
        }
        else Interact(KeyCode.Mouse0, IInteract.KeyState.Idle);
        
        if (Input.GetMouseButtonDown(2))
        {
            Interact(KeyCode.Mouse2, IInteract.KeyState.Down);
        }
        else if (Input.GetMouseButton(2))
        {
            Interact(KeyCode.Mouse2, IInteract.KeyState.Held);
        }
        else if (Input.GetMouseButtonUp(2))
        {
            Interact(KeyCode.Mouse2, IInteract.KeyState.Up);
        }
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            Interact(KeyCode.Mouse2, IInteract.KeyState.Idle, scrollInput);
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Interact(KeyCode.Delete, IInteract.KeyState.Down);
        }
    }
    void Interact(KeyCode keyCode, IInteract.KeyState keyState, float value = 0)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Interacter"))
            {
                hit.transform.GetComponent<IInteract>().Interact(hit.point, keyCode, keyState, value);
            }
        }
    }
}
