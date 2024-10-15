using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static IInteract;

public class Button_JSW : MonoBehaviour, IInteract
{
    public Device_JSW device;
    bool hover;
    public Image hoverImage { get; protected set; }
    public void Interact(Vector3 pos, KeyCode keyCode, KeyState keyState, float value = 0)
    {
        if (keyCode == KeyCode.Mouse0)
        {
            if (keyState == KeyState.Down) interactFunc();
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
    public Action interactFunc { get; protected set; }
    void Hover()
    {
        if (hoverImage == null) return;
        hover = true;
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(IHover());
        hover = false;
    }
    private void Start()
    {
        if (buttonKind == ButtonFunction.CreateSquare)
        {
            hoverImage = transform.Find("Hover").GetComponent<Image>();
            interactFunc = () =>
            {
                if (device.deviceState == Device_JSW.DeviceState.CreateSquare)
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
            hoverImage = transform.Find("Hover").GetComponent<Image>();
            interactFunc = () =>
            {
                if (device.deviceState == Device_JSW.DeviceState.CreateImage)
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
            hoverImage = GetComponent<Image>();
            interactFunc = () =>
            {
                device.display.SetColorUI(false);
                device.display.SetAlignmentUI(false);
                device.objComp.RPC_MoveFrontOrBack(true);
            };
        }
        else if (buttonKind == ButtonFunction.SendToBack)
        {
            hoverImage = GetComponent<Image>();
            interactFunc = () =>
            {
                device.display.SetColorUI(false);
                device.display.SetAlignmentUI(false);
                device.objComp.RPC_MoveFrontOrBack(false);
            };
        }
        else if (buttonKind == ButtonFunction.Bold)
        {
            hoverImage = GetComponent<Image>();
            interactFunc = () =>
            {
                device.display.SetColorUI(false);
                device.display.SetAlignmentUI(false);
                device.SelectedObj.GetComponent<Square_JSW>().RPC_Bold();
            };
        }
        else if (buttonKind == ButtonFunction.Record)
        {
            interactFunc = () =>
            {
                DocumentManager_JSW docMng = device.documentManager;
                if (docMng.isRecording)
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
            hoverImage = GetComponent<Image>();
            interactFunc = () =>
            {
                device.documentManager.GetDocument();
            };
        }
        else if (buttonKind == ButtonFunction.Color)
        {
            hoverImage = GetComponent<Image>();
            interactFunc = () =>
            {
                if (device.display.colorUI.activeSelf == false) device.display.SetColorUI(true);
                else device.display.SetColorUI(false);
                device.display.SetAlignmentUI(false);
            };
        }
        else if (buttonKind == ButtonFunction.SetColor)
        {
            hoverImage = GetComponent<Image>();
            interactFunc = () =>
            {
                Color c = transform.GetChild(0).GetComponent<Image>().color;
                device.SetColor(c);
                device.display.SetColorUI(false);
                transform.parent.parent.GetChild(0).GetComponent<Image>().color = c;
            };
        }
        else if (buttonKind == ButtonFunction.Alignment)
        {
            hoverImage = GetComponent<Image>();
            interactFunc = () =>
            {
                if (device.display.alignmentUI.activeSelf == false) device.display.SetAlignmentUI(true);
                else device.display.SetAlignmentUI(false);
                device.display.SetColorUI(false);
            };
        }
        else if (buttonKind == ButtonFunction.SetAlignment)
        {
            hoverImage = GetComponent<Image>();
            interactFunc = () =>
            {
                TextAlignmentOptions option;
                if (gameObject.name == "Left") option = TextAlignmentOptions.Left;
                else if (gameObject.name == "Center") option = TextAlignmentOptions.Center;
                else option = TextAlignmentOptions.Right;
                device.SetAlignmnet(option);
                device.display.SetAlignmentUI(false);
                device.display.SetAlignmentIcon(option);
            };
        }
        else if (buttonKind == ButtonFunction.Share)
        {
            hoverImage = transform.Find("Hover").GetComponent<Image>();
            interactFunc = () =>
            {
                device.camComp.RPC_Share();
            };
        }
        else if (buttonKind == ButtonFunction.Following)
        {
            hoverImage = transform.Find("Hover").GetComponent<Image>();
            interactFunc = () =>
            {
                device.camComp.RPC_Following();
            };
        }
        else if (buttonKind == ButtonFunction.Cancel)
        {
            hoverImage = transform.Find("Hover").GetComponent<Image>();
            interactFunc = () =>
            {
                device.camComp.Cancel();
            };
        }
    }
    Coroutine hoverCoroutine;
    IEnumerator IHover()
    {
        if (!hoverImage.enabled) hoverImage.enabled = true;
        yield return null;
        if (!hover) hoverImage.enabled = false;
    }
}
