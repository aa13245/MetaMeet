using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;

    private Vector3 lastPosition; // 마지막 포지션

    [SerializeField] LayerMask placementLayermask; // 배치 가능 레이어 

    public event Action OnClicked, OnExit; // 클릭 이벤트 , 취소 이벤트

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 클릭후
        { 
            OnClicked?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) // 취소 이벤트
        {
            OnExit?.Invoke();
        }
    }

    // 포인터 위치에 UI요소가 있는지 확인
    public bool IsPointerOverUI() => 
        EventSystem.current.IsPointerOverGameObject();

    // 마우스 포지션
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
