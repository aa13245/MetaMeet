using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [Header("������Ʈ ���� ���� ����")]
    [SerializeField] private float previewYOffset = 0.06f;
    [Header("�ε������")]
    [SerializeField] private GameObject cellIndicator;

    [Header("������ ������Ʈ")]
    [SerializeField] private GameObject previewObject;

    [Header("������ ������Ʈ ��Ƽ����")]
    [SerializeField] private Material previewMaterialPrefab;


    [Header("������ ������Ʈ ��Ƽ���� ����")]
    [SerializeField] private Material previewMaterialInstance;

    private Renderer cellIndicatorRenderer;

    [Header("���� ������Ʈ �ε���")]
    [SerializeField] private int driectionObjectIndex = 0;

    [Header("4���� ȸ�� Y�� ���ʹϾ� ����Ʈ")]
    [SerializeField]
    private Vector3[] driectionRotationList = new Vector3[4] {
        new Vector3(0, 0, 0),
        new Vector3(0, 90f, 0),
        new Vector3(0, 180f, 0),
        new Vector3(0, 270f, 0),
    };

    [Header("4���� ȸ�� ������ ����Ʈ")]
    [SerializeField]
    private Vector3Int[] driectionPositionList = new Vector3Int[4] {
        Vector3Int.zero,
        Vector3Int.zero,
        Vector3Int.zero,
        Vector3Int.zero,
    };

    [Header("���� ������")] // ������ ����ɶ� ���� ����� �����
    public Vector2Int dynamicObjectSize = new Vector2Int();

    // ������ ����Ǹ� ���� �Ǿ� �ϴ� ������ ���� 
    public void SetDriectionData(int drectionIndex, Vector2Int size)
    {
        driectionObjectIndex = drectionIndex;

        driectionPositionList[0] = new Vector3Int(0, 0, 0);
        driectionPositionList[1] = new Vector3Int(0, 0, size.y);
        driectionPositionList[2] = new Vector3Int(size.x, 0, size.y);
        driectionPositionList[3] = new Vector3Int(size.y, 0, size.y - size.x);

        SetDynamicObjectSize(size);
    }

    // ������ ����ʿ� ���� �����ؾ� �ϴ� ������Ʈ�� ������ 
    public void SetDynamicObjectSize(Vector2Int size)
    {
        // ����� ���� ��쿣 ó�� �� �ʿ䰡 ���� 
        if (size.x == size.y)
        {
            dynamicObjectSize = size;
        }
        // ����� �ٸ� ��쿡 ���� ȸ���ÿ� 
        // ��ǥ�� �ٲ�Ƿ� ó�� �Ѵ�.
        else
        {
            // �� �Ʒ�
            if (driectionObjectIndex == 0 || driectionObjectIndex == 2)
            {
                dynamicObjectSize = new Vector2Int(size.x, size.y);
            }
            // ������ ����
            else if (driectionObjectIndex == 1 || driectionObjectIndex == 3)
            {
                dynamicObjectSize = new Vector2Int(size.y, size.x);
            }
        }



    }

    private void Start()
    {
        // ���͸��� 
        previewMaterialInstance = new Material(previewMaterialPrefab);
        // �ε������� ���� 
        cellIndicator.SetActive(false);
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }


    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size)
    {
        // ������Ʈ ����
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
