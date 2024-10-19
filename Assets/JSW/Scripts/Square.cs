using Photon.Pun;
using TMPro;
using UnityEngine;

public class Square : Obj, IPunObservable
{
    public RectTransform imgTf;
    public UnityEngine.UI.Image img;
    public RectTransform inputFieldTf;
    public TMP_Text Text { get; set; }
    TMP_InputField inputField;
    public BoxCollider boxCollider;
    // Start is called before the first frame update
    void Start()
    {
        Text = inputFieldTf.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
        Text.enableWordWrapping = true;
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
    public override void ChangeObjState(ObjState s)
    {
        if (objState == s) return;
        objState = s;
        if (s == ObjState.Idle)
        {
            Color pastColor = img.color;
            pastColor.a = 1;
            img.color = pastColor;
        }
        else if (s == ObjState.Ghost)
        {
            Color pastColor = img.color;
            pastColor.a = 0.5f;
            img.color = pastColor;
        }
    }
    // ����ü
    public void RPC_Bold()
    {
        bool enable = (Text.fontStyle & FontStyles.Bold) != 0;
        pv.RPC(nameof(Bold), RpcTarget.All, enable);
    }
    [PunRPC]
    public void Bold(bool enable)
    {
        if (!enable) Text.fontStyle |= FontStyles.Bold;
        else Text.fontStyle &= ~FontStyles.Bold;
    }
    // ��Ʈ ������
    public void RPC_SetFontSize(float value)
    {
        pv.RPC(nameof(SetFontSize), RpcTarget.All, value);
    }
    [PunRPC]
    public void SetFontSize(float value)
    {
        Text.fontSize = value;
    }
    // ��� ���� ����
    public void RPC_SetBGColor(float[] color)
    {
        pv.RPC(nameof(SetBGColor), RpcTarget.All, color);
    }
    [PunRPC]
    public void SetBGColor(float[] color)
    {
        img.color = new Color(color[0], color[1], color[2], color[3]);
    }
    // Get ��� ����
    public Color GetBGColor()
    {
        return img.color;
    }
    // ���� ����
    public void RPC_SetAlignment(int option)
    {
        pv.RPC(nameof(SetAlignment), RpcTarget.All, option);
    }
    [PunRPC]
    public void SetAlignment(int option)
    {
        Text.alignment = (TextAlignmentOptions)option;
    }
    // Get ����
    public TextAlignmentOptions GetAlignment() { return Text.alignment; }

    public override void InitVirtual(float[] color, byte[] imgData)
    {
        SetBGColor(new float[] { color[0], color[1], color[2], color[3] });
        base.InitVirtual(color, imgData);
    }
    // �ؽ�Ʈ ����
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
