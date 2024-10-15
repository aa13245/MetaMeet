using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour, IPunObservable
{
    public PhotonView pv;
    public Camera cam;
    public Device Device { get; set; }
    WhiteBoard wb;
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
    {   // 생성 통신 대기
        yield return new WaitUntil(() => GameObject.Find("WhiteBoard(Clone)") != null);
        yield return new WaitUntil(() => wb.displays.Count != 0);
        // 디스플레이 매칭
        transform.parent = wb.transform.Find("Cameras");
        gameObject.name = idx.ToString();
        wb.displays[idx].GetComponent<Display>().InitNotMaster(gameObject);
        Device = wb.displays[idx].GetComponentInParent<Device>();
    }
    Dictionary<string, bool> enables = new Dictionary<string, bool>();
    private void Start()
    {
        wb = GameObject.Find("WhiteBoard(Clone)").GetComponent<WhiteBoard>();
        enables["SetPos"] = true;
    }
    // 카메라 줌 조절
    public void RPC_SetZoom(float value)
    {
        pv.RPC(nameof(SetZoom), RpcTarget.Others, value);
    }
    [PunRPC]
    public void SetZoom(float value)
    {
        cam.orthographicSize = value;
    }
    // 카메라 위치 조절
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
    // 공유 모드 On
    public void RPC_Share()
    {
        pv.RPC(nameof(Share), RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void Share()
    {
        if (wb.Sharer == -1) // 아무도 공유중이 아닐 때만 가능
        {
            wb.Sharer = pv.ViewID; 
            foreach (Display each in wb.displays)
            {
                each.device.Share(pv.ViewID);
            }
        }
    }
    // 팔로잉 모드 On
    public void RPC_Following()
    {
        pv.RPC(nameof(Following), RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void Following()
    {
        if (wb.Sharer != -1) // 누군가 공유중일 때만 가능
        {
            Device.Following(true);
        }
    }
    // 공유/팔로잉 Off
    public void RPC_Cancel()
    {
        pv.RPC(nameof(Cancel), RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void Cancel()
    {
        // 공유 종료
        if (wb.Sharer == pv.ViewID)
        {
            wb.Sharer = -1;
            foreach (Display each in wb.displays)
            {
                each.device.Share(-1);
            }
        }
        // 팔로잉 종료
        else
        {
            Device.Following(false);
        }
    }
    // 타 유저 선택 UI 동기화
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
    // 동기화 초당 30회 제한
    IEnumerator Timer(string method)
    {
        enables[method] = false;
        yield return new WaitForSeconds(0.033333f);
        enables[method] = true;
    }
}
