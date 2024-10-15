using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static IInteract;

public class Display_JSW : MonoBehaviourPunCallbacks, IInteract
{
    public int idx { get; private set; }
    GameObject whiteBoard;
    public Device_JSW device;
    public Material mat;
    public RenderTexture texture;
    public GameObject boardCamPrefab;

    public GameObject camObj { get; private set; }
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
    {
        whiteBoard = GameObject.Find("WhiteBoard(Clone)");
        if (whiteBoard == null) return;
        WhiteBoard_JSW wb = whiteBoard.GetComponent<WhiteBoard_JSW>();
        idx = wb.idxcnt++;
        wb.displays.Add(this);
        //Transform camerasTf = whiteBoard.transform.Find("Cameras");
        //boardCam = Instantiate(boardCamPrefab, camerasTf.position, Quaternion.Euler(Vector3.zero), camerasTf);
        SetGizmo();
        StartCoroutine(Init());
    }
    IEnumerator Init()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        if (PhotonNetwork.IsMasterClient)
        {
            Material newMat = new Material(mat);
            RenderTexture newTex = new RenderTexture(texture);
            newMat.mainTexture = newTex;
            GetComponent<Renderer>().material = newMat;
            Transform camerasTf = whiteBoard.transform.Find("Cameras");
            camObj = PhotonNetwork.InstantiateRoomObject("BoardCam", camerasTf.position, Quaternion.Euler(Vector3.zero));
            camObj.transform.parent = camerasTf;
            camObj.gameObject.name = idx.ToString();
            Cam_JSW camComp = camObj.GetComponent<Cam_JSW>();
            camComp.RPC_Init(idx);
            camComp.device = GetComponentInParent<Device_JSW>();
            cam = camObj.GetComponent<Camera>();
            cam.targetTexture = newTex;
            device.cam = cam;
            device.camComp = camComp;
        }
    }
    public void InitNotMaster(GameObject _camObj)
    {
        Material newMat = new Material(mat);
        RenderTexture newTex = new RenderTexture(texture);
        newMat.mainTexture = newTex;
        GetComponent<Renderer>().material = newMat;
        cam = _camObj.GetComponent<Camera>();
        cam.targetTexture = newTex;
        camObj = _camObj;
        device.cam = cam;
        device.camComp = _camObj.GetComponent<Cam_JSW>();
    }
    private void OnDestroy()
    {
        Destroy(camObj);
    }
    private void LateUpdate()
    {
        UpdateGizmo();
    }
    Vector3 localPos;
    public Vector2 Convert2Board()
    {
        float x = localPos.x + 0.5f;
        float y = localPos.y + 0.5f;
        float camHeight = cam.orthographicSize * 2.0f;
        float camWidth = camHeight * cam.aspect;
        float touchPosX = Mathf.Lerp(camObj.transform.position.x - camWidth / 2, camObj.transform.position.x + camWidth / 2, x);
        float touchPosY = Mathf.Lerp(camObj.transform.position.y - camHeight / 2, camObj.transform.position.y + camHeight / 2, y);
        return new Vector2(touchPosX, touchPosY);
    }
    public void Interact(Vector3 pos, KeyCode keyCode, KeyState keyState, float value)
    {
        localPos = transform.InverseTransformPoint(pos);
        Vector2 boardPos = Convert2Board();
        device.Touch(boardPos.x, boardPos.y, keyCode, keyState, value);
    }
    void UpdateGizmo()
    {   // 선택된 오브젝트 있을 때
        if (device.SelectedObj != null)
        {
            // 크기
            Vector3 objScale = device.objComp.GetScale();
            selectRT.sizeDelta = new Vector2(objScale.x * 112.5f + 1, objScale.y * 112.5f + 1) / cam.orthographicSize;
            // 위치
            float camHeight = cam.orthographicSize * 2.0f;
            float camWidth = camHeight * cam.aspect;
            Vector2 posRatio = Vector2.zero;
            posRatio.x = (device.SelectedObj.transform.position.x - (cam.transform.position.x - camWidth / 2)) / ((cam.transform.position.x + camWidth / 2) - (cam.transform.position.x - camWidth / 2));
            posRatio.y = (device.SelectedObj.transform.position.y -  (cam.transform.position.y - camHeight / 2)) / ((cam.transform.position.y + camHeight / 2) - ( cam.transform.position.y -camHeight / 2));
            selectRT.localPosition = new Vector2(posRatio.x - 0.5f, posRatio.y - 0.5f);
            optionRT.localPosition = new Vector2(posRatio.x - 0.5f, posRatio.y - 0.4f + selectRT.sizeDelta.y / 450);
            // 사이즈 조절 기즈모
            if (device.editText)
            {
                if (selectRT.GetChild(0).gameObject.activeSelf)
                {
                    selectRT.GetChild(0).gameObject.SetActive(false);
                    selectRT.GetComponent<Image>().color = new Color(0.6949685f, 0.8283311f, 1);
                }
            }
            else
            {
                if (!selectRT.GetChild(0).gameObject.activeSelf)
                {
                    selectRT.GetChild(0).gameObject.SetActive(true);
                    selectRT.GetComponent<Image>().color = new Color(0.1568627f, 0.5242739f, 1);
                }
            }
        }
        // 제거된 상황
        else
        {
            SetGizmo();
        }
    }
    public void SetGizmo()
    {
        if (device.SelectedObj == null)
        {
            if (selectRT.gameObject.activeSelf)
            {
                selectRT.gameObject.SetActive(false);
                optionRT.gameObject.SetActive(false);
                SetColorUI(false);
                SetAlignmentUI(false);
            }
        }
        else
        {
            if (!selectRT.gameObject.activeSelf)
            {
                selectRT.gameObject.SetActive(true);
                optionRT.gameObject.SetActive(true);
            }
            Obj_JSW obj = device.SelectedObj.GetComponent<Obj_JSW>();
            if (obj.objKind == Obj_JSW.ObjKind.Square)
            {
                foreach (GameObject ui in textUIs) ui.SetActive(true);
                Square_JSW square = device.SelectedObj.GetComponent<Square_JSW>();
                fontSizeIF.text = square.text.fontSize.ToString();
                colorUI.transform.parent.GetChild(0).GetComponent<Image>().color = square.GetBGColor();
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
    Dictionary<int, Select_JSW> otherSelectComps = new Dictionary<int, Select_JSW>();
    public void OtherSelect(int idx, int viewId, string nickname)
    {
        if (!otherSelectComps.ContainsKey(idx) || otherSelectComps[idx] == null)
        {   // 생성
            if (viewId != -1)
            {
                GameObject newObj = Instantiate(selectPrefab, otherSelects);
                Select_JSW comp = newObj.GetComponent<Select_JSW>();
                otherSelectComps[idx] = comp;
                comp.viewId = viewId;
                comp.obj = PhotonNetwork.GetPhotonView(viewId).GetComponent<Obj_JSW>();
                comp.device = device;
                comp.nickname = nickname;
                comp.OnChanged();
            }
        }
        else
        {   // 변경
            if (viewId != -1)
            {
                Select_JSW comp = otherSelectComps[idx];
                if (viewId != comp.viewId)
                {
                    comp.viewId = viewId;
                    comp.obj = PhotonNetwork.GetPhotonView(viewId).GetComponent<Obj_JSW>();
                    comp.transform.SetParent(otherSelects);
                    comp.OnChanged();
                }
                if (nickname != comp.nickname)
                {
                    comp.nickname = nickname;
                    comp.OnChanged();
                }
            }
            // 제거
            else
            {
                if (otherSelectComps[idx] != null) Destroy(otherSelectComps[idx].gameObject);
                otherSelectComps.Remove(idx);
            }
        }
    }
}
