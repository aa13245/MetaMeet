using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Select_JSW : MonoBehaviour
{
    public int viewId;
    public Obj_JSW obj;
    public Device_JSW device;
    public string nickname;
    public RectTransform rt;
    public RectTransform namePanel;
    public TMP_Text nameText;
    public void OnChanged()
    {
        nameText.text = nickname;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        if (obj == null)
        {
            Destroy(gameObject);
            return;
        }
        // 크기
        Vector3 objScale = obj.GetScale();
        rt.sizeDelta = new Vector2(objScale.x * 112.5f + 1, objScale.y * 112.5f + 1) / device.cam.orthographicSize;
        // 위치
        float camHeight = device.cam.orthographicSize * 2.0f;
        float camWidth = camHeight * device.cam.aspect;
        Vector2 posRatio = Vector2.zero;
        posRatio.x = (obj.transform.position.x - (device.cam.transform.position.x - camWidth / 2)) / ((device.cam.transform.position.x + camWidth / 2) - (device.cam.transform.position.x - camWidth / 2));
        posRatio.y = (obj.transform.position.y - (device.cam.transform.position.y - camHeight / 2)) / ((device.cam.transform.position.y + camHeight / 2) - (device.cam.transform.position.y - camHeight / 2));
        rt.localPosition = new Vector2(posRatio.x - 0.5f, posRatio.y - 0.5f);
        namePanel.localPosition = new Vector2(posRatio.x - 0.5f - rt.sizeDelta.x / 800, posRatio.y - 0.49f + rt.sizeDelta.y / 450);
    }
}
