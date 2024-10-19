using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class DemoScene : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        // 포톤 접속
        PhotonNetwork.GameVersion = "1.0.0";
        PhotonNetwork.NickName = "test";

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        RoomOptions roomOpt = new RoomOptions();
        roomOpt.MaxPlayers = 10;
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

        PhotonNetwork.CreateRoom("Test", roomOpt, TypedLobby.Default);
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        PhotonNetwork.LoadLevel(4);
    }
}
