using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class Choice
{
    public string text;
}

[Serializable]
public class OpenAIAPI
{
    public Choice[] choices;
    public string model;
}

[Serializable]
public class RequestData
{
    public string model;
    public string prompt;
    public float temperature;
    public int max_tokens;
    public int top_p;
    public int frequency_penalty;
    public int presence_penalty;
}

namespace UnityLibrary
{
    public class OpenAI : MonoBehaviour
    {
        const string url = "https://api.openai.com/v1/completions";

        public string modelName = "text-davinci-003";

        public InputField inputPrompt;
        public InputField inputResults;

        string apiKey = null;
        bool isRunning = false;


        void Start()
        {
            LoadAPIKey();
        }

        public void Execute()
        {
            if (isRunning)
            {
                Debug.LogError("Already running");
                return;
            }
            isRunning = true;
            
            // fill in request data
            RequestData requestData = new RequestData()
            {
                model = modelName,
                prompt = inputPrompt.text,
                temperature = 0.7f,
                max_tokens = 256,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0
            };

            string jsonData = JsonUtility.ToJson(requestData);

            byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);
            StartCoroutine(HandleRequest(postData, jsonData));
        } // execute


        private IEnumerator HandleRequest(byte[] postData, string jsonData)
        {
            UnityWebRequest request = UnityWebRequest.Post(url, jsonData);
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                OpenAIAPI responseData = JsonUtility.FromJson<OpenAIAPI>(request.downloadHandler.text);

                if (responseData.model != null && responseData.model.StartsWith("Error:"))
                {
                    Debug.LogError("OpenAI API Error: " + responseData.model);
                }
                else if (responseData.choices != null && responseData.choices.Length > 0)
                {
                    string generatedText = responseData.choices[0].text.TrimStart('\n').TrimStart('\n');
                    MainThreadDispatcher.RunOnMainThread(() => {
                        inputResults.text = generatedText;
                 
                    });
                }
                else
                {
                    Debug.LogError("Unexpected API response");
                }
            }

            // Dispose of the web request
            request.Dispose();
            isRunning = false;
        }







        void LoadAPIKey()
        {
            // TODO optionally use from env.variable

            // MODIFY path to API key if needed
            var keypath = Path.Combine(Application.streamingAssetsPath, "secretkey.txt");
            if (File.Exists(keypath) == false)
            {
                Debug.LogError("Apikey missing: " + keypath);
            }

            //Debug.Log("Load apikey: " + keypath);
            apiKey = File.ReadAllText(keypath).Trim();
            Debug.Log("API key loaded, len= " + apiKey.Length);
        }
    }
}
