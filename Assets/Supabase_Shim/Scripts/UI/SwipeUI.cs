using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SwipeUI : MonoBehaviour
{
    [SerializeField]
    private Scrollbar scrollBar;                    // Scrollbar�� ��ġ�� �������� ���� ������ �˻�

    private float[] scrollPageValues;           // �� �������� ��ġ �� [0.0 - 1.0]
    private float valueDistance = 0;            // �� ������ ������ �Ÿ�
    public int currentPage = 0;            // ���� ������
    private int maxPage = 0;                // �ִ� ������

    private void Awake()
    {
        // ��ũ�� �Ǵ� �������� �� value ���� �����ϴ� �迭 �޸� �Ҵ�
        scrollPageValues = new float[transform.childCount];

        // ��ũ�� �Ǵ� ������ ������ �Ÿ�
        valueDistance = 1f / (scrollPageValues.Length - 1f);

        // ��ũ�� �Ǵ� �������� �� value ��ġ ���� [0 <= value <= 1]
        for (int i = 0; i < scrollPageValues.Length; ++i)
        {
            scrollPageValues[i] = valueDistance * i;
        }

        // �ִ� �������� ��
        maxPage = transform.childCount;
    }

    public void SetScrollBarValueDown()
    {
        if (0 <= currentPage - 1)
        {
            SetScrollBarValue(currentPage - 1);
        }
    }

    public void SetScrollBarValueUp()
    {
        if (maxPage > currentPage + 1)
        {
            SetScrollBarValue(currentPage + 1);
        }
    }

    private void Start()
    {
        SetScrollBarValue(0);
    }

    public void SetScrollBarValue(int index)
    {
        currentPage = index;
        scrollBar.value = scrollPageValues[index];
    }

}
