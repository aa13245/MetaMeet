using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static IInteract;

public class Display : MonoBehaviourPunCallbacks, IInteract
{
    public int Idx { get; private set; }
    GameObject whiteBoard;
    public Device device;
    public Material mat;
    public RenderTexture texture;
    public GameObject boardCamPrefab;

    public GameObject CamObj { get; private set; }
    public Camera cam;

    public GameObject testDot;
    public RectTransform selectRT;
    public Transform otherSelects;
    public RectTransform optionRT;
    public TMP_InputField fontSizeIF;
    public GameObject colorUI;
    public GameObject alignmentUI;
    public GameObject[] textUIs;
    public GameObject selectPrefab;
    
    void Start()
    {   // �ʱ� ��ġ�� �� Ŭ���̾�Ʈ�� supabase�κ��� �޾� ���� Instantiate�ؼ� (�������� ����ȭ X)
        // ����̽����� �ε����� Ŭ���̾�Ʈ���� ���� �ο��ϰ� whiteBoard���� ������ (�ε����� ����ȭ)
        whiteBoard = GameObject.Find("WhiteBoard(Clone)");
        if (whiteBoard == null) return;
        WhiteBoard wb = whiteBoard.GetComponent<WhiteBoard>();
        Idx = wb.Idxcnt++;
        wb.displays.Add(this);
        SetGizmo();
        StartCoroutine(Init());
    }
    IEnumerator Init()
    {   // ���÷��̸� �������ϴ� ī�޶�� �������� Instantiate�ؼ� �������� ����ȭ��
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        if (PhotonNetwork.IsMasterClient)
        {   // ������ �� �ʱ�ȭ
            Material newMat = new Material(mat);
            RenderTexture newTex = new RenderTexture(texture);
            newMat.mainTexture = newTex;
            GetComponent<Renderer>().material = newMat;
            Transform camerasTf = whiteBoard.transform.Find("Cameras");
            CamObj = PhotonNetwork.InstantiateRoomObject("BoardCam", camerasTf.position, Quaternion.Euler(Vector3.zero));
            CamObj.transform.parent = camerasTf;
            CamObj.gameObject.name = Idx.ToString();
            Cam camComp = CamObj.GetComponent<Cam>();
            camComp.RPC_Init(Idx);
            camComp.Device = GetComponentInParent<Device>();
            cam = CamObj.GetComponent<Camera>();
            cam.targetTexture = newTex;
            device.Cam = cam;
            device.CamComp = camComp;
        }
    }
    // ���� �ƴ� �� �ʱ�ȭ
    public void InitNotMaster(GameObject _camObj)
    {
        Material newMat = new Material(mat);
        RenderTexture newTex = new RenderTexture(texture);
        newMat.mainTexture = newTex;
        GetComponent<Renderer>().material = newMat;
        cam = _camObj.GetComponent<Camera>();
        cam.targetTexture = newTex;
        CamObj = _camObj;
        device.Cam = cam;
        device.CamComp = _camObj.GetComponent<Cam>();
    }
    private void OnDestroy()
    {
        Destroy(CamObj);
    }
    private void LateUpdate()
    {
        UpdateGizmo();
    }
    public Vector3 localPos { get; private set; }
    // ���÷��� ��ġ ��ǥ�� ȭ��Ʈ���� �� ��ǥ�� ��ȯ
    public Vector2 Convert2Board(Vector3 _localPos)
    {   // ���÷����� ��ġ ���� ��ǥ ( 0 ~ 1 )
        float x = _localPos.x + 0.5f;
        float y = _localPos.y + 0.5f;
        // ī�޶� ����, ���� ����
        float camHeight = cam.orthographicSize * 2.0f;
        float camWidth = camHeight * cam.aspect;
        // Lerf ( ī�޶��� ���� �� ���� by ��ġ ���� ���� )
        float touchPosX = Mathf.Lerp(CamObj.transform.position.x - camWidth / 2, CamObj.transform.position.x + camWidth / 2, x);
        float touchPosY = Mathf.Lerp(CamObj.transform.position.y - camHeight / 2, CamObj.transform.position.y + camHeight / 2, y);
        return new Vector2(touchPosX, touchPosY);
    }
    public void Interact(Vector3 pos, KeyCode keyCode, KeyState keyState, float value)
    {
        localPos = transform.InverseTransformPoint(pos); // ���÷��� ��ġ ���� ������ǥ
        Vector2 boardPos = Convert2Board(localPos);  // ī�޶�� ��ȯ ��ǥ
        device.Touch(boardPos.x, boardPos.y, keyCode, keyState, value);
    }
    void UpdateGizmo()
    {   // ���õ� ������Ʈ ���� ��
        if (device.SelectedObj != null)
        {
            // ũ��
            Vector3 objScale = device.objComp.GetScale();
            selectRT.sizeDelta = new Vector2(objScale.x * 112.5f + 1, objScale.y * 112.5f + 1) / cam.orthographicSize;
            // ���÷��� �� ��ġ - (ī�޶� �𼭸������� �Ÿ� / ī�޶� ����) ����
            float camHeight = cam.orthographicSize * 2.0f;
            float camWidth = camHeight * cam.aspect;
            Vector2 posRatio = Vector2.zero;
            posRatio.x = (device.SelectedObj.transform.position.x - (cam.transform.position.x - camWidth / 2)) / ((cam.transform.position.x + camWidth / 2) - (cam.transform.position.x - camWidth / 2));
            posRatio.y = (device.SelectedObj.transform.position.y -  (cam.transform.position.y - camHeight / 2)) / ((cam.transform.position.y + camHeight / 2) - ( cam.transform.position.y -camHeight / 2));
            // ���� UI, �ɼ� UI
            selectRT.localPosition = new Vector2(posRatio.x - 0.5f, posRatio.y - 0.5f);
            optionRT.localPosition = new Vector2(posRatio.x - 0.5f, posRatio.y - 0.4f + selectRT.sizeDelta.y / 450);
            // �ؽ�Ʈ ���� ���
            if (device.editText)
            {
                if (selectRT.GetChild(0).gameObject.activeSelf)
                {
                    selectRT.GetChild(0).gameObject.SetActive(false);
                    selectRT.GetComponent<UnityEngine.UI.Image>().color = new Color(0.6949685f, 0.8283311f, 1);
                }
            }
            // ������ ���� �����
            else
            {
                if (!selectRT.GetChild(0).gameObject.activeSelf)
                {
                    selectRT.GetChild(0).gameObject.SetActive(true);
                    selectRT.GetComponent<UnityEngine.UI.Image>().color = new Color(0.1568627f, 0.5242739f, 1);
                }
            }
        }
        // ���ŵ� ��Ȳ
        else
        {
            SetGizmo();
        }
    }
    public void SetGizmo()
    {   // ���õ� ������Ʈ ������
        if (device.SelectedObj == null)
        {   // ��� ����
            if (selectRT.gameObject.activeSelf)
            {
                selectRT.gameObject.SetActive(false);
                optionRT.gameObject.SetActive(false);
                SetColorUI(false);
                SetAlignmentUI(false);
            }
        }
        else
        {   // ���� �ѱ�
            if (!selectRT.gameObject.activeSelf)
            {
                selectRT.gameObject.SetActive(true);
                optionRT.gameObject.SetActive(true);
            }
            Obj obj = device.SelectedObj.GetComponent<Obj>();
            // �ɼ� UI ���ΰ�ħ
            if (obj.objKind == Obj.ObjKind.Square)
            {
                foreach (GameObject ui in textUIs) ui.SetActive(true);
                Square square = device.SelectedObj.GetComponent<Square>();
                fontSizeIF.text = square.Text.fontSize.ToString();
                colorUI.transform.parent.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = square.GetBGColor();
                SetAlignmentIcon(square.GetAlignment());
            }
            else
            {
                foreach (GameObject ui in textUIs) ui.SetActive(false);
            }
        }
    }
    public void SetColorUI(bool enable)
    {
        colorUI.SetActive(enable);
    }
    public void SetAlignmentUI(bool enable)
    {
        alignmentUI.SetActive(enable);
    }
    public void SetAlignmentIcon(TextAlignmentOptions option)
    {
        Transform btn = alignmentUI.transform.parent;
        if (option == TextAlignmentOptions.Left)
        {
            btn.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector3(-2, 0, 0);
            btn.GetChild(2).GetComponent<RectTransform>().localPosition = new Vector3(-1, -4.891542f, 0);
        }
        if (option == TextAlignmentOptions.Center)
        {
            btn.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            btn.GetChild(2).GetComponent<RectTransform>().localPosition = new Vector3(0, -4.891542f, 0);
        }
        if (option == TextAlignmentOptions.Right)
        {
            btn.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector3(2, 0, 0);
            btn.GetChild(2).GetComponent<RectTransform>().localPosition = new Vector3(1, -4.891542f, 0);
        }
    }
    Dictionary<int, Select> otherSelectComps = new Dictionary<int, Select>();
    // Ÿ ���� ���� UI
    public void OtherSelect(int idx, int viewId, string nickname)
    {
        if (!otherSelectComps.ContainsKey(idx) || otherSelectComps[idx] == null)
        {   // ����
            if (viewId != -1)
            {
                GameObject newObj = Instantiate(selectPrefab, otherSelects);
                Select comp = newObj.GetComponent<Select>();
                otherSelectComps[idx] = comp;
                comp.viewId = viewId;
                comp.obj = PhotonNetwork.GetPhotonView(viewId).GetComponent<Obj>();
                comp.device = device;
                comp.nickname = nickname;
                comp.OnChanged();
            }
        }
        else
        {   // ����
            if (viewId != -1)
            {
                Select comp = otherSelectComps[idx];
                if (viewId != comp.viewId)
                {
                    comp.viewId = viewId;
                    comp.obj = PhotonNetwork.GetPhotonView(viewId).GetComponent<Obj>();
                    comp.transform.SetParent(otherSelects);
                    comp.OnChanged();
                }
                if (nickname != comp.nickname)
                {
                    comp.nickname = nickname;
                    comp.OnChanged();
                }
            }
            // ����
            else
            {
                if (otherSelectComps[idx] != null) Destroy(otherSelectComps[idx].gameObject);
                otherSelectComps.Remove(idx);
            }
        }
    }
}
