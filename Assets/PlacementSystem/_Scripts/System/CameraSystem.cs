using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [Header("ī�޶� ���콺 ȸ�� ����ġ Ű")]
    [SerializeField] private KeyCode rotationYn = KeyCode.Q;
    [SerializeField] private KeyCode movementYn = KeyCode.E;
    [SerializeField] private KeyCode movementUp = KeyCode.W;
    [SerializeField] private KeyCode movementDown = KeyCode.S;
    [SerializeField] private KeyCode movementLeft = KeyCode.A;
    [SerializeField] private KeyCode movementRight = KeyCode.D;

    [Space(20f)]
    [Header("ī�޶� ���콺 ȸ�� ����ġ")]
    [SerializeField] private bool _rotation;

    [Space(20f)]
    [Header("ī�޶� Ű���� �̵� ����ġ")]
    [SerializeField] private bool _movement;

    [Space(20f)]
    [Header("UI ����ġ")]
    [SerializeField] private bool _ui;
    [SerializeField] private GameObject _full;

    [Space(20f)]
    [Header("���콺 �̵� ȸ�� �ӵ�")]
    [SerializeField] private float _mouseSeeed;
    [SerializeField] private float _moveSeeed;

    private Vector3 _initPosition;
    private Vector3 _initRotation;


    private void Start()
    {
        _initPosition = transform.position;
        _initRotation = transform.eulerAngles;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _ui = !_ui;
            _full.SetActive(_ui);
        }

        if (Input.GetKeyDown(rotationYn))
        {
            _rotation = !_rotation;
        }
        if (Input.GetKeyDown(movementYn))
        {
            _movement = !_movement;
        }

        if (_movement)
        {
            Vector3 deltaPosition = Vector3.zero;

            if (Input.GetKey(movementUp))
                deltaPosition += transform.forward;

            if (Input.GetKey(movementDown))
                deltaPosition -= transform.forward;

            if (Input.GetKey(movementLeft))
                deltaPosition -= transform.right;

            if (Input.GetKey(movementRight))
                deltaPosition += transform.right;

            transform.position += deltaPosition * Time.deltaTime * _moveSeeed;
        }

        if (_rotation)
        {
            // Pitch �� �Ʒ� 
            transform.rotation *= Quaternion.AngleAxis(
               -Input.GetAxis("Mouse Y") * _mouseSeeed,
               Vector3.right
           );

            // Paw �� ��
            transform.rotation = Quaternion.Euler(
                transform.eulerAngles.x,
                transform.eulerAngles.y + Input.GetAxis("Mouse X") * _mouseSeeed,
                transform.eulerAngles.z
            );
        }

    }
}
