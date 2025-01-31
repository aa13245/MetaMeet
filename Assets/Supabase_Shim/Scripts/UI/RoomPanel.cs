using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour
{
    public TMP_Text[] roomTexts = new TMP_Text[3];
    public UnityEngine.UI.Button btn_join;

    public void SetRoomInfo(RoomInfo room)
    {
        roomTexts[0].text = room.Name;
        roomTexts[1].text = $"({room.PlayerCount}/{room.MaxPlayers})";
        string masterName = room.CustomProperties["MASTER_NAME"].ToString();
        roomTexts[2].text = masterName;
    }
}
