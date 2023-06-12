using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

using Newtonsoft.Json.Linq;
using Siccity.GLTFUtility;
using glTFLoader;
using System.ComponentModel.Design.Serialization;

public class ModelLoader : MonoBehaviour
{
    //wrapper for model to load into
    GameObject wrapper;
    string filePath;
    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        filePath = $"{Application.persistentDataPath}/Files/";
        StartCoroutine(GetModelData("http://34.230.66.189/api/?type=gltf"));
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GetModelData(string url)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            JObject modelData = ParseApi(uwr.downloadHandler.text);
            Debug.Log("First item: " + modelData["items"].First);
            foreach (var item in modelData["items"])
            {
                wrapper = new GameObject
                {
                    name = "Model " + index
                };
                Debug.Log(item);
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
                    yield return StartCoroutine(DownloadFile(binUrl));
                    yield return StartCoroutine(DownloadFile(gltfUrl));
                    ChangeBinUri(path);
                    ModelLoad(path);
                }
                index++;
            }
        }
    }

    IEnumerator DownloadFile(string url)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        uwr.downloadHandler = new DownloadHandlerFile(GetFilePath(url));
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

    void ModelLoad(string path)
    {
        ResetWrapper();
        Debug.Log(path);
        GameObject model = Importer.LoadFromFile(path);
        Debug.Log("here");
        model.transform.SetParent(wrapper.transform);
    }

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

    public string GetFilePath(string url)
    {
        string[] pieces = url.Split('/');
        string filename = index + pieces[pieces.Length - 1];

        return $"{filePath}{filename}";
    }

    public JObject ParseApi(string json)
    {
        return JObject.Parse(json);
    }
}
