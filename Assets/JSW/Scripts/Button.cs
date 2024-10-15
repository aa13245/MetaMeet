using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static IInteract;

public class Button : MonoBehaviour, IInteract
{
    public Device device;
    bool hover;
    public UnityEngine.UI.Image HoverImage { get; protected set; }
    public void Interact(Vector3 pos, KeyCode keyCode, KeyState keyState, float value = 0)
    {
        if (keyCode == KeyCode.Mouse0)
        {
            if (keyState == KeyState.Down) InteractFunc();
            else if (keyState == KeyState.Idle) Hover();
        }

    }
    public enum ButtonFunction
    {
        CreateSquare,
        CreateImage,
        BringToFront,
        SendToBack,
        Bold,
        Record,
        CreateDocument,
        Color,
        SetColor,
        Alignment,
        SetAlignment,
        Share,
        Following,
        Cancel
    }
    public ButtonFunction buttonKind;
    public Action InteractFunc { get; protected set; }
    void Hover()
    {
        if (HoverImage == null) return;
        hover = true;
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(IHover());
        hover = false;
    }
    private void Start()
    {
        if (buttonKind == ButtonFunction.CreateSquare)
        {
            HoverImage = transform.Find("Hover").GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                if (device.deviceState == Device.DeviceState.CreateSquare)
                {
                    device.Idle();
                }
                else
                {
                    device.CreateSquareBtn(true);
                }
            };
        }
        else if (buttonKind == ButtonFunction.CreateImage)
        {
            HoverImage = transform.Find("Hover").GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                if (device.deviceState == Device.DeviceState.CreateImage)
                {
                    device.Idle();
                }
                else
                {
                    device.CreateImageBtn(true);
                }
            };
        }
        else if (buttonKind == ButtonFunction.BringToFront)
        {
            HoverImage = GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                device.Display.SetColorUI(false);
                device.Display.SetAlignmentUI(false);
                device.objComp.RPC_MoveFrontOrBack(true);
            };
        }
        else if (buttonKind == ButtonFunction.SendToBack)
        {
            HoverImage = GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                device.Display.SetColorUI(false);
                device.Display.SetAlignmentUI(false);
                device.objComp.RPC_MoveFrontOrBack(false);
            };
        }
        else if (buttonKind == ButtonFunction.Bold)
        {
            HoverImage = GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                device.Display.SetColorUI(false);
                device.Display.SetAlignmentUI(false);
                device.SelectedObj.GetComponent<Square>().RPC_Bold();
            };
        }
        else if (buttonKind == ButtonFunction.Record)
        {
            InteractFunc = () =>
            {
                DocumentManager docMng = device.DocumentManager;
                if (docMng.IsRecording)
                {   // ≥Ï¿Ω ¡æ∑·
                    transform.GetChild(2).gameObject.SetActive(false);
                    transform.GetChild(1).gameObject.SetActive(true);
                    docMng.StopRecording();
                }
                else
                {   // ≥Ï¿Ω Ω√¿€
                    transform.GetChild(2).gameObject.SetActive(true);
                    transform.GetChild(1).gameObject.SetActive(false);
                    docMng.StartRecording();
                }
            };
        }
        else if (buttonKind == ButtonFunction.CreateDocument)
        {
            HoverImage = GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                device.DocumentManager.GetDocument();
            };
        }
        else if (buttonKind == ButtonFunction.Color)
        {
            HoverImage = GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                if (device.Display.colorUI.activeSelf == false) device.Display.SetColorUI(true);
                else device.Display.SetColorUI(false);
                device.Display.SetAlignmentUI(false);
            };
        }
        else if (buttonKind == ButtonFunction.SetColor)
        {
            HoverImage = GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                Color c = transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color;
                device.SetColor(c);
                device.Display.SetColorUI(false);
                transform.parent.parent.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = c;
            };
        }
        else if (buttonKind == ButtonFunction.Alignment)
        {
            HoverImage = GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                if (device.Display.alignmentUI.activeSelf == false) device.Display.SetAlignmentUI(true);
                else device.Display.SetAlignmentUI(false);
                device.Display.SetColorUI(false);
            };
        }
        else if (buttonKind == ButtonFunction.SetAlignment)
        {
            HoverImage = GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                TextAlignmentOptions option;
                if (gameObject.name == "Left") option = TextAlignmentOptions.Left;
                else if (gameObject.name == "Center") option = TextAlignmentOptions.Center;
                else option = TextAlignmentOptions.Right;
                device.SetAlignmnet(option);
                device.Display.SetAlignmentUI(false);
                device.Display.SetAlignmentIcon(option);
            };
        }
        else if (buttonKind == ButtonFunction.Share)
        {
            HoverImage = transform.Find("Hover").GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                device.CamComp.RPC_Share();
            };
        }
        else if (buttonKind == ButtonFunction.Following)
        {
            HoverImage = transform.Find("Hover").GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                device.CamComp.RPC_Following();
            };
        }
        else if (buttonKind == ButtonFunction.Cancel)
        {
            HoverImage = transform.Find("Hover").GetComponent<UnityEngine.UI.Image>();
            InteractFunc = () =>
            {
                device.CamComp.Cancel();
            };
        }
    }
    Coroutine hoverCoroutine;
    IEnumerator IHover()
    {
        if (!HoverImage.enabled) HoverImage.enabled = true;
        yield return null;
        if (!hover) HoverImage.enabled = false;
    }
}
