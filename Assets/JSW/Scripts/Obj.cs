using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj : MonoBehaviour, IPunObservable
{
    public PhotonView pv;
    public WhiteBoard WhiteBoard { get; set; }
    public enum ObjKind
    {
        Square,
        Image
    }
    public ObjKind objKind;

    public enum ObjState
    {
        Idle,
        Ghost
    }
    public ObjState objState;
    Dictionary<string, bool> enables = new Dictionary<string, bool>();
    private void Awake()
    {
        enables["SetScale"] = true;
        enables["SetPos"] = true;
    }
    // �ʱ�ȭ
    public virtual void RPC_Init(Color c = new Color(), byte[] imgData = null)
    {
        pv.RPC(nameof(Init), RpcTarget.AllBuffered, new float[] {c.r, c.g, c.b, c.a}, imgData);
    }
    [PunRPC]
    public void Init(float[] color, byte[] imgData)
    {
        WhiteBoard = GameObject.Find("WhiteBoard(Clone)").GetComponent<WhiteBoard>();
        InitVirtual(color, imgData);
    }
    public virtual void InitVirtual(float[] color, byte[] imgData)
    {
        transform.SetParent(WhiteBoard.transform.Find("Objects"));
        ChangeObjState(ObjState.Ghost);
    }
    // ��ġ
    public void RPC_Place()
    {
        pv.RPC(nameof(Place), RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void Place()
    {
        WhiteBoard.Add(gameObject);
        ChangeObjState(ObjState.Idle);
    }
    // ���� ��ȭ (��Ʈ/��ġ)
    public virtual void ChangeObjState(ObjState s)
    {
        
    }
    // ��ġ ����
    public void RPC_SetPos(Vector3 pos, bool final = false)
    {
        if (enables["SetPos"] || final)
        {
            pv.RPC(nameof(SetPos), RpcTarget.Others, pos);
            if (gameObject.activeSelf) StartCoroutine(Timer("SetPos"));
        }
    }
    [PunRPC]
    public void SetPos(Vector3 pos)
    {
        transform.position = pos;
    }
    // ũ��
    public virtual Vector3 GetScale()
    {
        return transform.localScale;
    }
    // ũ�� ����
    public void RPC_SetScale(Vector3 scale, bool final = false)
    {
        if (enables["SetScale"] || final)
        {
            pv.RPC(nameof(CallSetScale), RpcTarget.Others, scale);
            StartCoroutine(Timer("SetScale"));
        }
    }
    [PunRPC]
    public void CallSetScale(Vector3 scale)
    {
        SetScale(scale);
    }
    public virtual void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }
    // ����
    public void RPC_Destroy()
    {
        pv.RPC(nameof(Destroy), RpcTarget.All);
    }
    [PunRPC]
    public void Destroy()
    {
        WhiteBoard.Remove(gameObject);
        if (pv.IsMine) PhotonNetwork.Destroy(gameObject);
    }
    // SetActive
    public void RPC_SetActive(bool value)
    {
        enables["SetPos"] = true;
        pv.RPC(nameof(SetActive), RpcTarget.OthersBuffered, value);
    }
    [PunRPC]
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
    // �� ��/�� �� ������
    public void RPC_MoveFrontOrBack(bool value)
    {
        pv.RPC(nameof(MoveFrontOrBack), RpcTarget.All, value);
    }
    [PunRPC]
    public void MoveFrontOrBack(bool value)
    {
        WhiteBoard.MoveFrontOrBack(gameObject, value);
    }
    // ����ȭ �ʴ� 30ȸ ����
    IEnumerator Timer(string method)
    {
        enables[method] = false;
        yield return new WaitForSeconds(0.033333f);
        enables[method] = true;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
