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


    [Header("마우스 정보 매니저")]
    [SerializeField] private InputManager inputManager;

    [Header("그리드 좌표 정보")]
    [SerializeField] Grid grid;

    [Header("그리드 쉐이더")]
    [SerializeField] private GameObject gridVisualization;

    [Header("오브젝트 데이터")]
    [SerializeField] private ObjectsDatabaseSO database;

    [Header("오브젝트 쉐이더 처리 시스템")]
    [SerializeField] PreviewSystem preview;

    [Header("현재 상태")]
    [SerializeField] private BuildingState currentState = BuildingState.None;

    [Header("오브젝트 프리뷰의 포지션")]
    [SerializeField] private Vector3 previewObjectPosition;

    [Header("충돌 처리 오브젝트 좌표 저장 리스트 - 바닥 & 오브젝트")]
    public GridData furnitureData, floorData;


    [Header("최종 오브젝트 저장 좌표 리스트")]
    public List<GameObject> placedGameobjects = new List<GameObject>();

    [Header("선택 오브젝트 인덱스")]
    [SerializeField] private int selectedObjectIndex = -1;

    [Header("마우스의 포지션")]
    [SerializeField] private Vector3 mousePosition;
    [Header("그리드의 포지션")]
    [SerializeField] private Vector3Int gridPosition;


    [Header("오브젝트 생성되면 그 오브젝트의 포지션")]
    [SerializeField] private Vector3 createObjectPosition;
    [Header("오브젝트 생성되면 그 오브젝트의 회전값")]
    [SerializeField] private Vector3 createObjectRotation;
    [Header("그리드의 포지션의 마지막 포지션")]
    [SerializeField] private Vector3Int lastDetectedPosition = Vector3Int.zero;

    [SerializeField] private int rotation;


    [Header("충돌되는 좌표인지 여부")]
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

        // 데이터 저장 
        SupabaseManager.instance.InsertPlacedObectByPlacedObjectClass(placedObjectList);

        // 화면 캡쳐 빌드시에도 가능 하도록 변경
        string path = Path.Combine(Application.persistentDataPath, "0.PNG");
        ScreenCapture.CaptureScreenshot(path);

        //저장 메세지 띄우기 캡쳐시에 안보이기 위해 지연
        Invoke("SaveState", 1f);

        //저장 데이터 처리를 위해 지연
        Invoke("LoadScene", 3f);
    }
    public void SaveState()
    {
        currentState = BuildingState.SaveState;
        text_state.text = "<color=#FFFFFF>저장이 완료 되었습니다.</color>";
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



        // 마우스 버튼 클릭후에
        inputManager.OnClicked += Placestructure;
        inputManager.OnExit += StopPlacement;
    }

    // 배치 시작
    public void StartPlacement(int ID)
    {
        StopPlacement();
        // 그리드 이펙트를 켠다.
        gridVisualization.SetActive(true);

        currentState = BuildingState.PlacementState;

        if (currentState == BuildingState.PlacementState)
        {
            // 현재 선택된 오브젝트의 인덱스 넣기
            selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);

            if (selectedObjectIndex > -1)
            {
                // 사이즈 최신화
                preview.SetDynamicObjectSize(database.objectsData[selectedObjectIndex].Size);
                // 방향 포지션 다시 셋팅
                preview.SetDriectionData(preview.GetDriectionObjectIndex(), database.objectsData[selectedObjectIndex].Size);

                // 프리뷰 그리기
                preview.StartShowingPlacementPreview(
                        database.objectsData[selectedObjectIndex].Prefab,
                        database.objectsData[selectedObjectIndex].Size
                );
            }
            // 인덱스가 정상적으로 생성되지 않았다면 (예외 처리)
            else
            {
                Debug.LogError($"No ID found {ID}");
                return;
            }
        }

        // 마우스 버튼 클릭후에
        inputManager.OnClicked += Placestructure;
        inputManager.OnExit += StopPlacement;
    }

    // 배치 모드 끄기 
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

        //생성된 오브젝트의 포지션을 다시 제거함
        createObjectPosition = Vector3.zero;
        //생성된 오브젝트의 회전값을 다시 제거함
        createObjectRotation = Vector3.zero;

        currentState = BuildingState.None;
    }

    // 마우스 클릭후에 
    // 마우스 좌표를 셀 좌표로 변환하여 
    // 오브젝트 생성후에 셀 좌표를 다시 월드 좌표로 변환하여 
    // 오브젝트의 포지션을 생성합니다.
    private void Placestructure()
    {
        //포인터 위치에 UI가 있다면 하지 않음 (예외 처리)
        if (inputManager.IsPointerOverUI())
        {
            return;
        }


        /// 중요 부분 !!
        /// // grid.WorldToCell 은 월드에 포지션을 기준으로 그리드에 셀의 포지션을 가져옴
        /// // grid.CellToWorld 는 이미 변환된 셀 포지션을 월드 포지션으로 다시 변경 해줌 
        /// // 이거 겁나 중요 !!

        ////////////////////////
        // 마우스 클릭후에 하는 작업 
        ////////////////////////
        ///
        // 마우스의 포지션 가져오기
        mousePosition = inputManager.GetSelectedMapPosition();
        // 크리드의 포지션 가져오기 
        gridPosition = grid.WorldToCell(mousePosition);

        if (currentState == BuildingState.PlacementState)
        {
            //
            placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

            // 배치가 안되요
            if (placementValidity == false)
            {
                return;
            }

            //////////////////////////
            // 가져온 포지션을 기준으로 오브젝트를 생성합니다.
            /////////////////////////

            int index = PlaceObject(database.objectsData[selectedObjectIndex].Prefab
            , grid.CellToWorld(gridPosition) + preview.GetDriectionPosition(preview.GetDriectionObjectIndex())
            , preview.GetDritectionRotation(preview.GetDriectionObjectIndex()));

            // 장판인지 오브젝트인지 구분 
            GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;

            // 장판인지 오브젝트인지 구분하여 데이터 저장함..
            // 지속적으로 저장하여 나중에 빨간색 프리뷰 이미지가 나올수 있도록
            // 충돌 처리 하기 위해 저장
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
        // 최초 배치 시작에서 선택된 오브젝트 인덱스를 기준으로 오브젝트를 생성 
        GameObject newObject = Instantiate(prefab);

        //////////
        /// 최종 배치 데이터에 회전부분 넣기 시작
        /// 기존 데이터에서 회전시 진행되는 회전 포지션 값과 회전에 로테이션 Y 값을 추가하여 
        /// 최종 배치 데이터에 넣기 
        //////////

        //생성된 오브젝트의 포지션을 그리드 포지션을 가져와서 넣기
        //newObject.transform.position = grid.CellToWorld(gridPosition);

        newObject.transform.position = position;
        newObject.transform.eulerAngles = rotation;

        //생성된 오브젝트 리스트에 저장하여 관리
        placedGameobjects.Add(newObject);
        //생성된 오브젝트의 포지션을 우선 저장함 
        createObjectPosition = newObject.transform.position;
        createObjectRotation = newObject.transform.eulerAngles;

        //////////
        /// 최종 배치 데이터에 회전부분 넣기 종료
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
            // 장판인지 오브젝트인지 구분
            GridData selectData = database.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;
            return selectData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size, preview.GetDriectionObjectIndex(), preview.GetDynamicObjectSize());            
        }
        return false;
    }

    public void RotationPlacement(int driection) // 방향을 받아서 오브젝트의 위치를 바꿈
    {
        // 선택된 오브젝트의 아이디가 기준임
        if (selectedObjectIndex < 0)
        {
            Debug.Log("현재 선택된 오브젝트의 아이디가 없습니다.");
            return;
        }

        // 방향 포지션 셋팅
        preview.SetDriectionData(driection, database.objectsData[selectedObjectIndex].Size);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // 선택된 오브젝트의 아이디가 기준임
            if (selectedObjectIndex < 0)
            {
                Debug.Log("현재 선택된 오브젝트의 아이디가 없습니다.");
                return;
            }

            rotation++;
            if (rotation >= 4) rotation = 0;
           
            // 방향 포지션 셋팅
            preview.SetDriectionData(rotation, database.objectsData[selectedObjectIndex].Size);
            preview.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // 선택된 오브젝트의 아이디가 기준임
            if (selectedObjectIndex < 0)
            {
                Debug.Log("현재 선택된 오브젝트의 아이디가 없습니다.");
                return;
            }

            if (rotation == -1) rotation = 0;
            if (rotation <= 0) rotation = 4;
            rotation--;

            // 방향 포지션 셋팅
            preview.SetDriectionData(rotation, database.objectsData[selectedObjectIndex].Size);
            preview.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
        }

        if (currentState == BuildingState.None)
        {
            text_state.text = "";
            return;
        }

        // 선택된 인덱스가 없다면 실행하지 않음
        //if (selectedObjectIndex < 0)
        //    return;

        // 마우스의 포지션 가져오기
        mousePosition = inputManager.GetSelectedMapPosition();
        // 마우스의 포지션으로 그리드 포지션 찾기
        gridPosition = grid.WorldToCell(mousePosition);

        // 다른 셀로 이동했다며
        if (lastDetectedPosition != gridPosition)
        {

            if (currentState == BuildingState.PlacementState)
            {
                text_state.text = "<color=#FFFFFF>배치상태</color>";

                placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
                
                preview.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);

                // 프리뷰 오브젝트의 현재 좌표 
                previewObjectPosition = preview.GetPreviewObjectPosition();
            }

            if (currentState == BuildingState.RemovingState)
            {
                text_state.text = "<color=#FF0000>삭제상태</color>";

                //placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
                preview.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
            }

            lastDetectedPosition = gridPosition;
        }

    }
}
