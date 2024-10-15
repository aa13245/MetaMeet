using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

[System.Serializable]
public struct PostInfo
{
    public int userId;
    public int id;
    public string title;
    public string body;
}
[System.Serializable]
public struct PostInfoArray
{
    public List<PostInfo> data;
}
public class HttpManager : MonoBehaviour
{
    static HttpManager instance;
    public static HttpManager GetInstance()
    {
        if (instance == null)
        {
            GameObject go = new GameObject();
            go.name = "HttpManager";
            go.AddComponent<HttpManager>();
        }
        return instance;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public class HttpInfo
    {
        public string url = "";
        // Body  ������
        public string body = "";

        // contentType
        public string contentType = "";
        // ��� ���� �� ȣ��Ǵ� �Լ� ���� ����
        public Action<DownloadHandler> onComplete;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    // GET : �������� �����͸� ��ȸ ��û
    public IEnumerator Get(HttpInfo info)
    {
        string url = info.url;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // ������ ��û ������
            yield return webRequest.SendWebRequest();

            // �������� ������ �Դ�.
            DoneRequest(webRequest, info);
        }
    }
    public IEnumerator Post(HttpInfo info)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Post(info.url, info.body, info.contentType))
        {
            // ������ ��û ������
            yield return webRequest.SendWebRequest();

            // �������� ������ �Դ�.
            DoneRequest(webRequest, info);
        }
    }
    // ���� ���ε�(form-data)
    public IEnumerator UploadFileByFormData(HttpInfo info)
    {
        // info.data���� ������ ��ġ
        // info.data �� �ִ� ������ byte �迭�� �о����.
        byte[] data = File.ReadAllBytes(info.body);

        // data�� MultipartForm ���� ����
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("file", data, "audio.wav", info.contentType));

        using (UnityWebRequest webRequest = UnityWebRequest.Post(info.url, formData))
        {
            print("��û");
            // ������ ��û ������
            yield return webRequest.SendWebRequest();

            // �������� ������ �Դ�.
            DoneRequest(webRequest, info);
        }
    }

    // ���� ���ε�
    public IEnumerator UploadFileByByte(HttpInfo info)
    {
        // info.data���� ������ ��ġ
        // info.data �� �ִ� ������ byte �迭�� �о����.
        byte[] data = File.ReadAllBytes(info.body);

        using (UnityWebRequest webRequest = new UnityWebRequest(info.url, "Post"))
        {
            // ���ε� �ϴ� ������
            webRequest.uploadHandler = new UploadHandlerRaw(data);
            webRequest.uploadHandler.contentType = info.contentType;

            // ���� �޴� ������ ����
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            // ������ ��û ������
            yield return webRequest.SendWebRequest();

            // �������� ������ �Դ�.
            DoneRequest(webRequest, info);
        }
    }
    public IEnumerator DownloadSprite(HttpInfo info)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(info.url))
        {
            yield return webRequest.SendWebRequest();
            DoneRequest(webRequest, info);
        }
    }
    public IEnumerator DownloadAudio(HttpInfo info)
    {
        using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(info.url, AudioType.WAV))
        {
            yield return webRequest.SendWebRequest();
            //DownloadHandlerAudioClip handler = webRequest.downloadHandler as DownloadHandlerAudioClip;
            //handler.audioClip �� audiosource�� �����ϰ� �÷���
            DoneRequest(webRequest, info);
        }
    }
    void DoneRequest(UnityWebRequest webRequest, HttpInfo info)
    {
        // ���࿡ ����� �����̶��
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // ���� �� �����͸� ��û�� Ŭ������ ������.
            if (info.onComplete != null)
            {
                info.onComplete(webRequest.downloadHandler);
            }
            // print(webRequest.downloadHandler.text);
        }
        else
        {
            // �׷��� �ʴٸ� (Error ���)
            // Error �� ������ ���
            Debug.LogError("Net Error : " + webRequest.error);
        }
    }
}
