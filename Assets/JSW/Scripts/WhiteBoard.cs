using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBoard : MonoBehaviour
{
    public int Sharer { get; set; }
    public Transform objsTf;
    List<GameObject> objs = new List<GameObject>();

    public int Idxcnt { get; set; }
    public List<Display> displays = new List<Display>();
    // 오브젝트 목록에 추가
    public void Add(GameObject obj)
    {
        objs.Add(obj);
        if (objs.Count == 1)
        {
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, objsTf.position.z);
        }
        else
        {
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, objs[^2].transform.position.z - 0.000001f);
        }
    }
    // 오브젝트 목록에서 제거
    public void Remove(GameObject obj)
    {
        objs.Remove(obj);
    }
    // 맨 앞/뒤로 보내기
    public void MoveFrontOrBack(GameObject obj, bool front)
    {
        if (objs.Count == 1) return;
        if (front)
        {
            objs.Remove(obj);
            objs.Add(obj);
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, objs[^2].transform.position.z - 0.000001f);
        }
        else
        {
            objs.Remove(obj);
            objs.Insert(0, obj);
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, objs[1].transform.position.z + 0.000001f);
        }
    }

    // 포톤 보이스 전체 녹음 기능
    public List<AudioSource> audioSources; // 녹음할 AudioSource 리스트
    private AudioClip recordedClip; // 녹음된 AudioClip
    private int sampleRate = 44100; // 샘플링 주파수
    private bool isRecording = false; // 녹음 상태
    private List<float> recordedData; // 녹음된 데이터 저장

    void Start()
    {
        Sharer = -1;
        audioSources = new List<AudioSource>(FindObjectsOfType<AudioSource>());
        recordedData = new List<float>();
    }
    public void OtherSelect(int idx, int viewId, string nickname)
    {
        foreach (Display display in displays)
        {
            display.OtherSelect(idx, viewId, nickname);
        }
    }
    public void StartRecording()
    {
        if (!isRecording)
        {
            recordedData.Clear(); // 이전 데이터 초기화
            isRecording = true;
            Debug.Log("녹음 시작");
            StartCoroutine(RecordAudio());
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Debug.Log("녹음 종료");
            SaveRecording();
        }
    }

    private IEnumerator RecordAudio()
    {
        int bufferSize = 16384; // 샘플 크기
        float[] data = new float[bufferSize];
        float[] mixedData = new float[bufferSize]; // 모든 소리를 합칠 배열

        while (isRecording)
        {
            // mixedData를 초기화
            Array.Clear(mixedData, 0, mixedData.Length);

            foreach (var source in audioSources)
            {
                if (source.isPlaying)
                {
                    // AudioSource에서 데이터 가져오기
                    source.GetOutputData(data, 0);

                    // 데이터를 mixedData에 합치기
                    for (int i = 0; i < data.Length; i++)
                    {
                        mixedData[i] += data[i]; // 겹쳐서 저장
                    }
                }
            }

            // 최종적으로 합쳐진 mixedData를 recordedData에 추가
            recordedData.AddRange(mixedData);

            // 대기 시간 조정
            yield return new WaitForSeconds((float)bufferSize / sampleRate); // 일정 시간 대기
        }
    }

    private void SaveRecording()
    {
        if (recordedData.Count > 0)
        {
            // 녹음된 AudioClip 생성
            recordedClip = AudioClip.Create("RecordedClip", recordedData.Count * 2, 1, sampleRate, false);
            recordedClip.SetData(recordedData.ToArray(), 0); // 데이터 설정
        }
        else
        {
            Debug.LogWarning("녹음된 데이터가 없습니다.");
        }
    }

    // 녹음된 AudioClip 반환 메서드
    public AudioClip GetRecordedClip()
    {
        return recordedClip;
    }
}
