using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reload : MonoBehaviour
{

    public GameObject modelLoader;
    public Transform playerCapsule;
    public Slider heightSlider;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Resize();
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

    public void ResetView()
    {
        Camera.main.fieldOfView = 100f;
        
    }

    void Resize()
    {
        float val = heightSlider.value * 10;
        playerCapsule.transform.position = new Vector3(playerCapsule.transform.position.x, val / 2, playerCapsule.transform.position.z);
        playerCapsule.transform.localScale = new Vector3(playerCapsule.transform.localScale.x, val, playerCapsule.transform.localScale.z);
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}
