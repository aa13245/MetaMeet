using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviourPunCallbacks
{
    public static SelectionManager Instance;
    public GameObject[] playerCharacters;
    public int selectedCharacter = 0;
    //public Transform spawnPoint;
    public GameObject selectionUI;

    //public string gameScene = "TA_Level_ver3";
    private string selectedCharacterDataName = "SelectedCharacter";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HideAllCharacters();
        selectedCharacter = PlayerPrefs.GetInt(selectedCharacterDataName, 0);
        playerCharacters[selectedCharacter].SetActive(true);
    }

    void Update()
    {
        
    }

    private void HideAllCharacters()
    {
        foreach(GameObject go in playerCharacters)
        {
            go.SetActive(false);
        }
    }

    public void NextCharacter()
    {
        playerCharacters[selectedCharacter].SetActive(false);
        selectedCharacter++;
        if (selectedCharacter >= playerCharacters.Length)
        {
            selectedCharacter = 0;
        }
        playerCharacters[selectedCharacter].SetActive(true);
    }

    public void PreviousCharacter()
    {
        playerCharacters[selectedCharacter].SetActive(false);
        selectedCharacter--;
        if(selectedCharacter < 0)
        {
            selectedCharacter = playerCharacters.Length - 1;
        }
        playerCharacters[selectedCharacter].SetActive(true);
    }

    public void StartGame()
    {
        selectionUI.SetActive(false);
        HideAllCharacters();
        PlayerPrefs.SetInt(selectedCharacterDataName, selectedCharacter);
        PlayerPrefs.Save();
        //PlayerPrefs.SetInt(selectedCharacterDataName, selectedCharacter);
        //SceneManager.LoadScene(gameScene);
        //selectedCharacter = PlayerPrefs.GetInt(selectedCharacterDataName, 0);
        //Instantiate(playerCharacters[selectedCharacter], spawnPoint.position, Quaternion.identity);
    }
}
