using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBoard : MonoBehaviour
{
    public int Sharer { get; set; }
    public Transform objsTf;
    List<GameObject> Objs = new List<GameObject>();

    public int Idxcnt { get; set; }
    public List<Display> displays = new List<Display>();
    // ������Ʈ ��Ͽ� �߰�
    public void Add(GameObject obj)
    {
        Objs.Add(obj);
        if (Objs.Count == 1)
        {
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, objsTf.position.z);
        }
        else
        {
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, Objs[^2].transform.position.z - 0.000001f);
        }
    }
    // ������Ʈ ��Ͽ��� ����
    public void Remove(GameObject obj)
    {
        Objs.Remove(obj);
    }
    // �� ��/�ڷ� ������
    public void MoveFrontOrBack(GameObject obj, bool front)
    {
        if (Objs.Count == 1) return;
        if (front)
        {
            Objs.Remove(obj);
            Objs.Add(obj);
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, Objs[^2].transform.position.z - 0.000001f);
        }
        else
        {
            Objs.Remove(obj);
            Objs.Insert(0, obj);
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, Objs[1].transform.position.z + 0.000001f);
        }
    }

    // ���� ���̽� ��ü ���� ���
    public List<AudioSource> audioSources; // ������ AudioSource ����Ʈ
    private AudioClip recordedClip; // ������ AudioClip
    private int sampleRate = 44100; // ���ø� ���ļ�
    private bool isRecording = false; // ���� ����
    private List<float> recordedData; // ������ ������ ����

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
            recordedData.Clear(); // ���� ������ �ʱ�ȭ
            isRecording = true;
            Debug.Log("���� ����");
            StartCoroutine(RecordAudio());
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Debug.Log("���� ����");
            SaveRecording();
        }
    }

    private IEnumerator RecordAudio()
    {
        int bufferSize = 16384; // ���� ũ��
        float[] data = new float[bufferSize];
        float[] mixedData = new float[bufferSize]; // ��� �Ҹ��� ��ĥ �迭

        while (isRecording)
        {
            // mixedData�� �ʱ�ȭ
            Array.Clear(mixedData, 0, mixedData.Length);

            foreach (var source in audioSources)
            {
                if (source.isPlaying)
                {
                    // AudioSource���� ������ ��������
                    source.GetOutputData(data, 0);

                    // �����͸� mixedData�� ��ġ��
                    for (int i = 0; i < data.Length; i++)
                    {
                        mixedData[i] += data[i]; // ���ļ� ����
                    }
                }
            }

            // ���������� ������ mixedData�� recordedData�� �߰�
            recordedData.AddRange(mixedData);

            // ��� �ð� ����
            yield return new WaitForSeconds((float)bufferSize / sampleRate); // ���� �ð� ���
        }
    }

    private void SaveRecording()
    {
        if (recordedData.Count > 0)
        {
            // ������ AudioClip ����
            recordedClip = AudioClip.Create("RecordedClip", recordedData.Count * 2, 1, sampleRate, false);
            recordedClip.SetData(recordedData.ToArray(), 0); // ������ ����
        }
        else
        {
            Debug.LogWarning("������ �����Ͱ� �����ϴ�.");
        }
    }

    // ������ AudioClip ��ȯ �޼���
    public AudioClip GetRecordedClip()
    {
        return recordedClip;
    }
}
