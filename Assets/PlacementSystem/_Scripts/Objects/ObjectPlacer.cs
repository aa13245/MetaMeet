using com.example;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ObjectPlacer : MonoBehaviourPunCallbacks
{
    public List<GameObject> placedGameObjects = new();
    public ObjectsDatabaseSO database;
    public GameObject loadPanel;
    public GameObject createRoomPanel;

    public TMP_InputField roomName;
    public TMP_InputField maxNum;
    internal int PlaceObject(GameObject prefab, Vector3 position, Vector3 rotation)
    {

        Debug.Log("PlaceObject position = " + position + ", rotation = " + rotation);

        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = position;
        newObject.transform.eulerAngles = rotation;
        placedGameObjects.Add(newObject);

        return placedGameObjects.Count - 1;
    }

    internal void RemoveObjectAt(int gameObjectIndex)
    {


        if (placedGameObjects.Count <= gameObjectIndex 
            || placedGameObjects[gameObjectIndex] == null)
        {
            return;
        }

        Destroy(placedGameObjects[gameObjectIndex]);

        placedGameObjects[gameObjectIndex] = null;

    }

    public void SavePlacementData()
    {
        PlacedObject placedObject;
        List<PlacedObject> placedObjectList = new List<PlacedObject>();

        foreach (GameObject gameObject in placedGameObjects)
        {
            placedObject = new PlacedObject();

            if (gameObject != null)
            {
                foreach (var item in database.objectsData)
                {
                    if (item.Name == gameObject.name.Substring(0, item.Name.Length))
                    { 
                        placedObject.obId = item.ID;
                    }

                    placedObject.x = gameObject.transform.position.x;
                    placedObject.y = gameObject.transform.position.y;
                    placedObject.z = gameObject.transform.position.z;
                    placedObject.rotY = gameObject.transform.eulerAngles.y;
                }
                placedObjectList.Add(placedObject);
            }
        }
        //SupabaseManager.instance.InsertPlacedObectByPlacedObjectClass(placedObjectList);

        Invoke("Button", 3f);
    }

    public void Button()
    {
        loadPanel.SetActive(true);
    }
    public void CreateRoom_Btn()
    {
        createRoomPanel.SetActive(true);
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
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        PhotonNetwork.LoadLevel(2);
    }
}
