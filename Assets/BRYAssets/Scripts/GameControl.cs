using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    public GameObject[] characters;
    public Transform playerStartPosition;
    public string menuScene = "CharacterSelectionScene";
    private string selectedCharacterDataName = "SelectedCharacter";
    int selectedCharacter;
    public GameObject playerObject;

    // Start is called before the first frame update
    void Start()
    {
        selectedCharacter = PlayerPrefs.GetInt(selectedCharacterDataName, 0);
        playerObject = Instantiate(characters[selectedCharacter], playerStartPosition.position, characters[selectedCharacter].transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuScene);
    }
}
