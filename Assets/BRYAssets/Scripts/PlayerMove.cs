using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using TMPro;
using UnityEngine;

public class PlayerMove : PlayerStateBase, IPunObservable
{
    public GameObject screenPanel;
    GameObject aimDot;
    CharacterController cc;
    Animator anim;
    CameraController cameraController;

    float yVelocity;
    float rotX;

    float moveInput = 0;
    Vector3 dir;


    //상태 변수
    bool isRunning = false;
    public bool isViewing { get; private set; }

    int layerMask;

    private PhotonView photonView;
    public TextMeshProUGUI nickName;
    public GameObject nickNamePanel;

    //애니메이션 동기화 변수
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkMovement;
    private bool networkIsMoving;
    private bool networkIsRunning;
    private bool networkIsJumping;


    void Start()
    {


        // 추가: 네트워크 변수 초기화
        networkPosition = transform.position;
        networkRotation = transform.rotation;

        photonView = GetComponent<PhotonView>();

        if (photonView != null)
        {
            nickName.text = photonView.Owner.NickName;
        }

        anim = GetComponentInChildren<Animator>();
        cc = GetComponent<CharacterController>();
        //로컬플레이어만
        if(photonView.IsMine)
        {
            cameraController = Camera.main.GetComponent<CameraController>();
            cameraController.playerMove = this;
            moveSpeed = walkSpeed;
            StartCoroutine(CursorOff());
            SetupLocalPlayer();
        }
        layerMask = ~(1 << LayerMask.NameToLayer("Player"));
    }

    void SetupLocalPlayer()
    {
        cameraController.target = gameObject.transform;

        GameObject canvas = GameObject.Find("Canvas(Clone)");
        screenPanel = canvas.transform.Find("ScreenPanel").gameObject;
        aimDot = canvas.transform.Find("AimDot").gameObject;
    }

    void Update()
    {


        if (photonView.IsMine)
        {
            nickNamePanel.transform.forward = Camera.main.transform.forward;
            RunCheck();
            JumpCheck();
            ScreenCheck();
            Move();
            Rotate();
            MoveAnimation();
            Emote();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Slerp(transform.rotation, networkRotation, Time.deltaTime * 10);
            //원격플레이어 애니메이션 업데이트
            RemoteAnimationUpdate();
            nickNamePanel.transform.forward = Camera.main.transform.forward;
        }
    }

    void ScreenCheck()
    {
        //Transform lookpostrans = transform.GetChild(0);
        //눈 앞으로 레이를 쏴서 물체를 체크
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 2, layerMask))
        {
            //만약 스크린이면
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Interacter") && !isViewing)
            {
                screenPanel.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    isViewing = true;
                    aimDot.SetActive(false);
                    screenPanel.SetActive(false);
                    CursorOn();
                }
            }
            else if (isViewing)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    aimDot.SetActive(true);
                    StartCoroutine(CursorOff());
                    isViewing = false;
                }
            }
            else if (hitInfo.transform.gameObject.layer != LayerMask.NameToLayer("Interacter"))
            {
                screenPanel.SetActive(false);
            }
        }
    }

        void CursorOn()
        {
            //마우스 커서가 나오게
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            //마우스 인풋을 꺼서 화면이 안 돌아가게 하기?
            useRotX = false;
            cameraController.useRotY = false;
        }
        IEnumerator CursorOff()
        {
            yield return null;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            //카메라 다시 돌아감
            useRotX = true;
            cameraController.useRotY = true;
        }


        //달리기 상태 체크
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

        //점프 체크
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
                    //anim.SetTrigger("Jump");
                    photonView.RPC("JumpRPC", RpcTarget.All);
                    yVelocity = jumpPower;
                    jumpCurrCount++;
                }
            }
        }

        void MoveAnimation()
        {
            Vector3 localDir = transform.InverseTransformDirection(dir.normalized);

            anim.SetFloat("x", localDir.x);
            anim.SetFloat("y", localDir.z);

            // 이동 및 달리기 상태 설정
            networkIsMoving = moveInput > 0;
            networkIsRunning = isRunning;
            anim.SetBool("IsMoving", networkIsMoving);
            anim.SetBool("IsRunning", networkIsRunning);
        }

        void RemoteAnimationUpdate()
        {
            Vector3 localDir = transform.InverseTransformDirection(networkMovement.normalized);

            anim.SetFloat("x", localDir.x);
            anim.SetFloat("y", localDir.z);

            anim.SetBool("IsMoving", networkIsMoving);
            anim.SetBool("IsRunning", networkIsRunning);

            if(networkIsJumping)
            {
            anim.SetTrigger("Jump");

            }
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

            networkMovement = dir;
        }


        void Rotate()
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            if (useRotX) rotX += mx * rotSpeed * Time.deltaTime;
        //if (useRotY) rotY += my * rotSpeed * Time.deltaTime;

        //rotY = Mathf.Clamp(rotY, -60, 60);
            if (GetComponent<PlayerInteract>().isSitting == false)
            { 
                transform.localEulerAngles = new Vector3(0, rotX, 0);
            }

        }

    [PunRPC]
    void JumpRPC()
    {
        anim.SetTrigger("Jump");
    }

    [PunRPC]
    void EmoteRPC(int emoteIndex)
    {
        switch (emoteIndex)
        {
            case 1:
                anim.SetTrigger("Wave");
                break;
            case 2:
                anim.SetTrigger("Bow");
                break;
            case 3:
                anim.SetTrigger("Clap");
                break;
            case 4:
                anim.SetTrigger("RaiseHand");
                break;
        }
    }

    void Emote()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            photonView.RPC("EmoteRPC", RpcTarget.All, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            photonView.RPC("EmoteRPC", RpcTarget.All, 2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            photonView.RPC("EmoteRPC", RpcTarget.All, 3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            photonView.RPC("EmoteRPC", RpcTarget.All, 4);
        }
    }

    //void Emote()
    //    {
    //        if (Input.GetKeyDown(KeyCode.Alpha1))
    //        {
    //            //손흔들기 
    //            anim.SetTrigger("Wave");
    //        }
    //        if (Input.GetKeyDown(KeyCode.Alpha2))
    //        {
    //            //꾸벅 인사
    //            anim.SetTrigger("Bow");
    //        }
    //        if (Input.GetKeyDown(KeyCode.Alpha3))
    //        {
    //            //박수
    //            anim.SetTrigger("Clap");
    //        }
    //        if (Input.GetKeyDown(KeyCode.Alpha4))
    //        {
    //            //질문
    //            anim.SetTrigger("RaiseHand");
    //        }
    //    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 로컬 플레이어: 다른 클라이언트에게 데이터 전송
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(dir); // 이동 방향 전송
            stream.SendNext(networkIsMoving);
            stream.SendNext(networkIsRunning);
            stream.SendNext(anim.GetBool("Jump"));
            stream.SendNext(isViewing);
        }
        else
        {
            //원격 플레이어 데이터 수신
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkMovement = (Vector3)stream.ReceiveNext();
            networkIsMoving = (bool)stream.ReceiveNext();
            networkIsRunning = (bool)stream.ReceiveNext();
            networkIsJumping = (bool)stream.ReceiveNext();
            isViewing = (bool)stream.ReceiveNext();
        }
    }
}

