using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBase : MonoBehaviour
{
    // Start is called before the first frame update
    protected GameManagerScript manager;

    protected void init()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManagerScript>();
    }
}
