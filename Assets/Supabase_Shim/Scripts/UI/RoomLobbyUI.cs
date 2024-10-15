using com.example;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit.Forms;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomLobbyUI : MonoBehaviourPunCallbacks
{
  
    public GameObject createRoomPanel;
    public GameObject swipeUIPanel;
    public SwipeUI swipeUI;

    public TMP_InputField roomName;
    public TMP_InputField maxNum;


    private void Start()
    {

        //C: \Users\hotan\AppData\LocalLow\DefaultCompany\MetaMeet

        string screenshotPath;
        screenshotPath = Application.persistentDataPath + "/0.PNG";

        Debug.Log(screenshotPath);

        FileInfo fi = new FileInfo(screenshotPath);
        if (fi.Exists) {
            Debug.Log("파일 있음");

            byte[] imageData = File.ReadAllBytes(screenshotPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            Sprite screenshotSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            swipeUI.transform.GetChild(0).GetComponent<Image>().sprite = screenshotSprite;
        }
        else { Debug.Log("파일 없음"); }


    }

    // 방꾸미기 버튼
    public void RoomEditor_Btn()
    {
        SceneManager.LoadScene(2);
    }

    // 방 입장하기 
    public void RoomEnter_Btn()
    {
        int roomId = swipeUI.currentPage;
        Debug.Log(roomId);
        
        // 현재룸 아이디 저장 
        SupabaseManager.instance.InsertCurrentRoom(roomId);

        swipeUIPanel.gameObject.SetActive(false);
        createRoomPanel.gameObject.SetActive(true);
    }

    public void CreateRoom()
    { 
        Load();
    }

    public void Load()
    {
        string _roomName = roomName.text;
        int playerCount = Convert.ToInt32(maxNum.text);

        if (_roomName.Length > 0 && playerCount > 1)
        {
            // 나의 룸을 만든다.
            RoomOptions roomOpt = new RoomOptions();
            roomOpt.MaxPlayers = playerCount;
            roomOpt.IsOpen = true;
            roomOpt.IsVisible = true;
            // 룸의 커스텀 정보를 추가한다.
            // 키 값 등록하기
            roomOpt.CustomRoomPropertiesForLobby = new string[] { "MASTER_NAME", "PASSWORD" };
            // 키에 맞는 해시 테이블 추가하기
            Hashtable roomTable = new Hashtable();
            roomTable.Add("MASTER_NAME", PhotonNetwork.NickName);
            roomTable.Add("PASSWORD", 1234);
            roomOpt.CustomRoomProperties = roomTable;

            PhotonNetwork.CreateRoom(_roomName, roomOpt, TypedLobby.Default);
        }

        SceneManager.LoadScene(3);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        // 성공적으로 방에 입장되었음을 알려준다.
        print(MethodInfo.GetCurrentMethod().Name + " is Call!");

        // 방에 입장한 친구들은 모두 1번 씬으로 이동하자!
        PhotonNetwork.LoadLevel(1);
    }




}
