using Photon.Pun;
using UnityEngine;

public class SpeakerInit : MonoBehaviour, IPunObservable
{ 
    public void RPC_Init(int viewId)
    {
        GetComponent<PhotonView>().RPC(nameof(Init), RpcTarget.AllBuffered, viewId);
    }
    [PunRPC]
    public void Init(int viewId)
    {
        Transform player = PhotonView.Find(viewId).transform;
        gameObject.transform.SetParent(player);
        transform.position = player.position;
        if (!GetComponent<PhotonView>().IsMine) GameObject.Find("WhiteBoard(Clone)").GetComponent<WhiteBoard>().audioSources.Add(GetComponent<AudioSource>());
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
    private void OnDestroy()
    {
        if (!GetComponent<PhotonView>().IsMine) GameObject.Find("WhiteBoard(Clone)").GetComponent<WhiteBoard>().audioSources.Remove(GetComponent<AudioSource>());
    }
}
