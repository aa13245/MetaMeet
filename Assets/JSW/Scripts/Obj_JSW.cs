using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Obj_JSW : MonoBehaviour, IPunObservable
{
    public PhotonView pv;
    public WhiteBoard_JSW whiteBoard { get; set; }
    public enum ObjKind
    {
        Square,
        Image
    }
    public ObjKind objKind;

    public enum ObjState
    {
        Idle,
        Ghost,

    }
    public ObjState objState;
    Dictionary<string, bool> enables = new Dictionary<string, bool>();
    private void Awake()
    {
        enables["SetScale"] = true;
        enables["SetPos"] = true;
    }
    public virtual void RPC_Init(Color c = new Color(), byte[] imgData = null)
    {
        pv.RPC(nameof(Init), RpcTarget.AllBuffered, new float[] {c.r, c.g, c.b, c.a}, imgData);
    }
    [PunRPC]
    public void Init(float[] color, byte[] imgData)
    {
        whiteBoard = GameObject.Find("WhiteBoard(Clone)").GetComponent<WhiteBoard_JSW>();
        InitVirtual(color, imgData);
    }
    public virtual void InitVirtual(float[] color, byte[] imgData)
    {
        transform.SetParent(whiteBoard.transform.Find("Objects"));
        ChangeObjState(ObjState.Ghost);
    }
    public void RPC_Place()
    {
        pv.RPC(nameof(Place), RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void Place()
    {
        whiteBoard.Add(gameObject);
        ChangeObjState(ObjState.Idle);
    }
    public virtual void ChangeObjState(ObjState s)
    {
        
    }
    public virtual Vector3 GetScale()
    {
        return transform.localScale;
    }
    public virtual void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }
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
    public void RPC_SetScale(Vector3 scale, bool final = false)
    {
        if (enables["SetScale"] || final)
        {
            pv.RPC(nameof(SetScale_), RpcTarget.Others, scale);
            StartCoroutine(Timer("SetScale"));
        }
    }
    [PunRPC]
    public void SetScale_(Vector3 scale)
    {
        SetScale(scale);
    }
    public void RPC_Destroy()
    {
        pv.RPC(nameof(Destroy), RpcTarget.All);
    }
    [PunRPC]
    public void Destroy()
    {
        whiteBoard.Remove(gameObject);
        if (pv.IsMine) PhotonNetwork.Destroy(gameObject);
    }
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
    public void RPC_MoveFrontOrBack(bool value)
    {
        pv.RPC(nameof(MoveFrontOrBack), RpcTarget.All, value);
    }
    [PunRPC]
    public void MoveFrontOrBack(bool value)
    {
        whiteBoard.MoveFrontOrBack(gameObject, value);
    }
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
