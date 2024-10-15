using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam_JSW : MonoBehaviour, IPunObservable
{
    public PhotonView pv;
    public Camera cam;
    public Device_JSW device { get; set; }
    WhiteBoard_JSW wb;
    public void RPC_Init(int idx)
    {
        pv.RPC(nameof(Init), RpcTarget.OthersBuffered, idx);
    }
    [PunRPC]
    public void Init(int idx)
    {
        StartCoroutine(InitC(idx));
    }
    IEnumerator InitC(int idx)
    {
        yield return new WaitUntil(() => GameObject.Find("WhiteBoard(Clone)") != null);
        yield return new WaitUntil(() => wb.displays.Count != 0);
        transform.parent = wb.transform.Find("Cameras");
        gameObject.name = idx.ToString();
        wb.displays[idx].GetComponent<Display_JSW>().InitNotMaster(gameObject);
        device = wb.displays[idx].GetComponentInParent<Device_JSW>();
    }
    Dictionary<string, bool> enables = new Dictionary<string, bool>();
    private void Start()
    {
        wb = GameObject.Find("WhiteBoard(Clone)").GetComponent<WhiteBoard_JSW>();
        enables["SetPos"] = true;
    }
    public void RPC_SetZoom(float value)
    {
        pv.RPC(nameof(SetZoom), RpcTarget.Others, value);
    }
    [PunRPC]
    public void SetZoom(float value)
    {
        cam.orthographicSize = value;
    }
    public void RPC_SetPos(Vector3 pos, bool final = false)
    {
        if (enables["SetPos"] || final)
        {
            pv.RPC(nameof(SetPos), RpcTarget.Others, pos);
            StartCoroutine(Timer("SetPos"));
        }
    }
    [PunRPC]
    public void SetPos(Vector3 pos)
    {
        transform.position = pos;
    }
    public void RPC_Share()
    {
        pv.RPC(nameof(Share), RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void Share()
    {
        if (wb.sharer == -1) // 아무도 공유중이 아닐 때만 가능
        {
            wb.sharer = pv.ViewID; 
            foreach (Display_JSW each in wb.displays)
            {
                each.device.Share(pv.ViewID);
            }
        }
    }
    public void RPC_Following()
    {
        pv.RPC(nameof(Following), RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void Following()
    {
        if (wb.sharer != -1) // 누군가 공유중일 때만 가능
        {
            device.Following(true);
        }
    }
    public void RPC_Cancel()
    {
        pv.RPC(nameof(Cancel), RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void Cancel()
    {
        // 공유 종료
        if (wb.sharer == pv.ViewID)
        {
            wb.sharer = -1;
            foreach (Display_JSW each in wb.displays)
            {
                each.device.Share(-1);
            }
        }
        // 팔로잉 종료
        else
        {
            device.Following(false);
        }
    }

    IEnumerator Timer(string method)
    {
        enables[method] = false;
        yield return new WaitForSeconds(0.033333f);
        enables[method] = true;
    }
    public void RPC_OtherSelect(int idx, int viewId, string nickname)
    {
        pv.RPC(nameof(OtherSelect), RpcTarget.All, idx, viewId, nickname);
    }
    [PunRPC]
    public void OtherSelect(int idx, int viewId, string nickname)
    {
        wb.OtherSelect(idx, viewId, nickname);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
