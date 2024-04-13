using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ApiHelper
{
    public class Loader : MonoBehaviour
    {
        static Loader _instance;
        public static Loader instance { get {
                if(_instance == null)
                {
                    GameObject x = new GameObject("Loader");
                    x.hideFlags = HideFlags.HideInHierarchy;
                    _instance = GameObject.Instantiate(x).AddComponent<Loader>();
                }
                return _instance;
            } }

        private const string API_Base_Url = "";

        public void Get(string endpoint, Action<string> onComplete, Action<string> onError)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new InvalidOperationException("Provided endpoint is not valid.");

            string url = System.IO.Path.Combine(API_Base_Url, endpoint);
            StartCoroutine(WebRequest(url, onComplete, onError));
        }

        private IEnumerator WebRequest(string url, Action<string> onSuccess, Action<string> onFailure)
        {
            UnityWebRequest req = new UnityWebRequest(url);
            req.downloadHandler = new DownloadHandlerBuffer();

            yield return req.SendWebRequest();

            if (string.IsNullOrEmpty(req.error))
            {
                if (string.IsNullOrEmpty(req.downloadHandler.text))
                    onFailure?.Invoke("Empty response received");
                else
                    onSuccess?.Invoke(req.downloadHandler.text);
            }
            else onFailure?.Invoke(req.error);

            req.Dispose();
            req.downloadHandler.Dispose();
        }

        ~Loader()
        {
            if (_instance == this)
            {
                Debug.Log("Loader instance was destroyed");
            }
        }
        private void Awake()
        {
            if(_instance==this)
                DontDestroyOnLoad(this);
        }
    }
}