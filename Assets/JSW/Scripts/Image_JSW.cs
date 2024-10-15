using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UnityEngine;

public class Image_JSW : Obj_JSW, IPunObservable
{
    public override void ChangeObjState(ObjState s)
    {
        if (objState == s) return;
        objState = s;
        if (s == ObjState.Idle)
        {
            Material mat = transform.GetComponent<Renderer>().material;
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1);
        }
        else if (s == ObjState.Ghost)
        {
            Material mat = transform.GetComponent<Renderer>().material;
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.3f);
        }
    }
    byte[] data = null;
    public override void RPC_Init(Color c = default, byte[] imgData = null)
    {
        for (int i = 0; i < imgData.Length; i += 100)
        {
            int length = Mathf.Min(100, imgData.Length - i);
            byte[] subData = new byte[length];
            Array.Copy(imgData, i, subData, 0, length);
            pv.RPC(nameof(Init), RpcTarget.OthersBuffered, null, subData);
        }
        pv.RPC(nameof(Init), RpcTarget.OthersBuffered, null, null);
    }
    public override void InitVirtual(float[] color, byte[] imgData)
    {
        // 수신 시작
        if (data == null)
        {
            data = imgData;
        }
        // 수신 중
        else if (imgData != null)
        {
            data = data.Concat(imgData).ToArray();
        }
        // 수신 완료
        else
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = new Material(renderer.material);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(data))
            {
                renderer.material.mainTexture = texture;
                SetScale(new Vector3(Mathf.Max(0.5f, texture.width / 200), Mathf.Max(0.5f, texture.height / 200), 1));
            }
            data = null;
            base.InitVirtual(color, null);
        }
    }
}
