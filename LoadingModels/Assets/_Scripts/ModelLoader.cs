using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

using Newtonsoft.Json.Linq;
using Siccity.GLTFUtility;
using glTFLoader;

public class ModelLoader : MonoBehaviour
{
    //wrapper for model to load into
    GameObject wrapper;
    //where all the downloaded files are stored
    string filePath;
    //index of model
    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        filePath = $"{Application.persistentDataPath}/Files/";
        //create textures folder if doesn't exist
        if (!Directory.Exists($"{filePath}textures/"))
        {
            Directory.CreateDirectory($"{filePath}textures/");
        }
        StartCoroutine(GetModelData("http://34.230.66.189/api/?type=gltf"));
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GetModelData(string url)
    {
        //send request to api
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            //convert the api to a JSON
            JObject modelData = ParseApi(uwr.downloadHandler.text);

            //iterate through each model in the JSON
            foreach (var item in modelData["items"])
            {
                if (item["name"].ToObject<string>() == "desk" || item["name"].ToObject<string>() == "books")
                {
                    continue;
                }
                //create GameObject for current model
                wrapper = new GameObject
                {
                    name = "Model " + index
                };
                Debug.Log(item);
                //urls for scene.bin and scene.gltf
                string binUrl = item["source"][0]["url"].ToObject<string>();
                string gltfUrl = item["source"][1]["url"].ToObject<string>();

                string path = GetFilePath(gltfUrl);
                if (File.Exists(path))
                {
                    Debug.Log("Found file locally, loading...");
                    ModelLoad(path);
                }
                else
                {
                    yield return StartCoroutine(DownloadFile(binUrl, GetFilePath(binUrl)));
                    yield return StartCoroutine(DownloadFile(gltfUrl, GetFilePath(gltfUrl)));
                    foreach (var texture in item["textures"])
                    {
                        string textureUrl = texture["url"].ToObject<string>();
                        Debug.Log("Texture url: " + textureUrl);
                        yield return StartCoroutine(DownloadFile(textureUrl, GetTextureFilePath(textureUrl)));
                    }

                    ChangeBinUri(path);
                    ModelLoad(path);
                }
                index++;
            }
        }
    }

    //downloads file to a specific path given the url
    IEnumerator DownloadFile(string url, string path)
    {
        Debug.Log("Path: " + path);
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        uwr.downloadHandler = new DownloadHandlerFile(path);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(uwr.error);
        }
        else
        {
            Debug.Log("Success");
        }
    }

    //loads the model into Unity
    void ModelLoad(string path)
    {
        ResetWrapper();
        Debug.Log(path);
        GameObject model = Importer.LoadFromFile(path);
        Debug.Log("here");
        model.transform.SetParent(wrapper.transform);
    }

    //resets the GameObject wrapper
    void ResetWrapper()
    {
        if (wrapper != null)
        {
            foreach (Transform trans in wrapper.transform)
            {
                Destroy(trans.gameObject);
            }
        }
    }

    //changes the gltf's target bin uri
    void ChangeBinUri(string path)
    {
        var deserializedGltf = Interface.LoadModel(path);


        for (int i = 0; i < deserializedGltf.Buffers.Length; ++i)
        {
            Debug.Log(deserializedGltf.Buffers.Length);
            Debug.Log(deserializedGltf.Buffers[i].Uri);
            deserializedGltf.Buffers[i].Uri = index + "scene.bin";
            Debug.Log(deserializedGltf.Buffers[i].Uri);
            Interface.SaveModel(deserializedGltf, path);
        }
    }

    //returns the file path for a file downloaded from a url
    public string GetFilePath(string url)
    {
        string[] pieces = url.Split('/');
        string filename = index + pieces[pieces.Length - 1];

        return $"{filePath}{filename}";
    }

    //same as GetFilePath() but into the textures folder
    public string GetTextureFilePath(string url)
    {
        string[] pieces = url.Split("/");
        string filename = pieces[pieces.Length - 1];

        return $"{filePath}textures/{filename}";
    }
    
    //parses the api text into a JSON
    public JObject ParseApi(string json)
    {
        return JObject.Parse(json);
    }
}
