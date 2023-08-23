using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

using Newtonsoft.Json.Linq;
using Siccity.GLTFUtility;
using glTFLoader;
using GLTFast;
using Unity.VisualScripting;

public class ModelLoader : MonoBehaviour
{
    //where all the downloaded files are stored
    string filePath;
    //index of model
    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        filePath = $"{Application.persistentDataPath}/Files/";

        //reset folder
        if (Directory.Exists(filePath)) { Directory.Delete(filePath, true); }
        Directory.CreateDirectory(filePath);

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
            Debug.Log("First item: " + modelData["items"][0]);
            yield return StartCoroutine(IterateJsonItems(modelData, "people"));
            yield return StartCoroutine(IterateJsonItems(modelData, "environment"));
            yield return StartCoroutine(IterateJsonItems(modelData, "items"));
        }
    }

    IEnumerator IterateJsonItems(JObject modelData, string itemType)
    {
        //iterate through each model in the JSON
        foreach (var item in modelData[itemType])
        {
            Debug.Log(item);
            //transforms
            float posX = item["position"]["x"].ToObject<float>();
            float posY = item["position"]["y"].ToObject<float>();
            float posZ = item["position"]["z"].ToObject<float>();

            float rotX = item["rotation"]["x"].ToObject<float>();
            float rotY = item["rotation"]["y"].ToObject<float>();
            float rotZ = item["rotation"]["z"].ToObject<float>();

            float scaleX = item["scale"]["x"].ToObject<float>();
            float scaleY = item["scale"]["y"].ToObject<float>();
            float scaleZ = item["scale"]["z"].ToObject<float>();

            float[] tf = { posX, posY, posZ, rotX, rotY, rotZ, scaleX, scaleY, scaleZ };

            Debug.Log("Transform: " + tf);

            //urls for scene.bin and scene.gltf
            /*string binUrl = item["source"][0]["url"].ToObject<string>();
            string gltfUrl = item["source"][1]["url"].ToObject<string>();*/

            //GLB File
            string gltfUrl = item["source"][0]["url"].ToObject<string>(); 

            string path = GetFilePath(gltfUrl);
            if (File.Exists(path))
            {
                Debug.Log("Found file locally, loading...");
                ModelLoad(path, tf);
            }
            else
            {
                //GLTF File
                /*yield return StartCoroutine(DownloadFile(binUrl, GetFilePath(binUrl)));
                yield return StartCoroutine(DownloadFile(gltfUrl, GetFilePath(gltfUrl)));
                foreach (var texture in item["textures"])
                {
                    string textureUrl = texture["url"].ToObject<string>();
                    Debug.Log("Texture url: " + textureUrl);
                    yield return StartCoroutine(DownloadFile(textureUrl, GetTextureFilePath(textureUrl)));
                }

                ChangeBinUri(path);
                ModelLoad(path, tf);*/
                
                //GLB File
                yield return StartCoroutine(DownloadFile(gltfUrl, GetFilePath(gltfUrl)));
                ModelLoad(path, tf);
            }
            index++;
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
            Debug.Log("Downloaded url: " + url);
            Debug.LogError(uwr.error);
        }
        else
        {
            Debug.Log("Download Success");
        }
    }

    //loads the model into Unity with proper transform
    async void ModelLoad(string path, float[] tf)
    {
        //ResetWrapper();
        Debug.Log(path);
        GameObject model = new GameObject("gltf");
        model.tag = "model";
        var gltf = model.AddComponent<GLTFast.GltfAsset>();
        await gltf.Load(path);
        AddBoxCollider(model);
        model.transform.localPosition = new Vector3(tf[0], tf[1], tf[2]);
        model.transform.localEulerAngles = new Vector3(tf[3], tf[4], tf[5]);
        model.transform.localScale = new Vector3(tf[6], tf[7], tf[8]);
    }

    //resets the GameObject wrapper
    /*void ResetWrapper()
    {
        if (wrapper != null)
        {
            foreach (Transform trans in wrapper.transform)
            {
                Destroy(trans.gameObject);
            }
        }
    }*/

    //adds a box collider to the model
    void AddBoxCollider(GameObject model)
    {
        model.AddComponent<BoxCollider>();
        model.GetComponent<BoxCollider>().size = FindBounds(model).size;
        model.GetComponent<BoxCollider>().center = FindBounds(model).center;
    }

    //finds the bounds of the model
    Bounds FindBounds(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        Debug.Log("Renderers: " + renderers.Length);
        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }
        return combinedBounds;
    }

    //changes the gltf's target bin uri
    void ChangeBinUri(string path)
    {
        var deserializedGltf = Interface.LoadModel(path);

        for (int i = 0; i < deserializedGltf.Buffers.Length; ++i)
        {
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
