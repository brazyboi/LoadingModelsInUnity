using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public bool isGodMode = false;
    public Vector3 default_pos;
    public Vector3 default_rot;
    public Vector3 default_scale;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetToDefaultView()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<CharacterController>().enabled = false;
            player.transform.position = default_pos;
            player.transform.eulerAngles = default_rot;
            player.transform.localScale = default_scale;
            player.GetComponent<CharacterController>().enabled = true;
        }
    }
}
