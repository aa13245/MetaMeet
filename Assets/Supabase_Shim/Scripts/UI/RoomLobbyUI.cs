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
            Debug.Log("���� ����");

            byte[] imageData = File.ReadAllBytes(screenshotPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            Sprite screenshotSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            swipeUI.transform.GetChild(0).GetComponent<Image>().sprite = screenshotSprite;
        }
        else { Debug.Log("���� ����"); }


    }

    // ��ٹ̱� ��ư
    public void RoomEditor_Btn()
    {
        SceneManager.LoadScene(2);
    }

    // �� �����ϱ� 
    public void RoomEnter_Btn()
    {
        int roomId = swipeUI.currentPage;
        Debug.Log(roomId);
        
        // ����� ���̵� ���� 
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
            // ���� ���� �����.
            RoomOptions roomOpt = new RoomOptions();
            roomOpt.MaxPlayers = playerCount;
            roomOpt.IsOpen = true;
            roomOpt.IsVisible = true;
            // ���� Ŀ���� ������ �߰��Ѵ�.
            // Ű �� ����ϱ�
            roomOpt.CustomRoomPropertiesForLobby = new string[] { "MASTER_NAME", "PASSWORD" };
            // Ű�� �´� �ؽ� ���̺� �߰��ϱ�
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

        // ���������� �濡 ����Ǿ����� �˷��ش�.
        print(MethodInfo.GetCurrentMethod().Name + " is Call!");

        // �濡 ������ ģ������ ��� 1�� ������ �̵�����!
        PhotonNetwork.LoadLevel(1);
    }




}
