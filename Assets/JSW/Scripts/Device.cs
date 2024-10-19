using TMPro;
using UnityEngine;
using static IInteract;
using SFB;
using Photon.Pun;
using static Obj;

public class Device : MonoBehaviour
{
    public WhiteBoard WhiteBoard { get; private set; }
    public Display Display { get; private set; }
    public DocumentManager DocumentManager { get; private set; }
    public GameObject Objs {  get; private set; }
    public Camera Cam { get; set; }
    public Cam CamComp { get; set; }
    public Camera FollowingTarget { get; set; }
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
            WhiteBoard = GameObject.Find("WhiteBoard(Clone)").GetComponent<WhiteBoard>();
            Display = GetComponentInChildren<Display>();
            if (deviceKind == DeviceKind.LargeScreen) DocumentManager = GetComponent<DocumentManager>();
            Objs = WhiteBoard.transform.Find("Objects").gameObject;
        }
    }
    private void LateUpdate()
    {
        // �ȷ��� ���
        if (FollowingTarget != null)
        {
            CamComp.SetPos(FollowingTarget.transform.position);
            CamComp.SetZoom(FollowingTarget.orthographicSize);
        }
        // Ŀ�� ȭ�� ������ �� ���� on/off
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
    Obj ghostObj;
    GameObject selectedObj;
    public bool EditText { get; set; }
    public Obj ObjComp { get; set; }
    public GameObject SelectedObj { get { return selectedObj; }
        set {
            selectedObj = value;
            if (selectedObj != null) ObjComp = selectedObj.GetComponent<Obj>();
            EditText = false;
            Display.SetGizmo();
            CamComp.RPC_OtherSelect(Display.Idx, selectedObj != null ? ObjComp.pv.ViewID : -1, PhotonNetwork.LocalPlayer.NickName);
        }
    }
    Vector3 mousePos = Vector3.back;
    bool doubleSelect;
    public void Touch(float x, float y, KeyCode keyCode, KeyState keyState, float value)
    {
        isMouse = true;
        // Idle
        if (deviceState == DeviceState.Idle)
        {   // ��Ŭ��
            if (keyCode == KeyCode.Mouse0)
            {
                if (keyState == KeyState.Down) // ���� �� - ����
                {
                    Ray ray = new Ray(new Vector3(x, y, WhiteBoard.transform.position.z - 10), Vector3.forward);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo))
                    {

                        GameObject newSelect = hitInfo.transform.gameObject;
                        if (newSelect.GetComponent<Obj>().objKind == ObjKind.Square && newSelect == selectedObj) doubleSelect = true;
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
                else if (keyState == KeyState.Held) // ������ ���� �� - �ű��
                {
                    if (selectedObj != null && mousePos.z != -1)
                    {
                        selectedObj.transform.Translate(new Vector3(x - mousePos.x, y - mousePos.y, 0));
                        ObjComp.RPC_SetPos(selectedObj.transform.position);
                        Vector3 newPos = new Vector2(x, y);
                        // �������� ��
                        if (mousePos != newPos)
                        {
                            doubleSelect = false;
                            EditText = false;
                        }
                        mousePos = newPos;
                    }
                }
                else if (keyState == KeyState.Up) // �� ��
                {
                    Ray ray = new Ray(new Vector3(x, y, WhiteBoard.transform.position.z - 10), Vector3.forward);
                    RaycastHit hitInfo;
                    if (doubleSelect && Physics.Raycast(ray, out hitInfo) && hitInfo.transform.gameObject == selectedObj)
                    {   // ���� ���
                        selectedObj.GetComponentInChildren<TMP_InputField>().ActivateInputField();
                        EditText = true;
                    }
                    else if (selectedObj != null) ObjComp.RPC_SetPos(selectedObj.transform.position, true);
                    mousePos = new Vector3(mousePos.x, mousePos.y, -1);
                }
            }
            // ����
            else if (keyCode == KeyCode.Delete && keyState == KeyState.Down)
            {
                if (selectedObj != null && !EditText)
                {
                    WhiteBoard.Remove(selectedObj);
                    ObjComp.RPC_Destroy();
                    SelectedObj = null;
                }
            }
            // ��Ŭ�� - ī�޶� �̵�
            else if (keyCode == KeyCode.Mouse2)
            {
                SetCam(x, y, keyState, value);
            }
        }
        // �޸���/�̹��� ��ġ
        else if (deviceState == DeviceState.CreateSquare || deviceState == DeviceState.CreateImage)
        {   // ��Ŭ��
            if (keyCode == KeyCode.Mouse0)
            {   // ������ �ʾ��� �� - ����
                if (keyState == KeyState.Idle || keyState == KeyState.Held)
                {   // ��Ʈ ����
                    if (ghost == null)
                    {   
                        if (deviceState == DeviceState.CreateSquare) // �޸���
                        {
                            ghost = PhotonNetwork.Instantiate("Square", Vector3.zero, Quaternion.identity);
                            ghostObj = ghost.GetComponent<Obj>();
                            ghostObj.RPC_Init(createSquare_Btn.GetChild(1).GetComponent<UnityEngine.UI.Image>().color);
                        }
                        else // �̹���
                        {
                            ghost = PhotonNetwork.Instantiate("Image", Vector3.zero,Quaternion.identity);
                            ghostObj = ghost.GetComponent<Obj>();
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
                            ghost.transform.SetParent(Objs.transform);
                            ghostObj.ChangeObjState(ObjState.Ghost);
                            ghostObj.WhiteBoard = WhiteBoard;
                        }
                    }
                    // ��Ʈ �̵�
                    ghost.transform.position = new Vector3(x, y, Objs.transform.position.z - 5);
                    ghostObj.RPC_SetPos(ghost.transform.position);
                }
                // �� �� - ��ġ
                else if (keyState == KeyState.Up)
                {
                    ghostObj.RPC_SetPos(ghost.transform.position, true);
                    ghost.GetComponent<Obj>().RPC_Place();
                    SelectedObj = ghost;
                    mousePos = new Vector3(x, y , -1);
                    ghost = null;
                    Idle();
                }
            }
            // ��Ŭ�� - ī�޶� �̵�
            else if (keyCode == KeyCode.Mouse2)
            {
                SetCam(x, y, keyState, value);
            }
        }
        // ������ ����
        else if (deviceState == DeviceState.SetSize && keyCode == KeyCode.Mouse0)
        {
            SetSize(x, y, keyCode, keyState);
        }
    }
    void SetCam(float x, float y, KeyState keyState, float value)
    {   // �� ȸ�� - �� ����
        if (value != 0f)
        {
            Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize - value * Cam.orthographicSize, 1, 100);
            Vector2 boardPos = Display.Convert2Board(Display.localPos);
            Cam.transform.Translate(-new Vector3(boardPos.x - x, boardPos.y - y, 0));
            CamComp.RPC_SetZoom(Cam.orthographicSize);
            CamComp.RPC_SetPos(Cam.transform.position, true);
        }
        // �巡��
        else
        {
            if (keyState == KeyState.Down)
            {
                mousePos = new Vector2(x, y);
            }
            else if (keyState == KeyState.Held)
            {
                Cam.transform.Translate(-new Vector3(x - mousePos.x, y - mousePos.y, 0));
                CamComp.RPC_SetPos(Cam.transform.position);
            }
            if (keyState == KeyState.Up)
            {
                mousePos = new Vector3(x, y, -1);
                CamComp.RPC_SetPos(Cam.transform.position, true);
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
    public void SetSize(float x, float y, KeyCode keyCode, KeyState keyState, SizeGizmo gizmo = null)
    {   // ����� ó�� ������ ��
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
        // ũ�� ���� ��
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
                ObjComp.RPC_SetPos(selectedObj.transform.position, true);
                ObjComp.RPC_SetScale(ObjComp.GetScale(), true);
            }
            // ������ �ִ� ��
            else
            {   // ���콺 �̵��� delta��ŭ ũ�� ���� �� ���ݸ�ŭ �̵�
                bool left = horizontal == Horizontal.Left;
                bool top = vertical == Vertical.Top;
                if (horizontal == Horizontal.Center) mousePos.x = x;
                else mousePos.x = left ? Mathf.Min(mousePos.x, selectedObj.transform.position.x - ObjComp.GetScale().x / 2) : Mathf.Max(mousePos.x, selectedObj.transform.position.x + ObjComp.GetScale().x / 2);
                if (vertical == Vertical.Middle) mousePos.y = y;
                else mousePos.y = top ? Mathf.Max(mousePos.y, selectedObj.transform.position.y + ObjComp.GetScale().y / 2) : Mathf.Min(mousePos.y, selectedObj.transform.position.y - ObjComp.GetScale().y / 2);
                Vector3 dv = new Vector3(x - mousePos.x, y - mousePos.y, 0);
                mousePos = new Vector2(x, y);
                Vector3 pastScale = ObjComp.GetScale();
                Vector3 newScale = new Vector3(Mathf.Max(0.1f, pastScale.x + (left ? -1 : 1) * dv.x), Mathf.Max(0.1f, pastScale.y + (top ? 1 : -1) * dv.y), 1);
                selectedObj.transform.Translate(new Vector3((left ? -1 : 1) * (newScale.x - pastScale.x), (top ? 1 : -1) * (newScale.y - pastScale.y), 0) / 2);
                ObjComp.RPC_SetPos(selectedObj.transform.position);
                ObjComp.SetScale(newScale);
                ObjComp.RPC_SetScale(newScale);
            }
        }
    }
    public void SetFontSize(string value)
    {
        if (value != "") selectedObj.GetComponent<Square>().RPC_SetFontSize(float.Parse(value));
    }
    void OffAll()
    {
        if (ghost != null) PhotonNetwork.Destroy(ghost);
        CreateSquareBtn(false);
        CreateImageBtn(false);
    }
    // ���޻���
    public void Idle()
    {
        deviceState = DeviceState.Idle;
        OffAll();
    }
    // �޸��� ���� ���
    public void CreateSquareBtn(bool enable)
    {
        if (enable)
        {
            OffAll();
            deviceState = DeviceState.CreateSquare;
            createSquare_Btn.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = new Color(0.7686f, 1, 0.835f);
            SelectedObj = null;
        }
        else
        {
            createSquare_Btn.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = Color.white;
        }
    }
    // �̹��� ���� ���
    byte[] loadTextureData;
    public void CreateImageBtn(bool enable)
    {
        if (enable)
        {
            OffAll();
            deviceState = DeviceState.CreateImage;
            createImage_Btn.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = new Color(0.7686f, 1, 0.835f);
            SelectedObj = null;
            string[] ex = new string[]
            {
                "xbm", "tif", "jfif", "ico", "tiff", "gif", "svg", "jpeg", "svgz", "jpg", "webp", "png", "bmp", "pjp", "apng", "pjpeg", "avif"
            };
            var paths = StandaloneFileBrowser.OpenFilePanel("����", "", new ExtensionFilter[1] { new ExtensionFilter("�̹��� ����", ex) }, false);
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
            createImage_Btn.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = Color.white;
        }
    }
    public void SetColor(Color c)
    {
        Square squareComp = SelectedObj.GetComponent<Square>();
        squareComp.RPC_SetBGColor(new float[] { c.r, c.g, c.b, c.a });
        if (createSquare_Btn != null)
        {
            createSquare_Btn.GetChild(1).GetComponent<UnityEngine.UI.Image>().color = c;
        }
    }
    public void SetAlignmnet(TextAlignmentOptions option)
    {
        Square squareComp = SelectedObj.GetComponent<Square>();
        squareComp.RPC_SetAlignment((int)option);
    }
    // ���� ���
    public void Share(int viewId)
    {
        // ���� ���� ��
        if (viewId == -1)
        {
            FollowingTarget = null;
            if (deviceKind == DeviceKind.PersonalDevice)
            {
                share_Btn.SetActive(true);
                following_Btn.SetActive(false);
                cancel_Btn.SetActive(false);
            }
        }
        // �ڽ��� ��
        else if (viewId == CamComp.pv.ViewID)
        {
            share_Btn.SetActive(false);
            cancel_Btn.SetActive(true);
        }
        // Ÿ ����̽���
        else if (deviceKind == DeviceKind.PersonalDevice)
        {
            share_Btn.SetActive(false);
            following_Btn.SetActive(true);
        }
        else if (deviceKind == DeviceKind.LargeScreen)
        {
            FollowingTarget = PhotonNetwork.GetPhotonView(viewId).GetComponent<Camera>();
        }
    }
    // �ȷ��� ���
    public void Following(bool on)
    {
        if (on)
        {
            FollowingTarget = PhotonNetwork.GetPhotonView(WhiteBoard.Sharer).GetComponent<Camera>();
            share_Btn.SetActive(false);
            following_Btn.SetActive(false);
            cancel_Btn.SetActive(true);
        }
        else
        {
            FollowingTarget = null;
            cancel_Btn.SetActive(false);
            if (WhiteBoard.Sharer == -1) share_Btn.SetActive(true);
            else following_Btn.SetActive(true);
        }
    }
}
