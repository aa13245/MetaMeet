using TMPro;
using UnityEngine;

public class Select : MonoBehaviour
{
    public int viewId;
    public Obj obj;
    public Device device;
    public string nickname;
    public RectTransform rt;
    public RectTransform namePanel;
    public TMP_Text nameText;
    public void OnChanged()
    {
        nameText.text = nickname;
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
        rt.sizeDelta = new Vector2(objScale.x * 112.5f + 1, objScale.y * 112.5f + 1) / device.Cam.orthographicSize;
        // 디스플레이 상 위치 - (카메라 모서리부터의 거리 / 카메라 범위) 비율
        float camHeight = device.Cam.orthographicSize * 2.0f;
        float camWidth = camHeight * device.Cam.aspect;
        Vector2 posRatio = Vector2.zero;
        posRatio.x = (obj.transform.position.x - (device.Cam.transform.position.x - camWidth / 2)) / ((device.Cam.transform.position.x + camWidth / 2) - (device.Cam.transform.position.x - camWidth / 2));
        posRatio.y = (obj.transform.position.y - (device.Cam.transform.position.y - camHeight / 2)) / ((device.Cam.transform.position.y + camHeight / 2) - (device.Cam.transform.position.y - camHeight / 2));

        rt.localPosition = new Vector2(posRatio.x - 0.5f, posRatio.y - 0.5f);
        namePanel.localPosition = new Vector2(posRatio.x - 0.5f - rt.sizeDelta.x / 800, posRatio.y - 0.49f + rt.sizeDelta.y / 450);
    }
}
