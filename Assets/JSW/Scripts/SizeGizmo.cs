using UnityEngine;

public class SizeGizmo : MonoBehaviour, IInteract
{
    public Display display;
    public Device.Horizontal horizontal;
    public Device.Vertical vertical;

    // 오브젝트 사이즈 조절 기즈모 조작
    public void Interact(Vector3 pos, KeyCode keyCode, IInteract.KeyState keyState, float value = 0)
    {
        if (keyCode == KeyCode.Mouse0)
        {
            Vector3 localPos = display.transform.InverseTransformPoint(pos); // 디스플레이 터치 지점 로컬좌표
            Vector2 touchPos = display.Convert2Board(localPos); // 카메라상 변환 좌표

            display.device.SetSize(touchPos.x, touchPos.y, keyCode, keyState, this);
        }
    }
}
