using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Square_JSW : Obj_JSW, IPunObservable
{
    public RectTransform imgTf;
    public RectTransform inputFieldTf;
    public TMP_Text text { get; set; }
    TMP_InputField inputField;
    public BoxCollider boxCollider;
    // Start is called before the first frame update
    void Start()
    {
        text = inputFieldTf.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
        text.enableWordWrapping = true;
        inputField = GetComponentInChildren<TMP_InputField>();
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }
    public override Vector3 GetScale()
    {
        return imgTf.transform.localScale;
    }
    public override void SetScale(Vector3 scale)
    {
        imgTf.localScale = scale;
        inputFieldTf.sizeDelta = new Vector2(scale.x, scale.y) * 100;
        boxCollider.size = new Vector3(scale.x, scale.y, boxCollider.size.z);
    }
    public void RPC_Bold()
    {
        bool enable = (text.fontStyle & FontStyles.Bold) != 0;
        pv.RPC(nameof(Bold), RpcTarget.All, enable);
    }
    [PunRPC]
    public void Bold(bool enable)
    {
        if (!enable) text.fontStyle |= FontStyles.Bold;
        else text.fontStyle &= ~FontStyles.Bold;
    }
    public void RPC_SetFontSize(float value)
    {
        pv.RPC(nameof(SetFontSize), RpcTarget.All, value);
    }
    [PunRPC]
    public void SetFontSize(float value)
    {
        text.fontSize = value;
    }
    public void RPC_SetBGColor(float[] color)
    {
        pv.RPC(nameof(SetBGColor), RpcTarget.All, color);
    }
    [PunRPC]
    public void SetBGColor(float[] color)
    {
        transform.GetChild(0).GetComponent<Image>().color = new Color(color[0], color[1], color[2], color[3]);
    }
    public Color GetBGColor()
    {
        return transform.GetChild(0).GetComponent<Image>().color;
    }
    public void RPC_SetAlignment(int option)
    {
        pv.RPC(nameof(SetAlignment), RpcTarget.All, option);
    }
    [PunRPC]
    public void SetAlignment(int option)
    {
        text.alignment = (TextAlignmentOptions)option;
    }
    public override void InitVirtual(float[] color, byte[] imgData)
    {
        SetBGColor(new float[] { color[0], color[1], color[2], color[3] });
        base.InitVirtual(color, imgData);
    }
    public override void ChangeObjState(ObjState s)
    {
        if (objState == s) return;
        objState = s;
        if (s == ObjState.Idle)
        {
            Image image = transform.GetChild(0).GetComponent<Image>();
            Color pastColor = image.color;
            pastColor.a = 1;
            image.color = pastColor;
        }
        else if (s == ObjState.Ghost)
        {
            Image image = transform.GetChild(0).GetComponent<Image>();
            Color pastColor = image.color;
            pastColor.a = 0.5f;
            image.color = pastColor;
        }
    }
    public TextAlignmentOptions GetAlignment() { return text.alignment; }
    public void OnInputValueChanged(string text)
    {
        pv.RPC(nameof(InputChange), RpcTarget.Others, text);
    }
    [PunRPC] public void InputChange(string text)
    {
        inputField.onValueChanged.RemoveListener(OnInputValueChanged);
        inputField.text = text.Replace("\\n","\n");
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }
}
