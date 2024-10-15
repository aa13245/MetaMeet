using UnityEngine;

public class SizeGizmo : MonoBehaviour, IInteract
{
    public Display display;
    public Device.Horizontal horizontal;
    public Device.Vertical vertical;

    // ������Ʈ ������ ���� ����� ����
    public void Interact(Vector3 pos, KeyCode keyCode, IInteract.KeyState keyState, float value = 0)
    {
        if (keyCode == KeyCode.Mouse0)
        {
            Vector3 localPos = display.transform.InverseTransformPoint(pos); // ���÷��� ��ġ ���� ������ǥ
            Vector2 touchPos = display.Convert2Board(localPos); // ī�޶�� ��ȯ ��ǥ

            display.device.SetSize(touchPos.x, touchPos.y, keyCode, keyState, this);
        }
    }
}
