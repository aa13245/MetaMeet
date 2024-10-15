using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt;
    public string InteractionPrompt => prompt;

    public Transform sitPosition;
    public bool isOccupied = false;

    public void InteractWithObjects(PlayerInteract interactor)
    {
        if (interactor.photonView.IsMine)
        {
            if (!isOccupied && !interactor.isSitting)
            {
                // 수정: sitPosition의 정보를 전달
                interactor.SitOnChair(sitPosition);
                interactor.photonView.RPC("SyncSitOnChair", RpcTarget.OthersBuffered, sitPosition.position, sitPosition.rotation);
                isOccupied = true;
            }
            else if (isOccupied && interactor.isSitting)
            {
                interactor.StartCoroutine("StandUp");
                interactor.photonView.RPC("SyncStandUp", RpcTarget.OthersBuffered);
                isOccupied = false;
            }
        }
    }
 

    //public IEnumerator SitRoutine(PlayerInteract interactor)
    //{
    //    isOccupied = true;
    //    interactor.SitOnChair(sitPosition);
    //    yield return new WaitForSeconds(sitAnimationDuration);
    //}

    //public IEnumerator StandRoutine(PlayerInteract interactor)
    //{
    //    interactor.StandUp();
    //    yield return new WaitForSeconds(sitAnimationDuration);
    //    isOccupied = false;
    //    interactor.model.rotation = Quaternion.identity;
    //}

}