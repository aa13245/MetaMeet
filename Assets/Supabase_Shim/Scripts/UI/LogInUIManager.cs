using com.example;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
//using static UnityEditor.PlayerSettings;
using Photon.Realtime;

public class LogInUIManager : MonoBehaviourPunCallbacks
{

    public GameObject LoginPanel;
    public GameObject RegisterPanel;
    public GameObject LobbyPanel;
    //public GameObject RoomLobbyPanel;
    public GameObject RoomListPanel;

    public TMP_InputField input_id;
    public TMP_InputField input_password;

    public TMP_InputField register_input_id;
    public TMP_InputField register_input_password;


    public TextMeshProUGUI loginSuccessText;
    public TextMeshProUGUI registerSuccessText;

    List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    public Transform scrollContent;
    public GameObject roomPrefab;

    private void Start()
    {
        InitUI();
        transform.GetChild(0).gameObject.SetActive(true);
    }

    #region �α���, ȸ������
    private void InitUI()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public async void Login()
    {
        if (input_id.text.Length > 0 && input_password.text.Length > 0)
        {
            string message = await  SupabaseManager.instance.SelectUserDataByName(input_id.text, input_password.text);
            loginSuccessText.text = message;

            if (message == "�α��� ����")
            {
                LoginPanel.SetActive(false);
                // ���� ����
                PhotonNetwork.GameVersion = "1.0.0";
                PhotonNetwork.NickName = input_id.text;
                
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else 
        {
            loginSuccessText.text = "�Է����ּ���.";
            Debug.Log("�Է����ּ���.");        
        }

    }
    public void Register()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        RegisterPanel.SetActive(true);
    }

    public void LoginPanelSetActive()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        LoginPanel.SetActive(true);
    }

    // ȸ������ ��ư
    public async void Register_Btn()
    {
        string message = await SupabaseManager.instance
            .InsertCallUserData(register_input_id.text, register_input_password.text);

        if (message == "��� �Ǿ����ϴ�.")
        {
            LoginPanelSetActive();
        }

        registerSuccessText.text = message;
    }

    #endregion

    // �游��� ��ư
    public void MakeRoom_Btn()
    {
        InitUI();
        SceneManager.LoadScene(1);
        //RoomLobbyPanel.gameObject.SetActive(true);
    }

    // �����ϱ� ��ư
    public void RoomList_Btn()
    {
        InitUI();
        RoomListPanel.SetActive(true);
    }

    /*
     * 
    // ��ٹ̱� ��ư
    public void RoomEditor_Btn()
    {
        SceneManager.LoadScene(1);
    }

    // �� �����ϱ� 
    public void RoomEnter_Btn()
    {
        // ���� �� ���� 
        // ���� ���� ���� ȭ�� 
        // 

        SceneManager.LoadScene(2);
    }

    // 

    */

    #region ����

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        LobbyPanel.SetActive(true);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogError("Disconnected from Server - " + cause);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        foreach (RoomInfo room in roomList)
        {
            // ����, ���ŵ� �� ������ ���� ����Ʈ�� �ִٸ�...
            if (room.RemovedFromList)
            {
                // cachedRoomList���� �ش� ���� �����Ѵ�.
                cachedRoomList.Remove(room);
            }
            // �׷��� �ʴٸ�...
            else
            {
                // ����, �̹� cachedRoomList�� �ִ� ���̶��...
                if (cachedRoomList.Contains(room))
                {
                    // ���� �� ������ �����Ѵ�.
                    cachedRoomList.Remove(room);
                }
                // �� ���� cachedRoomList�� �߰��Ѵ�.
                cachedRoomList.Add(room);
            }
        }

        // ������ ��� �� ������ �����Ѵ�.
        for (int i = 0; i < scrollContent.childCount; i++)
        {
            Destroy(scrollContent.GetChild(i).gameObject);
        }

        foreach (RoomInfo room in cachedRoomList)
        {
            // cachedRoomList�� �ִ� ��� ���� ���� ��ũ�Ѻ信 �߰��Ѵ�.
            GameObject go = Instantiate(roomPrefab, scrollContent);
            RoomPanel roomPanel = go.GetComponent<RoomPanel>();
            roomPanel.SetRoomInfo(room);
            // ��ư�� �� ���� ��� �����ϱ�
            roomPanel.btn_join.onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRoom(room.Name);
            });
        }
    }

    #endregion
}
