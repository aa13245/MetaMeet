using com.example;
using ExitGames.Client.Photon.StructWrapping;
using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BuildingState
{
    None,
    PlacementState,
    RemovingState,
    SaveState,
}

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text_state;


    [Header("���콺 ���� �Ŵ���")]
    [SerializeField] private InputManager inputManager;

    [Header("�׸��� ��ǥ ����")]
    [SerializeField] Grid grid;

    [Header("�׸��� ���̴�")]
    [SerializeField] private GameObject gridVisualization;

    [Header("������Ʈ ������")]
    [SerializeField] private ObjectsDatabaseSO database;

    [Header("������Ʈ ���̴� ó�� �ý���")]
    [SerializeField] PreviewSystem preview;

    [Header("���� ����")]
    [SerializeField] private BuildingState currentState = BuildingState.None;

    [Header("������Ʈ �������� ������")]
    [SerializeField] private Vector3 previewObjectPosition;

    [Header("�浹 ó�� ������Ʈ ��ǥ ���� ����Ʈ - �ٴ� & ������Ʈ")]
    public GridData furnitureData, floorData;


    [Header("���� ������Ʈ ���� ��ǥ ����Ʈ")]
    public List<GameObject> placedGameobjects = new List<GameObject>();

    [Header("���� ������Ʈ �ε���")]
    [SerializeField] private int selectedObjectIndex = -1;

    [Header("���콺�� ������")]
    [SerializeField] private Vector3 mousePosition;
    [Header("�׸����� ������")]
    [SerializeField] private Vector3Int gridPosition;


    [Header("������Ʈ �����Ǹ� �� ������Ʈ�� ������")]
    [SerializeField] private Vector3 createObjectPosition;
    [Header("������Ʈ �����Ǹ� �� ������Ʈ�� ȸ����")]
    [SerializeField] private Vector3 createObjectRotation;
    [Header("�׸����� �������� ������ ������")]
    [SerializeField] private Vector3Int lastDetectedPosition = Vector3Int.zero;

    [SerializeField] private int rotation;


    [Header("�浹�Ǵ� ��ǥ���� ����")]
    public bool placementValidity;

    public void SaveCheckPlacementData()
    {
        furnitureData.SaveCheckData();

        //ScreenCapture.CaptureScreenshot("Assets/PlacementSystem/_Icon/ScreenShot/SomeLevel.PNG");

        Debug.Log(Application.dataPath);
    }

    public void SavePlacementData()
    {
        PlacedObject placedObject;
        List<PlacedObject> placedObjectList = new List<PlacedObject>();

        foreach (GameObject gameObject in placedGameobjects)
        {
            placedObject = new PlacedObject();

            if (gameObject != null)
            {
                foreach (var item in database.objectsData)
                {

                    if (item.Name == gameObject.name.Substring(0, Mathf.Min(gameObject.name.Length, item.Name.Length)))
                    {
                        placedObject.roomid = 0;
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

        // ������ ���� 
        SupabaseManager.instance.InsertPlacedObectByPlacedObjectClass(placedObjectList);

        // ȭ�� ĸ�� ����ÿ��� ���� �ϵ��� ����
        string path = Path.Combine(Application.persistentDataPath, "0.PNG");
        ScreenCapture.CaptureScreenshot(path);

        //���� �޼��� ���� ĸ�Ľÿ� �Ⱥ��̱� ���� ����
        Invoke("SaveState", 1f);

        //���� ������ ó���� ���� ����
        Invoke("LoadScene", 3f);
    }
    public void SaveState()
    {
        currentState = BuildingState.SaveState;
        text_state.text = "<color=#FFFFFF>������ �Ϸ� �Ǿ����ϴ�.</color>";
    }


    public void LoadScene()
    {

        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        StopPlacement();
        floorData = new GridData();
        furnitureData = new GridData();
    }

    

    public void StartRemoving()
    {
        StopPlacement();
        gridVisualization.SetActive(true);

        currentState = BuildingState.RemovingState;



        // ���콺 ��ư Ŭ���Ŀ�
        inputManager.OnClicked += Placestructure;
        inputManager.OnExit += StopPlacement;
    }

    // ��ġ ����
    public void StartPlacement(int ID)
    {
        StopPlacement();
        // �׸��� ����Ʈ�� �Ҵ�.
        gridVisualization.SetActive(true);

        currentState = BuildingState.PlacementState;

        if (currentState == BuildingState.PlacementState)
        {
            // ���� ���õ� ������Ʈ�� �ε��� �ֱ�
            selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);

            if (selectedObjectIndex > -1)
            {
                // ������ �ֽ�ȭ
                preview.SetDynamicObjectSize(database.objectsData[selectedObjectIndex].Size);
                // ���� ������ �ٽ� ����
                preview.SetDriectionData(preview.GetDriectionObjectIndex(), database.objectsData[selectedObjectIndex].Size);

                // ������ �׸���
                preview.StartShowingPlacementPreview(
                        database.objectsData[selectedObjectIndex].Prefab,
                        database.objectsData[selectedObjectIndex].Size
                );
            }
            // �ε����� ���������� �������� �ʾҴٸ� (���� ó��)
            else
            {
                Debug.LogError($"No ID found {ID}");
                return;
            }
        }

        // ���콺 ��ư Ŭ���Ŀ�
        inputManager.OnClicked += Placestructure;
        inputManager.OnExit += StopPlacement;
    }

    // ��ġ ��� ���� 
    public void StopPlacement()
    {
        if (currentState == BuildingState.None) return;

        //gridVisualization.SetActive(false);

        if (currentState == BuildingState.PlacementState)
        {
            preview.StopShowingPreview();
        }

        if (currentState == BuildingState.RemovingState)
        {
            preview.StopShowingPreview();
        }

        inputManager.OnClicked -= Placestructure;
        inputManager.OnExit -= StopPlacement;

        lastDetectedPosition = Vector3Int.zero;

        //������ ������Ʈ�� �������� �ٽ� ������
        createObjectPosition = Vector3.zero;
        //������ ������Ʈ�� ȸ������ �ٽ� ������
        createObjectRotation = Vector3.zero;

        currentState = BuildingState.None;
    }

    // ���콺 Ŭ���Ŀ� 
    // ���콺 ��ǥ�� �� ��ǥ�� ��ȯ�Ͽ� 
    // ������Ʈ �����Ŀ� �� ��ǥ�� �ٽ� ���� ��ǥ�� ��ȯ�Ͽ� 
    // ������Ʈ�� �������� �����մϴ�.
    private void Placestructure()
    {
        //������ ��ġ�� UI�� �ִٸ� ���� ���� (���� ó��)
        if (inputManager.IsPointerOverUI())
        {
            return;
        }


        /// �߿� �κ� !!
        /// // grid.WorldToCell �� ���忡 �������� �������� �׸��忡 ���� �������� ������
        /// // grid.CellToWorld �� �̹� ��ȯ�� �� �������� ���� ���������� �ٽ� ���� ���� 
        /// // �̰� �̳� �߿� !!

        ////////////////////////
        // ���콺 Ŭ���Ŀ� �ϴ� �۾� 
        ////////////////////////
        ///
        // ���콺�� ������ ��������
        mousePosition = inputManager.GetSelectedMapPosition();
        // ũ������ ������ �������� 
        gridPosition = grid.WorldToCell(mousePosition);

        if (currentState == BuildingState.PlacementState)
        {
            //
            placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

            // ��ġ�� �ȵǿ�
            if (placementValidity == false)
            {
                return;
            }

            //////////////////////////
            // ������ �������� �������� ������Ʈ�� �����մϴ�.
            /////////////////////////

            int index = PlaceObject(database.objectsData[selectedObjectIndex].Prefab
            , grid.CellToWorld(gridPosition) + preview.GetDriectionPosition(preview.GetDriectionObjectIndex())
            , preview.GetDritectionRotation(preview.GetDriectionObjectIndex()));

            // �������� ������Ʈ���� ���� 
            GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;

            // �������� ������Ʈ���� �����Ͽ� ������ ������..
            // ���������� �����Ͽ� ���߿� ������ ������ �̹����� ���ü� �ֵ���
            // �浹 ó�� �ϱ� ���� ����
            selectedData.AddObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size
                , database.objectsData[selectedObjectIndex].ID, placedGameobjects.Count - 1, preview.GetDriectionObjectIndex(), preview.GetDynamicObjectSize());

            preview.UpdatePosition(grid.CellToWorld(gridPosition), false);
        }

        if (currentState == BuildingState.RemovingState)
        {
            int gameObjectIndex = -1;

            GridData selectedData = null;
            if (furnitureData.CanPlaceObjectAt(gridPosition, Vector2Int.one, preview.GetDriectionObjectIndex() ,Vector2Int.one) == false)
            {
                selectedData = furnitureData;
            }
            else if (floorData.CanPlaceObjectAt(gridPosition, Vector2Int.one, preview.GetDriectionObjectIndex(), Vector2Int.one) == false)
            {
                selectedData = floorData;

            }

            if (selectedData == null)
            {
                //sound
                //soundFeedback.PlaySound(SoundType.wrongPlacement);
            }
            else
            {
                //soundFeedback.PlaySound(SoundType.Remove);
                gameObjectIndex = selectedData.GetRepresentationIndex(gridPosition);
                if (gameObjectIndex == -1)
                    return;
                selectedData.RemoveObjectAt(gridPosition);
                RemoveObjectAt(gameObjectIndex);
            }
            Vector3 cellPosition = grid.CellToWorld(gridPosition);

            if (gameObjectIndex == -1) return;

            if (preview != null)
            { 
                preview.UpdatePosition(cellPosition, CheckPlacementValidity(gridPosition, gameObjectIndex));
            }

        }

    }

    public void RemoveObjectAt(int gameObjectIndex)
    {
        if (placedGameobjects.Count <= gameObjectIndex
            || placedGameobjects[gameObjectIndex] == null)
            return;
        Destroy(placedGameobjects[gameObjectIndex]);
        placedGameobjects[gameObjectIndex] = null;
    }
    
    private int PlaceObject(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        // ���� ��ġ ���ۿ��� ���õ� ������Ʈ �ε����� �������� ������Ʈ�� ���� 
        GameObject newObject = Instantiate(prefab);

        //////////
        /// ���� ��ġ �����Ϳ� ȸ���κ� �ֱ� ����
        /// ���� �����Ϳ��� ȸ���� ����Ǵ� ȸ�� ������ ���� ȸ���� �����̼� Y ���� �߰��Ͽ� 
        /// ���� ��ġ �����Ϳ� �ֱ� 
        //////////

        //������ ������Ʈ�� �������� �׸��� �������� �����ͼ� �ֱ�
        //newObject.transform.position = grid.CellToWorld(gridPosition);

        newObject.transform.position = position;
        newObject.transform.eulerAngles = rotation;

        //������ ������Ʈ ����Ʈ�� �����Ͽ� ����
        placedGameobjects.Add(newObject);
        //������ ������Ʈ�� �������� �켱 ������ 
        createObjectPosition = newObject.transform.position;
        createObjectRotation = newObject.transform.eulerAngles;

        //////////
        /// ���� ��ġ �����Ϳ� ȸ���κ� �ֱ� ����
        //////////
        ///
        return placedGameobjects.Count - 1;
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        if (selectedObjectIndex == -1)
            return false;

        if (database.objectsData[selectedObjectIndex] != null)
        {
            // �������� ������Ʈ���� ����
            GridData selectData = database.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;
            return selectData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size, preview.GetDriectionObjectIndex(), preview.GetDynamicObjectSize());            
        }
        return false;
    }

    public void RotationPlacement(int driection) // ������ �޾Ƽ� ������Ʈ�� ��ġ�� �ٲ�
    {
        // ���õ� ������Ʈ�� ���̵� ������
        if (selectedObjectIndex < 0)
        {
            Debug.Log("���� ���õ� ������Ʈ�� ���̵� �����ϴ�.");
            return;
        }

        // ���� ������ ����
        preview.SetDriectionData(driection, database.objectsData[selectedObjectIndex].Size);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // ���õ� ������Ʈ�� ���̵� ������
            if (selectedObjectIndex < 0)
            {
                Debug.Log("���� ���õ� ������Ʈ�� ���̵� �����ϴ�.");
                return;
            }

            rotation++;
            if (rotation >= 4) rotation = 0;
           
            // ���� ������ ����
            preview.SetDriectionData(rotation, database.objectsData[selectedObjectIndex].Size);
            preview.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // ���õ� ������Ʈ�� ���̵� ������
            if (selectedObjectIndex < 0)
            {
                Debug.Log("���� ���õ� ������Ʈ�� ���̵� �����ϴ�.");
                return;
            }

            if (rotation == -1) rotation = 0;
            if (rotation <= 0) rotation = 4;
            rotation--;

            // ���� ������ ����
            preview.SetDriectionData(rotation, database.objectsData[selectedObjectIndex].Size);
            preview.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
        }

        if (currentState == BuildingState.None)
        {
            text_state.text = "";
            return;
        }

        // ���õ� �ε����� ���ٸ� �������� ����
        //if (selectedObjectIndex < 0)
        //    return;

        // ���콺�� ������ ��������
        mousePosition = inputManager.GetSelectedMapPosition();
        // ���콺�� ���������� �׸��� ������ ã��
        gridPosition = grid.WorldToCell(mousePosition);

        // �ٸ� ���� �̵��ߴٸ�
        if (lastDetectedPosition != gridPosition)
        {

            if (currentState == BuildingState.PlacementState)
            {
                text_state.text = "<color=#FFFFFF>��ġ����</color>";

                placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
                
                preview.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);

                // ������ ������Ʈ�� ���� ��ǥ 
                previewObjectPosition = preview.GetPreviewObjectPosition();
            }

            if (currentState == BuildingState.RemovingState)
            {
                text_state.text = "<color=#FF0000>��������</color>";

                //placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
                preview.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
            }

            lastDetectedPosition = gridPosition;
        }

    }
}
