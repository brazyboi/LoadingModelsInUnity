using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reload : MonoBehaviour
{

    public GameObject modelLoader;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //reloads the scene by deleting the model loader and then regenerating it
    public void ReloadScene()
    {
        //wipe hiearchy
        var objects = GameObject.FindGameObjectsWithTag("model");
        foreach (var obj in objects)
        {
            Destroy(obj.gameObject);
        }
        GameObject ml = GameObject.FindGameObjectWithTag("ModelLoader");
        if (ml != null)
        {
            Destroy(ml);
        }
        Instantiate(modelLoader);
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}
