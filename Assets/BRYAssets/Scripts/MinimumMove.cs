using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;

public class MinimumMove : PlayerStateBase
{
    
    public GameObject sitPanel;
    CharacterController cc;
    Animator anim;
    CameraController cameraController;

    float yVelocity;
    float rotX;


    float moveInput = 0;
    Vector3 dir;

    public float sittableDist = 2f;

    //���� ����
    bool isRunning = false;
    bool isSitting = false;
    public bool isViewing { get; private set; }
    bool canInteract = false;

    int layerMask;
    


    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        cc = GetComponent<CharacterController>();
        
        moveSpeed = walkSpeed;
        StartCoroutine(CursorOff());
        layerMask = ~(1 << LayerMask.NameToLayer("Player"));
       
        
    }

    void Update()
    {

        RunCheck();
        JumpCheck();
        Move();
        Rotate();
        MoveAnimation();
        Emote();
    }

    
    void CursorOn()
    {
        //���콺 Ŀ���� ������
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        //���콺 ��ǲ�� ���� ȭ���� �� ���ư��� �ϱ�?
        useRotX = false;
        cameraController.useRotY = false;
    }
    IEnumerator CursorOff()
    {
        yield return null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //ī�޶� �ٽ� ���ư�
        useRotX = true;
       
    }


    //�޸��� ���� üũ
    void RunCheck()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) //Running();
        {
            isRunning = true;
            moveSpeed = runSpeed;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift)) //RunningCancel();
        {
            isRunning = false;
            moveSpeed = walkSpeed;
        }
    }

    //���� üũ
    void JumpCheck()
    {
        if (cc.isGrounded)
        {
            yVelocity = 0;
            jumpCurrCount = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCurrCount < jumpMaxCount)
            {
                anim.SetTrigger("Jump");
                yVelocity = jumpPower;
                jumpCurrCount++;
            }
        }
    }


    void MoveAnimation()
    {
        Vector3 localDir = transform.InverseTransformDirection(dir.normalized);

        // ���� Ʈ�� �Ķ���� ����
        anim.SetFloat("x", localDir.x);
        anim.SetFloat("y", localDir.z);

        // �̵� �� �޸��� ���� ����
        anim.SetBool("IsMoving", moveInput > 0);
        anim.SetBool("IsRunning", isRunning);
    }

    void Move()
    {
        if (isViewing) { return; }
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveInput = Mathf.Abs(h) + Mathf.Abs(v);

        Vector3 dirH = transform.right * h;
        Vector3 dirV = transform.forward * v;
        dir = dirH + dirV;
        dir.Normalize();

        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        cc.Move(dir * moveSpeed * Time.deltaTime);
    }


    void Rotate()
    {
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        if (useRotX) rotX += mx * rotSpeed * Time.deltaTime;
        //if (useRotY) rotY += my * rotSpeed * Time.deltaTime;

        //rotY = Mathf.Clamp(rotY, -60, 60);

        transform.localEulerAngles = new Vector3(0, rotX, 0);

    }

    void Emote()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //������ 
            anim.SetTrigger("Wave");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //�ٹ� �λ�
            anim.SetTrigger("Bow");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //�ڼ�
            anim.SetTrigger("Clap");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            //����
            anim.SetTrigger("RaiseHand");
        }
    }


    //void TryInteractWithChair()
    //{
    //    Collider[] colliders = Physics.OverlapSphere(transform.position, interactionDistance);
    //    foreach (Collider collider in colliders)
    //    {
    //        InteractableChair chair = collider.GetComponent<InteractableChair>();
    //        if (chair != null)
    //        {
    //            canInteract = true;
    //            nearbyChair = chair;
    //            sitPanel.SetActive(true);
    //            return;
    //        }
    //    }
    //    canInteract = false;
    //    nearbyChair = null;
    //    sitPanel.SetActive(false);
    //}

    //���� ���ͷ��� üũ
    //void SitCheck()
    //{
    //    if (canInteract && Input.GetKeyDown(KeyCode.E))
    //    {
    //        if (isSitting)
    //        {
    //            StandUp();
    //        }
    //        else
    //        {
    //            TryInteractWithChair();
    //        }
    //    }
    //}

    //void Interact()
    //{
    //    if (nearbyChair != null)
    //    {
    //        nearbyChair.SitDown(gameObject);
    //    }
    //}

    //public void SitOnChair(Transform sitPosition)
    //    {
    //        isSitting = true;
    //        cc.enabled = false;
    //        transform.position = sitPosition.position;
    //        transform.rotation = sitPosition.rotation;
    //    }

    //    public void StandUp()
    //    {
    //        isSitting = false;
    //        cc.enabled = true;
    //        // You might want to offset the position slightly to prevent clipping
    //        transform.position += transform.forward * 0.5f;
    //    }
}
