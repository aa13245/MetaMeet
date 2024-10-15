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
    {   // 초기 배치를 각 클라이언트가 supabase로부터 받아 각자 Instantiate해서 (포톤으로 동기화 X)
        // 디바이스들의 인덱스를 클라이언트들이 각자 부여하고 whiteBoard에서 관리함 (인덱스로 동기화)
        whiteBoard = GameObject.Find("WhiteBoard(Clone)");
        if (whiteBoard == null) return;
        WhiteBoard wb = whiteBoard.GetComponent<WhiteBoard>();
        Idx = wb.Idxcnt++;
        wb.displays.Add(this);
        SetGizmo();
        StartCoroutine(Init());
    }
    IEnumerator Init()
    {   // 디스플레이를 렌더링하는 카메라는 포톤으로 Instantiate해서 포톤으로 동기화됨
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        if (PhotonNetwork.IsMasterClient)
        {   // 방장일 때 초기화
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
    // 방장 아닐 때 초기화
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
    // 디스플레이 터치 좌표를 화이트보드 상 좌표로 반환
    public Vector2 Convert2Board(Vector3 _localPos)
    {   // 디스플레이의 터치 지점 좌표 ( 0 ~ 1 )
        float x = _localPos.x + 0.5f;
        float y = _localPos.y + 0.5f;
        // 카메라 가로, 세로 길이
        float camHeight = cam.orthographicSize * 2.0f;
        float camWidth = camHeight * cam.aspect;
        // Lerf ( 카메라의 양쪽 끝 범위 by 터치 지점 비율 )
        float touchPosX = Mathf.Lerp(CamObj.transform.position.x - camWidth / 2, CamObj.transform.position.x + camWidth / 2, x);
        float touchPosY = Mathf.Lerp(CamObj.transform.position.y - camHeight / 2, CamObj.transform.position.y + camHeight / 2, y);
        return new Vector2(touchPosX, touchPosY);
    }
    public void Interact(Vector3 pos, KeyCode keyCode, KeyState keyState, float value)
    {
        localPos = transform.InverseTransformPoint(pos); // 디스플레이 터치 지점 로컬좌표
        Vector2 boardPos = Convert2Board(localPos);  // 카메라상 변환 좌표
        device.Touch(boardPos.x, boardPos.y, keyCode, keyState, value);
    }
    void UpdateGizmo()
    {   // 선택된 오브젝트 있을 때
        if (device.SelectedObj != null)
        {
            // 크기
            Vector3 objScale = device.objComp.GetScale();
            selectRT.sizeDelta = new Vector2(objScale.x * 112.5f + 1, objScale.y * 112.5f + 1) / cam.orthographicSize;
            // 디스플레이 상 위치 - (카메라 모서리부터의 거리 / 카메라 범위) 비율
            float camHeight = cam.orthographicSize * 2.0f;
            float camWidth = camHeight * cam.aspect;
            Vector2 posRatio = Vector2.zero;
            posRatio.x = (device.SelectedObj.transform.position.x - (cam.transform.position.x - camWidth / 2)) / ((cam.transform.position.x + camWidth / 2) - (cam.transform.position.x - camWidth / 2));
            posRatio.y = (device.SelectedObj.transform.position.y -  (cam.transform.position.y - camHeight / 2)) / ((cam.transform.position.y + camHeight / 2) - ( cam.transform.position.y -camHeight / 2));
            // 선택 UI, 옵션 UI
            selectRT.localPosition = new Vector2(posRatio.x - 0.5f, posRatio.y - 0.5f);
            optionRT.localPosition = new Vector2(posRatio.x - 0.5f, posRatio.y - 0.4f + selectRT.sizeDelta.y / 450);
            // 텍스트 수정 모드
            if (device.editText)
            {
                if (selectRT.GetChild(0).gameObject.activeSelf)
                {
                    selectRT.GetChild(0).gameObject.SetActive(false);
                    selectRT.GetComponent<UnityEngine.UI.Image>().color = new Color(0.6949685f, 0.8283311f, 1);
                }
            }
            // 사이즈 조절 기즈모
            else
            {
                if (!selectRT.GetChild(0).gameObject.activeSelf)
                {
                    selectRT.GetChild(0).gameObject.SetActive(true);
                    selectRT.GetComponent<UnityEngine.UI.Image>().color = new Color(0.1568627f, 0.5242739f, 1);
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
    {   // 선택된 오브젝트 없으면
        if (device.SelectedObj == null)
        {   // 모두 끄기
            if (selectRT.gameObject.activeSelf)
            {
                selectRT.gameObject.SetActive(false);
                optionRT.gameObject.SetActive(false);
                SetColorUI(false);
                SetAlignmentUI(false);
            }
        }
        else
        {   // 전부 켜기
            if (!selectRT.gameObject.activeSelf)
            {
                selectRT.gameObject.SetActive(true);
                optionRT.gameObject.SetActive(true);
            }
            Obj obj = device.SelectedObj.GetComponent<Obj>();
            // 옵션 UI 새로고침
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
    // 타 유저 선택 UI
    public void OtherSelect(int idx, int viewId, string nickname)
    {
        if (!otherSelectComps.ContainsKey(idx) || otherSelectComps[idx] == null)
        {   // 생성
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
        {   // 변경
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
            // 제거
            else
            {
                if (otherSelectComps[idx] != null) Destroy(otherSelectComps[idx].gameObject);
                otherSelectComps.Remove(idx);
            }
        }
    }
}
