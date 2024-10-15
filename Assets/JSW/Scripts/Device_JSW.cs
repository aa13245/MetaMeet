using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static IInteract;
using SFB;
using Photon.Pun;
using static Obj_JSW;

public class Device_JSW : MonoBehaviour
{
    public WhiteBoard_JSW whiteBoard { get; private set; }
    public Display_JSW display { get; private set; }
    public DocumentManager_JSW documentManager { get; private set; }
    public GameObject objs {  get; private set; }
    public Camera cam { get; set; }
    public Cam_JSW camComp { get; set; }
    public Camera followingTarget { get; set; }
    public Transform createSquare_Btn;
    public Transform createImage_Btn;
    public GameObject share_Btn;
    public GameObject following_Btn;
    public GameObject cancel_Btn;

    public GameObject square;
    public GameObject image;
    public enum DeviceKind { PersonalDevice, LargeScreen }
    public DeviceKind deviceKind;
    public enum DeviceState
    {
        Idle,
        CreateSquare,
        CreateImage,
        SetSize
    }
    public DeviceState deviceState;
    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("WhiteBoard(Clone)") != null)
        {
            whiteBoard = GameObject.Find("WhiteBoard(Clone)").GetComponent<WhiteBoard_JSW>();
            display = GetComponentInChildren<Display_JSW>();
            if (deviceKind == DeviceKind.LargeScreen) documentManager = GetComponent<DocumentManager_JSW>();
            objs = whiteBoard.transform.Find("Objects").gameObject;
        }
    }
    private void LateUpdate()
    {
        // 팔로잉 모드
        if (followingTarget != null)
        {
            camComp.SetPos(followingTarget.transform.position);
            camComp.SetZoom(followingTarget.orthographicSize);
        }
        // 커서 화면 나갔을 때 고스팅 on/off
        if (ghost != null)
        {
            if (!isMouse && ghost.activeSelf == true)
            {
                ghost.SetActive(false);
                ghostObj.RPC_SetActive(false);
            }
            else if (isMouse && ghost.activeSelf == false)
            {
                ghost.SetActive(true);
                ghostObj.RPC_SetActive(true);
            }
        }
        isMouse = false;
    }
    bool isMouse;
    GameObject ghost;
    Obj_JSW ghostObj;
    GameObject selectedObj;
    public bool editText { get; set; }
    public Obj_JSW objComp { get; set; }
    public GameObject SelectedObj { get { return selectedObj; }
        set {
            selectedObj = value;
            if (selectedObj != null) objComp = selectedObj.GetComponent<Obj_JSW>();
            editText = false;
            display.SetGizmo();
            camComp.RPC_OtherSelect(display.idx, selectedObj != null ? objComp.pv.ViewID : -1, PhotonNetwork.LocalPlayer.NickName);
        }
    }
    Vector3 mousePos = Vector3.back;
    bool doubleSelect;
    public void Touch(float x, float y, KeyCode keyCode, KeyState keyState, float value)
    {
        isMouse = true;
        // Idle
        if (deviceState == DeviceState.Idle)
        {   // 좌클릭
            if (keyCode == KeyCode.Mouse0)
            {
                if (keyState == KeyState.Down) // 누를 때
                {
                    Ray ray = new Ray(new Vector3(x, y, whiteBoard.transform.position.z - 10), Vector3.forward);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo))
                    {

                        GameObject newSelect = hitInfo.transform.gameObject;
                        if (newSelect.GetComponent<Obj_JSW>().objKind == Obj_JSW.ObjKind.Square && newSelect == selectedObj) doubleSelect = true;
                        else
                        {
                            doubleSelect = false;
                            SelectedObj = newSelect;
                        }
                        mousePos = new Vector2(x, y);
                    }
                    else
                    {
                        SelectedObj = null;
                    }
                }
                else if (keyState == KeyState.Held) // 누르고 있을 때
                {
                    if (selectedObj != null && mousePos.z != -1)
                    {
                        selectedObj.transform.Translate(new Vector3(x - mousePos.x, y - mousePos.y, 0));
                        objComp.RPC_SetPos(selectedObj.transform.position);
                        Vector3 newPos = new Vector2(x, y);
                        // 움직였을 때
                        if (mousePos != newPos)
                        {
                            doubleSelect = false;
                            editText = false;
                        }
                        mousePos = newPos;
                    }
                }
                else if (keyState == KeyState.Up) // 뗄 때
                {
                    Ray ray = new Ray(new Vector3(x, y, whiteBoard.transform.position.z - 10), Vector3.forward);
                    RaycastHit hitInfo;
                    if (doubleSelect && Physics.Raycast(ray, out hitInfo) && hitInfo.transform.gameObject == selectedObj)
                    {   // 수정 모드
                        selectedObj.GetComponentInChildren<TMP_InputField>().ActivateInputField();
                        editText = true;
                    }
                    else if (selectedObj != null) objComp.RPC_SetPos(selectedObj.transform.position, true);
                    mousePos = new Vector3(mousePos.x, mousePos.y, -1);
                }
            }
            // 제거
            else if (keyCode == KeyCode.Delete && keyState == KeyState.Down)
            {
                if (selectedObj != null && !editText)
                {
                    whiteBoard.Remove(selectedObj);
                    objComp.RPC_Destroy();
                    SelectedObj = null;
                }
            }
            else if (keyCode == KeyCode.Mouse2)
            {
                SetCam(x, y, keyState, value);
            }
        }
        // 네모/이미지 배치
        else if (deviceState == DeviceState.CreateSquare || deviceState == DeviceState.CreateImage)
        {
            if (keyCode == KeyCode.Mouse0)
            {   // 고스팅
                if (keyState == KeyState.Idle || keyState == KeyState.Held)
                {
                    if (ghost == null)
                    {
                        if (deviceState == DeviceState.CreateSquare)
                        {
                            ghost = PhotonNetwork.Instantiate("Square", Vector3.zero, Quaternion.identity);
                            ghostObj = ghost.GetComponent<Obj_JSW>();
                            ghostObj.RPC_Init(createSquare_Btn.GetChild(1).GetComponent<Image>().color);
                        }
                        else
                        {
                            ghost = PhotonNetwork.Instantiate("Image", Vector3.zero,Quaternion.identity);
                            ghostObj = ghost.GetComponent<Obj_JSW>();
                            // Others
                            ghostObj.RPC_Init(default, loadTextureData);
                            // Me
                            Renderer renderer = ghost.GetComponent<Renderer>();
                            renderer.material = new Material(renderer.material);
                            Texture2D texture = new Texture2D(2, 2);
                            if (texture.LoadImage(loadTextureData))
                            {
                                renderer.material.mainTexture = texture;
                                ghostObj.SetScale(new Vector3(Mathf.Max(0.5f, texture.width / 200), Mathf.Max(0.5f, texture.height / 200), 1));
                            }
                            ghost.transform.SetParent(objs.transform);
                            ghostObj.ChangeObjState(ObjState.Ghost);
                            ghostObj.whiteBoard = whiteBoard;
                        }
                    }
                    ghost.transform.position = new Vector3(x, y, objs.transform.position.z - 5);
                    ghostObj.RPC_SetPos(ghost.transform.position);
                }
                // 배치
                else if (keyState == KeyState.Up)
                {
                    ghostObj.RPC_SetPos(ghost.transform.position, true);
                    ghost.GetComponent<Obj_JSW>().RPC_Place();
                    SelectedObj = ghost;
                    mousePos = new Vector3(x, y , -1);
                    ghost = null;
                    Idle();
                }
            }
            else if (keyCode == KeyCode.Mouse2)
            {
                SetCam(x, y, keyState, value);
            }
        }
        // 사이즈 조절
        else if (deviceState == DeviceState.SetSize && keyCode == KeyCode.Mouse0)
        {
            SetSize(x, y, keyCode, keyState);
        }
    }
    void SetCam(float x, float y, KeyState keyState, float value)
    {   // 휠
        if (value != 0f)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - value * cam.orthographicSize, 1, 100);
            Vector2 boardPos = display.Convert2Board();
            cam.transform.Translate(-new Vector3(boardPos.x - x, boardPos.y - y, 0));
            camComp.RPC_SetZoom(cam.orthographicSize);
            camComp.RPC_SetPos(cam.transform.position, true);
        }
        // 드래그
        else
        {
            if (keyState == KeyState.Down)
            {
                mousePos = new Vector2(x, y);
            }
            else if (keyState == KeyState.Held)
            {
                cam.transform.Translate(-new Vector3(x - mousePos.x, y - mousePos.y, 0));
                camComp.RPC_SetPos(cam.transform.position);
            }
            if (keyState == KeyState.Up)
            {
                mousePos = new Vector3(x, y, -1);
                camComp.RPC_SetPos(cam.transform.position, true);
            }
        }
    }
    public enum Horizontal
    {
        Left, Center, Right
    }
    Horizontal horizontal;
    public enum Vertical
    {
        Bottom, Middle, Top
    }
    Vertical vertical;
    public void SetSize(float x, float y, KeyCode keyCode, KeyState keyState, SizeGizmo_JSW gizmo = null)
    {   // 기즈모 처음 눌렀을 때
        if (deviceState != DeviceState.SetSize)
        {
            if (keyState == KeyState.Down)
            {
                deviceState = DeviceState.SetSize;
                mousePos = new Vector2(x, y);
                horizontal = gizmo.horizontal;
                vertical = gizmo.vertical;
            }
        }
        // 크기 수정 중
        else
        {
            if (keyState == KeyState.Down)
            {
                deviceState = DeviceState.SetSize;
                mousePos = new Vector2(x, y);
                horizontal = gizmo.horizontal;
                vertical = gizmo.vertical;
            }
            else if (keyState == KeyState.Up || keyState == KeyState.Idle)
            {
                deviceState = DeviceState.Idle;
                objComp.RPC_SetPos(selectedObj.transform.position, true);
                objComp.RPC_SetScale(objComp.GetScale(), true);
            }
            else
            {
                bool left = horizontal == Horizontal.Left;
                bool top = vertical == Vertical.Top;
                if (horizontal == Horizontal.Center) mousePos.x = x;
                else mousePos.x = left ? Mathf.Min(mousePos.x, selectedObj.transform.position.x - objComp.GetScale().x / 2) : Mathf.Max(mousePos.x, selectedObj.transform.position.x + objComp.GetScale().x / 2);
                if (vertical == Vertical.Middle) mousePos.y = y;
                else mousePos.y = top ? Mathf.Max(mousePos.y, selectedObj.transform.position.y + objComp.GetScale().y / 2) : Mathf.Min(mousePos.y, selectedObj.transform.position.y - objComp.GetScale().y / 2);
                Vector3 dv = new Vector3(x - mousePos.x, y - mousePos.y, 0);
                mousePos = new Vector2(x, y);
                Vector3 pastScale = objComp.GetScale();
                Vector3 newScale = new Vector3(Mathf.Max(0.1f, pastScale.x + (left ? -1 : 1) * dv.x), Mathf.Max(0.1f, pastScale.y + (top ? 1 : -1) * dv.y), 1);
                selectedObj.transform.Translate(new Vector3((left ? -1 : 1) * (newScale.x - pastScale.x), (top ? 1 : -1) * (newScale.y - pastScale.y), 0) / 2);
                objComp.RPC_SetPos(selectedObj.transform.position);
                objComp.SetScale(newScale);
                objComp.RPC_SetScale(newScale);
            }
        }
    }
    public void SetFontSize(string value)
    {
        if (value != "") selectedObj.GetComponent<Square_JSW>().RPC_SetFontSize(float.Parse(value));
    }
    void OffAll()
    {
        if (ghost != null) PhotonNetwork.Destroy(ghost);
        CreateSquareBtn(false);
        CreateImageBtn(false);
    }
    public void Idle()
    {
        deviceState = DeviceState.Idle;
        OffAll();
    }
    public void CreateSquareBtn(bool enable)
    {
        if (enable)
        {
            OffAll();
            deviceState = DeviceState.CreateSquare;
            createSquare_Btn.GetChild(0).GetComponent<Image>().color = new Color(0.7686f, 1, 0.835f);
            SelectedObj = null;
        }
        else
        {
            createSquare_Btn.GetChild(0).GetComponent<Image>().color = Color.white;
        }
    }
    byte[] loadTextureData;
    public void CreateImageBtn(bool enable)
    {
        if (enable)
        {
            OffAll();
            deviceState = DeviceState.CreateImage;
            createImage_Btn.GetChild(0).GetComponent<Image>().color = new Color(0.7686f, 1, 0.835f);
            SelectedObj = null;
            string[] ex = new string[]
            {
                "xbm", "tif", "jfif", "ico", "tiff", "gif", "svg", "jpeg", "svgz", "jpg", "webp", "png", "bmp", "pjp", "apng", "pjpeg", "avif"
            };
            var paths = StandaloneFileBrowser.OpenFilePanel("열기", "", new ExtensionFilter[1] { new ExtensionFilter("이미지 파일", ex) }, false);
            if (paths.Length > 0)
            {
                byte[] fileData = System.IO.File.ReadAllBytes(paths[0]);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(fileData))
                {
                    loadTextureData = fileData;
                }
                else Idle();
            }
            else Idle();
        }
        else
        {
            createImage_Btn.GetChild(0).GetComponent<Image>().color = Color.white;
        }
    }
    public void SetColor(Color c)
    {
        Square_JSW squareComp = SelectedObj.GetComponent<Square_JSW>();
        squareComp.RPC_SetBGColor(new float[] { c.r, c.g, c.b, c.a });
        if (createSquare_Btn != null)
        {
            createSquare_Btn.GetChild(1).GetComponent<Image>().color = c;
        }
    }
    public void SetAlignmnet(TextAlignmentOptions option)
    {
        Square_JSW squareComp = SelectedObj.GetComponent<Square_JSW>();
        squareComp.RPC_SetAlignment((int)option);
    }
    public void Share(int viewId)
    {
        // 공유 껐을 때
        if (viewId == -1)
        {
            followingTarget = null;
            if (deviceKind == DeviceKind.PersonalDevice)
            {
                share_Btn.SetActive(true);
                following_Btn.SetActive(false);
                cancel_Btn.SetActive(false);
            }
        }
        // 자신일 때
        else if (viewId == camComp.pv.ViewID)
        {
            share_Btn.SetActive(false);
            cancel_Btn.SetActive(true);
        }
        // 타 디바이스들
        else if (deviceKind == DeviceKind.PersonalDevice)
        {
            share_Btn.SetActive(false);
            following_Btn.SetActive(true);
        }
        else if (deviceKind == DeviceKind.LargeScreen)
        {
            followingTarget = PhotonNetwork.GetPhotonView(viewId).GetComponent<Camera>();
        }
    }
    public void Following(bool on)
    {
        if (on)
        {
            followingTarget = PhotonNetwork.GetPhotonView(whiteBoard.sharer).GetComponent<Camera>();
            share_Btn.SetActive(false);
            following_Btn.SetActive(false);
            cancel_Btn.SetActive(true);
        }
        else
        {
            followingTarget = null;
            cancel_Btn.SetActive(false);
            if (whiteBoard.sharer == -1) share_Btn.SetActive(true);
            else following_Btn.SetActive(true);
        }
    }
}
