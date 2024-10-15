using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeGizmo_JSW : MonoBehaviour, IInteract
{
    public Display_JSW display;
    public Device_JSW.Horizontal horizontal;
    public Device_JSW.Vertical vertical;
    public void Interact(Vector3 pos, KeyCode keyCode, IInteract.KeyState keyState, float value = 0)
    {
        if (keyCode == KeyCode.Mouse0)
        {
            Vector3 localPos = display.transform.InverseTransformPoint(pos);
            float x = localPos.x + 0.5f;
            float y = localPos.y + 0.5f;
            float camHeight = display.cam.orthographicSize * 2.0f;
            float camWidth = camHeight * display.cam.aspect;
            float touchPosX = Mathf.Lerp(display.camObj.transform.position.x - camWidth / 2, display.camObj.transform.position.x + camWidth / 2, x);
            float touchPosY = Mathf.Lerp(display.camObj.transform.position.y - camHeight / 2, display.camObj.transform.position.y + camHeight / 2, y);

            display.device.SetSize(touchPosX, touchPosY, keyCode, keyState, this);
        }
    }
}
