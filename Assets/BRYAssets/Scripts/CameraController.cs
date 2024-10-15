using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public PlayerMove playerMove;
    public float distance = 2f;
    public float height = 1.5f;
    public float lateralOffset = 0.5f;
    public float minDistance = 1f;
    public float maxDistance = 5f;
    public float zoomSpeed = 2f;
    public float smoothSpeed = 5f;
    public float lookAtHeight = 1.7f; // Approximate eye level
    public float firstPersonThreshold = 1.2f;

    public float rotationLerpSpeed = 10f;

    private Vector3 currentVelocity;

    float rotationY;
    public bool useRotY;
    public float rotSpeed = 250;

    int playerLayerMask;

    // �߰�: �÷��̾� �ɱ� ���� �� ȸ�� ���� ����
    private bool isPlayerSitting = false;
    private Quaternion sittingRotation;



    private void Start()
    {
        InitializePlayerLayerMask();
    }
    void Update()
    {
        if (target != null && !playerMove.isViewing)
        {
            distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            YRotate();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetPosition = target.position + Vector3.up * height;

        // ����: �÷��̾� �ɱ� ���¿� ���� ȸ�� ó��
        float playerRotationY = isPlayerSitting ? sittingRotation.eulerAngles.y : target.eulerAngles.y;


        // X�� ȸ��(���� ����)�� �÷��̾��� Y�� ȸ���� ����
        Quaternion rotation = Quaternion.Euler(rotationY, playerRotationY, 0);

        // ����: �ɾ����� ���� ȸ�� ������ ���� ����
        if (!isPlayerSitting)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationLerpSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = rotation;
        }


        if (distance > firstPersonThreshold)
        {
            ShowPlayer();

            // 3��Ī ��
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + targetPosition;
            position += rotation * Vector3.right * lateralOffset;

            transform.rotation = rotation;
            transform.position = Vector3.SmoothDamp(transform.position, position, ref currentVelocity, 1f / smoothSpeed);
        }
        else
        {
            HidePlayer();
            // 1��Ī ��
            transform.rotation = rotation;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / smoothSpeed);
        }

        //if (distance > firstPersonThreshold)
        //{
        //    ShowPlayerLayer();
        //    // Third-person view (over-the-shoulder)
        //    Vector3 targetPosition = target.position + target.up * height;
        //    Vector3 desiredPosition = targetPosition
        //        - target.forward * distance
        //        + target.right * lateralOffset;

        //    // Smoothly move the camera
        //    transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1 / smoothSpeed);

        //    // Calculate and set rotation
        //    Vector3 lookAtPosition = target.position + target.up * lookAtHeight;
        //    Vector3 lookDirection = lookAtPosition - transform.position;
        //    transform.rotation = Quaternion.LookRotation(lookDirection);
        //}
        //else
        //{
        //    HidePlayerLayer();
        //    // First-person view
        //    Vector3 firstPersonPosition = target.position + target.up * lookAtHeight;
        //    transform.position = Vector3.SmoothDamp(transform.position, firstPersonPosition, ref currentVelocity, 1 / smoothSpeed);
        //    transform.rotation = target.rotation;
        //}
    }



    // �߰�: �÷��̾� �ɱ�/���� ���� ���� �޼ҵ�
    public void SetPlayerSitting(bool sitting, Quaternion rotation)
    {
        isPlayerSitting = sitting;
        sittingRotation = rotation;
    }

    void InitializePlayerLayerMask()
    {
        playerLayerMask = 1 << LayerMask.NameToLayer("Player");
    }

    void HidePlayer()
    {
        //currentPos = target.GetChild(1).position;
        //target.GetChild(1).position = new Vector3(currentPos.x, -10, currentPos.z);
        //target.GetChild(1).gameObject.SetActive(false);
        Camera.main.cullingMask &= ~playerLayerMask;
    }

    void ShowPlayer()
    {
        //currentPos = target.GetChild(1).position;
        //target.GetChild(1).position = new Vector3(currentPos.x, 0.1f, currentPos.z);
        //target.GetChild(1).gameObject.SetActive(true);
        Camera.main.cullingMask |= playerLayerMask;
    }

    void YRotate()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        if (useRotY) rotationY -= mouseY * rotSpeed * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -40f, 40f);
    }
}