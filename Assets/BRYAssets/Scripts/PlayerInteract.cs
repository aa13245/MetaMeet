using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInteract : MonoBehaviourPunCallbacks
{
    public bool isSitting = false;
    public Transform model;
    public GameObject sitPanel;
    PlayerMove player;
    CharacterController cc;
    Animator anim;
    // �߰�: CameraController ���� ����
    private CameraController cameraController;

    public Quaternion originalRotation { get; set; }

    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionPointRadius = 0.5f;
    [SerializeField] private LayerMask interactableMask;

    private Collider[] colliders = new Collider[3];
    [SerializeField] private int numFound;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        player = GetComponent<PlayerMove>();
        
    }
    private void Start()
    {
        GameObject canvas = GameObject.Find("Canvas(Clone)");
        sitPanel = canvas.transform.Find("SitPanel").gameObject;
        // �߰�: CameraController ������Ʈ ��������
        cameraController = Camera.main.GetComponent<CameraController>();

        //������ �ƴ� ��� ��ũ��Ʈ ��Ȱ��ȭ
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return; //���� �÷��̾ ������Ʈ

        InteractionCheck();
        InputCheck();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionPointRadius);
    }

    private void InputCheck()
    {
        originalRotation = transform.rotation;

        if (Input.GetKeyDown(KeyCode.LeftControl) && numFound > 0)
        {
            var chair = colliders[0].GetComponent<Chair>();
            if (chair != null)
            {
                chair.InteractWithObjects(this);
            }
        }
    }

    private void InteractionCheck()
    {
        if (isSitting)
        {
            sitPanel.SetActive(false);
            return;
        }

        numFound = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionPointRadius, colliders, interactableMask);

        if (numFound > 0)
        {
            var interactable = colliders[0].GetComponent<IInteractable>();

            if (interactable != null)
            {
                sitPanel.SetActive(true);
                return;
            }
        }
        else
        {
            sitPanel.SetActive(false);
        }
    }

    public void SitOnChair(Transform sitPosition)
    {
        if (!photonView.IsMine) return;

        isSitting = true;
        cc.enabled = false;
        player.useRotX = false;
        transform.position = sitPosition.position;
        transform.rotation = sitPosition.rotation;
        SetSittingAnimation(true);

        // �߰�: ī�޶� �ɱ� ���� ����
        cameraController.SetPlayerSitting(true, sitPosition.rotation);
    }

    public void StandUp()
    {
        if (!photonView.IsMine) return;

        isSitting = false;
        cc.enabled = true;
        player.useRotX = true;
        SetSittingAnimation(false);

        // �߰�: ī�޶� ���� ���� ����
        cameraController.SetPlayerSitting(false, originalRotation);
    }

    [PunRPC]
    private void SyncSitOnChair(Vector3 position, Quaternion rotation)
    {
        isSitting = true;
        cc.enabled = false;
        //player.useRotX = false;
        transform.position = position;
        transform.rotation = rotation;
        SetSittingAnimation(true);
    }

    [PunRPC]
    private void SyncStandUp()
    {
        isSitting = false;
        cc.enabled = true;
        player.useRotX = true;
        //model.rotation = Quaternion.identity;
        SetSittingAnimation(false);
        //model.localEulerAngles = Vector3.zero;
    }

    // �߰�: �ִϸ��̼� ���� ���� �޼���
    private void SetSittingAnimation(bool isSitting)
    {
        anim.SetBool("IsSitting", isSitting);
    }
}