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

    #region 로그인, 회원가입
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

            if (message == "로그인 성공")
            {
                LoginPanel.SetActive(false);
                // 포톤 접속
                PhotonNetwork.GameVersion = "1.0.0";
                PhotonNetwork.NickName = input_id.text;
                
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else 
        {
            loginSuccessText.text = "입력해주세요.";
            Debug.Log("입력해주세요.");        
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

    // 회원가입 버튼
    public async void Register_Btn()
    {
        string message = await SupabaseManager.instance
            .InsertCallUserData(register_input_id.text, register_input_password.text);

        if (message == "등록 되었습니다.")
        {
            LoginPanelSetActive();
        }

        registerSuccessText.text = message;
    }

    #endregion

    // 방만들기 버튼
    public void MakeRoom_Btn()
    {
        InitUI();
        SceneManager.LoadScene(1);
        //RoomLobbyPanel.gameObject.SetActive(true);
    }

    // 참여하기 버튼
    public void RoomList_Btn()
    {
        InitUI();
        RoomListPanel.SetActive(true);
    }

    /*
     * 
    // 방꾸미기 버튼
    public void RoomEditor_Btn()
    {
        SceneManager.LoadScene(1);
    }

    // 방 입장하기 
    public void RoomEnter_Btn()
    {
        // 현재 방 정보 
        // 포톤 방장 설정 화면 
        // 

        SceneManager.LoadScene(2);
    }

    // 

    */

    #region 포톤

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
            // 만일, 갱신된 룸 정보가 제거 리스트에 있다면...
            if (room.RemovedFromList)
            {
                // cachedRoomList에서 해당 룸을 제거한다.
                cachedRoomList.Remove(room);
            }
            // 그렇지 않다면...
            else
            {
                // 만일, 이미 cachedRoomList에 있는 방이라면...
                if (cachedRoomList.Contains(room))
                {
                    // 기존 룸 정보를 제거한다.
                    cachedRoomList.Remove(room);
                }
                // 새 룸을 cachedRoomList에 추가한다.
                cachedRoomList.Add(room);
            }
        }

        // 기존의 모든 방 정보를 삭제한다.
        for (int i = 0; i < scrollContent.childCount; i++)
        {
            Destroy(scrollContent.GetChild(i).gameObject);
        }

        foreach (RoomInfo room in cachedRoomList)
        {
            // cachedRoomList에 있는 모든 방을 만들어서 스크롤뷰에 추가한다.
            GameObject go = Instantiate(roomPrefab, scrollContent);
            RoomPanel roomPanel = go.GetComponent<RoomPanel>();
            roomPanel.SetRoomInfo(room);
            // 버튼에 방 입장 기능 연결하기
            roomPanel.btn_join.onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRoom(room.Name);
            });
        }
    }

    #endregion
}
