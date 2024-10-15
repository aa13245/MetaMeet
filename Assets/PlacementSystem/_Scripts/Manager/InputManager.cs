using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;

    private Vector3 lastPosition; // ������ ������

    [SerializeField] LayerMask placementLayermask; // ��ġ ���� ���̾� 

    public event Action OnClicked, OnExit; // Ŭ�� �̺�Ʈ , ��� �̺�Ʈ

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Ŭ����
        { 
            OnClicked?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) // ��� �̺�Ʈ
        {
            OnExit?.Invoke();
        }
    }

    // ������ ��ġ�� UI��Ұ� �ִ��� Ȯ��
    public bool IsPointerOverUI() => 
        EventSystem.current.IsPointerOverGameObject();

    // ���콺 ������
    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, placementLayermask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }
}
