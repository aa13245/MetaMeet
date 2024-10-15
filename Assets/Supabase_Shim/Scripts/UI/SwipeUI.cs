using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SwipeUI : MonoBehaviour
{
    [SerializeField]
    private Scrollbar scrollBar;                    // Scrollbar의 위치를 바탕으로 현재 페이지 검사

    private float[] scrollPageValues;           // 각 페이지의 위치 값 [0.0 - 1.0]
    private float valueDistance = 0;            // 각 페이지 사이의 거리
    public int currentPage = 0;            // 현재 페이지
    private int maxPage = 0;                // 최대 페이지

    private void Awake()
    {
        // 스크롤 되는 페이지의 각 value 값을 저장하는 배열 메모리 할당
        scrollPageValues = new float[transform.childCount];

        // 스크롤 되는 페이지 사이의 거리
        valueDistance = 1f / (scrollPageValues.Length - 1f);

        // 스크롤 되는 페이지의 각 value 위치 설정 [0 <= value <= 1]
        for (int i = 0; i < scrollPageValues.Length; ++i)
        {
            scrollPageValues[i] = valueDistance * i;
        }

        // 최대 페이지의 수
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
