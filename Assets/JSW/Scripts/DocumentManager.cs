using Photon.Pun;
using Photon.Voice.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using File = System.IO.File;

public class DocumentManager : MonoBehaviour
{
    AudioClip recordedClip;
    private List<float> recordedData = new List<float>(); // 누적할 데이터 리스트
    public Device device;
    string path = "";
    public bool IsRecording { get; private set; }
    public GameObject button;
    WhiteBoard whiteBoard;
    Recorder recorder;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("WhiteBoard(Clone)")  != null)
        {
            whiteBoard = GameObject.Find("WhiteBoard(Clone)").GetComponent<WhiteBoard>();
            recorder = whiteBoard.GetComponent<Recorder>();
        }
    }
    // 전체 녹음 시작
    public void StartRecording()
    {
        IsRecording = true;
        recordedData = new List<float>();
        StartCoroutine(ContinuousRecording());
        whiteBoard.StartRecording();
    }
    // 전체 녹음 종료
    public void StopRecording()
    {
        IsRecording = false;
        Microphone.End(null);
        StopCoroutine(ContinuousRecording());
        GatherData();
        recordedClip = AudioClip.Create("FinalRecording", recordedData.Count, 1, 4410, false);
        recordedClip.SetData(recordedData.ToArray(), 0);
        whiteBoard.StopRecording();
        // 병합
        recordedClip = MixAudioClips(whiteBoard.GetRecordedClip(), recordedClip);
        // 저장
        SaveAudioClipToWAV(Application.dataPath + "/test.wav");
        path = Application.dataPath + "/test.wav";
    }
    // AI 서버 회의 요약 요청
    public void GetDocument()
    {
        button.SetActive(false);
        // Api 호출
        HttpManager.HttpInfo info = new HttpManager.HttpInfo();
        info.url = "https://boss-goblin-tolerant.ngrok-free.app/cleaning_summary/";
        info.contentType = "audio/wav";
        info.body = path;
        info.onComplete = (DownloadHandler downloadHandler) =>
        {
            button.SetActive(true);
            print(downloadHandler.text);
            string value = downloadHandler.text;
            GameObject square = PhotonNetwork.Instantiate("Square", new Vector3(device.Objs.transform.position.x, device.Objs.transform.position.y, 0), Quaternion.identity);
            Square obj = square.GetComponent<Square>();
            obj.RPC_Init(Color.white);
            obj.RPC_Place();
            StartCoroutine(ChangeValue(obj, value));
        };
        StartCoroutine(HttpManager.GetInstance().UploadFileByFormData(info));
    }
    IEnumerator ContinuousRecording()
    {
        recordedClip = Microphone.Start(null, false, 60, 44100); // 녹음 시작
        while (true)
        {
            yield return new WaitForSeconds(60); // 60초 대기
            Microphone.End(null); // 녹음 종료
            GatherData();
            recordedClip = Microphone.Start(null, false, 60, 44100); // 녹음 시작
        }
    }
    // 녹음된 데이터를 누적하는 메소드
    void GatherData()
    {
        float[] data = new float[recordedClip.samples];
        recordedClip.GetData(data, 0);
        recordedData.AddRange(data);
    }
    IEnumerator ChangeValue(Square obj, string value)
    {
        yield return null;
        obj.OnInputValueChanged(value);
        obj.GetComponentInChildren<TMP_InputField>().text = value.Replace("\\n", "\n");
        obj.RPC_SetScale(new Vector3(5, 6, 1), true);
        obj.SetScale(new Vector3(5, 6, 1));
        obj.RPC_SetAlignment((int)TextAlignmentOptions.Left);
    }
    
    private void SaveAudioClipToWAV(string filePath)
    {
        if (recordedClip == null)
        {
            Debug.LogError("No AudioClip assigned.");
            return;
        }

        // AudioClip�� ����� ������ ��������
        float[] samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);

        // WAV ���� ��� �ۼ�
        using (FileStream fs = File.Create(filePath))
        {
            WriteWAVHeader(fs, recordedClip.channels, recordedClip.frequency, recordedClip.samples);
            ConvertAndWrite(fs, samples);
        }

        //Debug.Log("AudioClip saved as WAV: " + filePath);
    }

    // WAV ���� ��� �ۼ�
    private void WriteWAVHeader(FileStream fileStream, int channels, int frequency, int sampleCount)
    {
        var samples = sampleCount * channels;
        var fileSize = samples + 36;

        fileStream.Write(new byte[] { 82, 73, 70, 70 }, 0, 4); // "RIFF" ���
        fileStream.Write(BitConverter.GetBytes(fileSize), 0, 4);
        fileStream.Write(new byte[] { 87, 65, 86, 69 }, 0, 4); // "WAVE" ���
        fileStream.Write(new byte[] { 102, 109, 116, 32 }, 0, 4); // "fmt " ���
        fileStream.Write(BitConverter.GetBytes(16), 0, 4); // 16
        fileStream.Write(BitConverter.GetBytes(1), 0, 2); // ����� ���� (PCM)
        fileStream.Write(BitConverter.GetBytes(channels), 0, 2); // ä�� ��
        fileStream.Write(BitConverter.GetBytes(frequency), 0, 4); // ���� ����Ʈ
        fileStream.Write(BitConverter.GetBytes(frequency * channels * 2), 0, 4); // ����Ʈ ����Ʈ
        fileStream.Write(BitConverter.GetBytes(channels * 2), 0, 2); // ��� ũ��
        fileStream.Write(BitConverter.GetBytes(16), 0, 2); // ��Ʈ ����Ʈ
        fileStream.Write(new byte[] { 100, 97, 116, 97 }, 0, 4); // "data" ���
        fileStream.Write(BitConverter.GetBytes(samples), 0, 4);
    }

    // ����� ������ ��ȯ �� �ۼ�
    private void ConvertAndWrite(FileStream fileStream, float[] samples)
    {
        Int16[] intData = new Int16[samples.Length];
        // float -> Int16 ��ȯ
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767);
        }
        // Int16 ������ �ۼ�
        Byte[] bytesData = new Byte[intData.Length * 2];
        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        fileStream.Write(bytesData, 0, bytesData.Length);
    }
    // 오디오클립 병합
    AudioClip MixAudioClips(AudioClip clip1, AudioClip clip2)
    {
        // 각 클립의 샘플 수와 채널 수
        int clip1Samples = clip1.samples;
        int clip2Samples = clip2.samples;
        int channels = clip1.channels;

        // 최대 샘플 수 계산
        int maxSamples = Mathf.Max(clip1Samples, clip2Samples);

        // 새로운 샘플 배열 생성 (최대 샘플 수)
        float[] mixedSamples = new float[maxSamples * channels];

        // 첫 번째 클립의 샘플 데이터 가져오기
        float[] clip1Data = new float[clip1Samples * channels];
        clip1.GetData(clip1Data, 0);

        // 두 번째 클립의 샘플 데이터 가져오기
        float[] clip2Data = new float[clip2Samples * channels];
        clip2.GetData(clip2Data, 0);

        // 믹싱 과정
        for (int i = 0; i < maxSamples; i++)
        {
            // 첫 번째 클립 샘플
            float sample1 = (i < clip1Samples) ? clip1Data[i] : 0f;

            // 두 번째 클립 샘플
            float sample2 = (i < clip2Samples) ? clip2Data[i] : 0f;

            // 두 샘플을 합치기
            float mixedSample = sample1 + sample2;

            // 클리핑 방지
            mixedSample = Mathf.Clamp(mixedSample, -1f, 1f);

            // 믹싱된 샘플을 새로운 배열에 저장
            mixedSamples[i * channels] = mixedSample; // 모노일 경우
            if (channels > 1)
            {
                mixedSamples[i * channels + 1] = mixedSample; // 스테레오일 경우
            }
        }

        // 새로운 AudioClip 생성
        AudioClip mixedClip = AudioClip.Create("MixedClip", maxSamples, channels, clip1.frequency, false);
        mixedClip.SetData(mixedSamples, 0);

        return mixedClip;
    }
}
