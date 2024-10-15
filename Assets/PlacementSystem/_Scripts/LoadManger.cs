using com.example;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Collections;

public class LoadManger : MonoBehaviourPunCallbacks
{
    public ObjectsDatabaseSO database;
    public GameObject[] playerCharacters;
    public Transform[] spawnPoints;
    public GameObject canvas;
    public GameObject whiteBoard;

    private string selectedCharacterDataName = "SelectedCharacter";
    int selectedCharacter;

    async void Start() 
    {
        if(!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Disconnected from Photon Network");
            return;
        }

        Instantiate(canvas);
        //selectedCharacter = PlayerPrefs.GetInt(selectedCharacterDataName, 0);
        //Instantiate(playerCharacters[selectedCharacter], spawnPoint.position, Quaternion.identity);
        Instantiate(whiteBoard, new Vector3(0, -10000, 0), Quaternion.identity);

        int roomId = await SupabaseManager.instance.GetCurrentRoom();

        List<PlacedObject> placedObjectList = await SupabaseManager.instance.SelectPlacedObject(roomId);
        List<int> ints = new List<int>();


        for (int i = 0; i < placedObjectList.Count; i++)
        {


            for (int j = 0; j < database.objectsData.Count; j++)
            {
                if (placedObjectList[i].obId == database.objectsData[j].ID)
                {
                    ints.Add(j);
                    //Debug.Log("obId = " + placedObjectList[i].obId + ", Name = " + database.objectsData[i].Name);

                }

            }

            if (placedObjectList[i] != null)
            { 
                if (database.objectsData[ints[i]] != null )
                { 
                    GameObject placedProp = Instantiate(database.objectsData[ints[i]].Prefab);
                    placedProp.transform.position = new Vector3(placedObjectList[i].x - 1.2f, .2f, placedObjectList[i].z);
                    placedProp.transform.eulerAngles = new Vector3(0f, placedObjectList[i].rotY, 0f);            
                }
            
            }


        }


    }



    public void StartGame()
    {
        selectedCharacter = PlayerPrefs.GetInt(selectedCharacterDataName, 0);
        Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        //PhotonNetwork.Instantiate(playerCharacters[selectedCharacter].name, spawnPosition, Quaternion.identity);
        //PlayerPrefs.SetInt(selectedCharacterDataName, selectedCharacter);
        ////SceneManager.LoadScene(gameScene);
        //selectedCharacter = PlayerPrefs.GetInt(selectedCharacterDataName, 0);
        //Instantiate(playerCharacters[selectedCharacter], spawnPoint.position, Quaternion.identity);

        GameObject player = PhotonNetwork.Instantiate(playerCharacters[selectedCharacter].name, spawnPosition, Quaternion.identity);

        player.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.layer = LayerMask.NameToLayer("Player");
        //PlayerPrefs.SetInt(selectedCharacterDataName, selectedCharacter);
        ////SceneManager.LoadScene(gameScene);
        //selectedCharacter = PlayerPrefs.GetInt(selectedCharacterDataName, 0);
        //Instantiate(playerCharacters[selectedCharacter], spawnPoint.position, Quaternion.identity);

        // ����Ŀ
        GameObject speaker = PhotonNetwork.Instantiate("Speaker", Vector3.zero, Quaternion.identity);
        speaker.GetComponent<SpeakerInit>().RPC_Init(player.GetComponent<PhotonView>().ViewID);
    }
    

}
