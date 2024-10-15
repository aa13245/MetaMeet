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
        // Body  데이터
        public string body = "";

        // contentType
        public string contentType = "";
        // 통신 성공 후 호출되는 함수 담을 변수
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
    // GET : 서버에게 데이터를 조회 요청
    public IEnumerator Get(HttpInfo info)
    {
        string url = info.url;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 서버에게 응답이 왔다.
            DoneRequest(webRequest, info);
        }
    }
    public IEnumerator Post(HttpInfo info)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Post(info.url, info.body, info.contentType))
        {
            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 서버에게 응답이 왔다.
            DoneRequest(webRequest, info);
        }
    }
    // 파일 업로드(form-data)
    public IEnumerator UploadFileByFormData(HttpInfo info)
    {
        // info.data에는 파일의 위치
        // info.data 에 있는 파일을 byte 배열로 읽어오자.
        byte[] data = File.ReadAllBytes(info.body);

        // data를 MultipartForm 으로 셋팅
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("file", data, "audio.wav", info.contentType));

        using (UnityWebRequest webRequest = UnityWebRequest.Post(info.url, formData))
        {
            print("요청");
            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 서버에게 응답이 왔다.
            DoneRequest(webRequest, info);
        }
    }

    // 파일 업로드
    public IEnumerator UploadFileByByte(HttpInfo info)
    {
        // info.data에는 파일의 위치
        // info.data 에 있는 파일을 byte 배열로 읽어오자.
        byte[] data = File.ReadAllBytes(info.body);

        using (UnityWebRequest webRequest = new UnityWebRequest(info.url, "Post"))
        {
            // 업로드 하는 데이터
            webRequest.uploadHandler = new UploadHandlerRaw(data);
            webRequest.uploadHandler.contentType = info.contentType;

            // 응답 받는 데이터 공간
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 서버에게 응답이 왔다.
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
            //handler.audioClip 을 audiosource에 세팅하고 플레이
            DoneRequest(webRequest, info);
        }
    }
    void DoneRequest(UnityWebRequest webRequest, HttpInfo info)
    {
        // 만약에 결과가 정상이라면
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // 응답 온 데이터를 요청한 클래스로 보내자.
            if (info.onComplete != null)
            {
                info.onComplete(webRequest.downloadHandler);
            }
            // print(webRequest.downloadHandler.text);
        }
        else
        {
            // 그렇지 않다면 (Error 라면)
            // Error 의 이유를 출력
            Debug.LogError("Net Error : " + webRequest.error);
        }
    }
}
