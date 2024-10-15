using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [Header("오브젝트 위로 띄우는 변수")]
    [SerializeField] private float previewYOffset = 0.06f;
    [Header("인디게이터")]
    [SerializeField] private GameObject cellIndicator;

    [Header("프리뷰 오브젝트")]
    [SerializeField] private GameObject previewObject;

    [Header("프리뷰 오브젝트 머티리얼")]
    [SerializeField] private Material previewMaterialPrefab;


    [Header("프리뷰 오브젝트 머티리얼 복제")]
    [SerializeField] private Material previewMaterialInstance;

    private Renderer cellIndicatorRenderer;

    [Header("방향 오브젝트 인덱스")]
    [SerializeField] private int driectionObjectIndex = 0;

    [Header("4방향 회전 Y축 쿼터니언 리스트")]
    [SerializeField]
    private Vector3[] driectionRotationList = new Vector3[4] {
        new Vector3(0, 0, 0),
        new Vector3(0, 90f, 0),
        new Vector3(0, 180f, 0),
        new Vector3(0, 270f, 0),
    };

    [Header("4방향 회전 포지션 리스트")]
    [SerializeField]
    private Vector3Int[] driectionPositionList = new Vector3Int[4] {
        Vector3Int.zero,
        Vector3Int.zero,
        Vector3Int.zero,
        Vector3Int.zero,
    };

    [Header("방향 사이즈")] // 방향이 변경될때 마다 사이즈도 변경됨
    public Vector2Int dynamicObjectSize = new Vector2Int();

    // 방향이 변경되면 변경 되야 하는 데이터 업뎃 
    public void SetDriectionData(int drectionIndex, Vector2Int size)
    {
        driectionObjectIndex = drectionIndex;

        driectionPositionList[0] = new Vector3Int(0, 0, 0);
        driectionPositionList[1] = new Vector3Int(0, 0, size.y);
        driectionPositionList[2] = new Vector3Int(size.x, 0, size.y);
        driectionPositionList[3] = new Vector3Int(size.y, 0, size.y - size.x);

        SetDynamicObjectSize(size);
    }

    // 방향이 변경됨에 따라 변경해야 하는 오브젝트의 사이즈 
    public void SetDynamicObjectSize(Vector2Int size)
    {
        // 사이즈가 같은 경우엔 처리 할 필요가 없다 
        if (size.x == size.y)
        {
            dynamicObjectSize = size;
        }
        // 사이즈가 다른 경우에 방향 회전시에 
        // 좌표가 바뀌므로 처리 한다.
        else
        {
            // 위 아래
            if (driectionObjectIndex == 0 || driectionObjectIndex == 2)
            {
                dynamicObjectSize = new Vector2Int(size.x, size.y);
            }
            // 오른쪽 왼쪽
            else if (driectionObjectIndex == 1 || driectionObjectIndex == 3)
            {
                dynamicObjectSize = new Vector2Int(size.y, size.x);
            }
        }



    }

    private void Start()
    {
        // 머터리얼 
        previewMaterialInstance = new Material(previewMaterialPrefab);
        // 인디케이터 끄기 
        cellIndicator.SetActive(false);
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }


    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size)
    {
        // 오브젝트 생성
        previewObject = Instantiate(prefab);

        // PreviewPreview
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = previewMaterialInstance;
            }
            renderer.materials = materials;
        }

        // PrepareCursor
        if (size.x > 0 || size.y > 0)
        {
            cellIndicator.transform.localScale = new Vector3(size.x, 1, size.y);
            cellIndicatorRenderer.material.mainTextureScale = size;
        }

        cellIndicator.SetActive(true);
    }

    public int GetDriectionObjectIndex()
    {
        return driectionObjectIndex;
    }

    public Vector2Int GetDynamicObjectSize()
    {
        return dynamicObjectSize;
    }

    public Vector3Int GetDriectionPosition(int driectionObjectIndex)
    {
        return driectionPositionList[driectionObjectIndex];
    }

    public Vector3 GetDritectionRotation(int driectionObjectIndex)
    {
        return driectionRotationList[driectionObjectIndex];
    }

    public Vector3 GetPreviewObjectPosition()
    {
        return previewObject.transform.position;
    }

    public void StopShowingPreview()
    {
        cellIndicator.SetActive(false);
        Destroy(previewObject);
    }

    public void UpdatePosition(Vector3 position, bool validity)
    {
        if (previewObject != null)
        { 
            // MovePreview
            previewObject.transform.position = new Vector3(
                position.x + driectionPositionList[driectionObjectIndex].x,
                position.y + previewYOffset,
                position.z + driectionPositionList[driectionObjectIndex].z);

            previewObject.transform.eulerAngles = new Vector3(
                0,
                driectionRotationList[driectionObjectIndex].y,
                0);
        }


        // MoveCursor
        cellIndicator.transform.position = new Vector3(
            position.x + driectionPositionList[driectionObjectIndex].x,
            position.y,
            position.z + driectionPositionList[driectionObjectIndex].z);

        cellIndicator.transform.eulerAngles = new Vector3(
            0,
            driectionRotationList[driectionObjectIndex].y,
            0);

        // ApplyFeedback
        Color c = validity ? Color.white : Color.red;
        c.a = 0.5f;
        cellIndicatorRenderer.material.color = c;
        previewMaterialInstance.color = c;
    }

}
