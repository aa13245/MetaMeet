using Cysharp.Threading.Tasks;
using Photon.Pun.Demo.Cockpit;
using Photon.Pun.Demo.Cockpit.Forms;
using Photon.Realtime;
using Postgrest.Attributes;
using Postgrest.Models;
using Supabase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Client = Supabase.Client;

namespace com.example
{
    public class SupabaseManager : MonoBehaviour
    {
        public static SupabaseManager instance { get; private set; }
        public static string session_Id = "";

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != null)
            { 
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }

        public SupabaseSettings supabaseSettings = null;
        private Client supabase;

        SupabaseOptions options = InitSupabaseOptions();

        private async void Start()
        {
            SupabaseOptions options = InitSupabaseOptions();

            supabase = new Supabase.Client(supabaseSettings.SupabaseURL, supabaseSettings.SupabaseAnonKey, options);
            
            await supabase.InitializeAsync();

            //FileUpload();
            //FileDownload();
        }



        public static async void DeletePlacedObjectList(Client supabase)
        {
            await supabase.From<PlacedObject>().Where(x=>x.roomid == 0).Delete();
        }



        public void InsertPlacedObectByPlacedObjectClass(List<PlacedObject> placedObject)
        {
            DeletePlacedObjectList(supabase);


            foreach (PlacedObject o in placedObject)
            {
                InsertPlacedObject(supabase, o);
            }


            //StartCoroutine(InsertPlacedObect(placedObject));

           
        }

        IEnumerator InsertPlacedObect(List<PlacedObject> placedObject)
        {
            yield return new WaitForSeconds(.1f); 

            foreach (PlacedObject o in placedObject)
            {
                InsertPlacedObject(supabase, o);
            }
        }



        public async UniTask<string> InsertCallUserData(string name, string password)
        {
            string message = await InsertUserData(supabase, new UserData
            {
                name = name,
                password = password
            });

            return message;
        }
      


        private static SupabaseOptions InitSupabaseOptions()
        {
            return new SupabaseOptions
            {
                AutoConnectRealtime = true,
            };
        }




        private static async void InsertPlacedObject(Client supabase, PlacedObject placedObject)
        {
            var result = await supabase.From<PlacedObject>().Insert(placedObject);
        }

        /*
        private static async UniTask<string> InsertUserData(string name, string password)
        {
            string message = await InsertUserData(supabase, new UserData
            {
                name = name,
                password = password
            });

            return message;
        }
        */

        public async void InsertCurrentRoom(int data)
        {
            await supabase.From<CurrentRoom>().Where(x => x.roomid != -1).Delete();
            await supabase.From<CurrentRoom>().Insert(new CurrentRoom { roomid = data });
        }

        public async UniTask<int> GetCurrentRoom()
        {
            var resultGet = await supabase.From<CurrentRoom>().Select("*").Get();
            List<CurrentRoom> productsGet = resultGet.Models;
            return productsGet[0].roomid;
        }



        private static async UniTask<string> InsertUserData(Client supabase, UserData userData)
        {
            var resultGet = await supabase.From<UserData>().Select("*").Get();

            List<UserData> productsGet = resultGet.Models;

            bool Yn = false;

            string message = "";

            foreach (var product1 in productsGet)
            {
                if (product1.name == userData.name)
                {
                    message = product1.name + "이미 저장된 이름입니다.";
                    return message;               
                }
            }

            if (!Yn)
            {
                var result = await supabase.From<UserData>().Insert(userData);
                message = "등록 되었습니다.";
            }

            return message;
        }

        public async UniTask<string> SelectUserDataByName(string name, string password)
        {
            string message = await GetSelectUserDataByName(supabase, name, password);
            return message;
        }



        private static async UniTask<string> GetSelectUserDataByName(Client supabase, string name, string password)
        {
            var result = await supabase.From<UserData>().Select("*").Where(x => x.name == name).Get();

            string message = "";

            if (result != null)
            {
                List<UserData> serverUserData = result.Models;

                if (serverUserData.Count == 0)
                {
                   
                    Debug.Log("아이디 다름");
                    message = "아이디 다름";
                }

                foreach (var serverUser in serverUserData)
                {
                    if (serverUser.name == name)
                    {
                        if (serverUser.password == password)
                        {
                            // 로그인 성공시에 세션 저장
                            session_Id = serverUser.name;
                            Debug.Log("로그인 성공");
                            message = "로그인 성공";
                        }
                        else
                        {
                           
                            Debug.Log("패스워드 다름");
                            message = "패스워드 다름";
                        }
                    }
                      
                }
            }

            return message;
        }

        



        public async UniTask<List<UserData>> GetUserData()
        {
            var userData = await GetUserData(supabase);
            return userData.ToList();
        }

        public async UniTask<List<PlacedObject>> SelectPlacedObject(int roomId)
        {
            var products = await SelectPlacedObject(supabase, roomId);
            return products.ToList();
        }

        public async void FileUpload(string path, string fileName)
        {
            var imagePath = Path.Combine(path, fileName);

            Debug.Log($"{imagePath}");

            await supabase.Storage
              .From("File").Upload(imagePath, fileName, onProgress: (sender, progress) => Debug.Log($"{progress}%"));
        }

        public async void SetNickName(string session_Id, string nickName)
        {
            var update = await supabase
              .From<UserData>()
              .Where(x => x.name == session_Id)  // 아디가 동일한지 
              .Set(x => x.nickName, nickName)  // 닉네임 변경
              .Update();
        }

        public async UniTask<string> GetNickName(string session_Id)
        {
            var result = await supabase
              .From<UserData>()
              .Where(x => x.name == session_Id)  // 아디가 동일한지 
              .Get();
            List<UserData> products = result.Models;

            return products[0].nickName;
        }



        public async UniTask<byte[]> FileDownload(string path)
        {
            var bytes = await supabase.Storage
              .From("File")
              .Download(path, (sender, progress) => Debug.Log($"{progress}%"));

            return bytes;
        }

        public async void FileDelete(string fileName)
        { 
            await supabase.Storage.From("File").Remove(new List<string> { fileName });
        }



        private static async UniTask<List<PlacedObject>> SelectPlacedObject(Client supabase, int roomId)
        {
            var result = await supabase.From<PlacedObject>().Select("*").Where(x => x.roomid == roomId).Get();
            List<PlacedObject> products = result.Models;
            return products.ToList();
        }

        private static async UniTask<List<UserData>> GetUserData(Client supabase)
        {
            var result = await supabase.From<UserData>().Select("*").Get();
            List<UserData> products = result.Models;

            return products.ToList();
        }

        private void OnDisable()
        {
            if (supabase != null)
            { 
                supabase = null;
            
            }

        }
    }

    


}
