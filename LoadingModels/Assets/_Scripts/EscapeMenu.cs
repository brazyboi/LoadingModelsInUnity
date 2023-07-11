using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EscapeMenu : GameBase
{
    public GameObject escapeMenu;
    public TextMeshProUGUI GodModeText;
    public Slider heightSlider;
    public static bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        base.init();
        Cursor.visible = false;
        escapeMenu.SetActive(false);
    }

    float timing = 0f;
    // Update is called once per frame
    void Update()
    {
        if (timing > 0f)
        {
            timing -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) { 
                ResumeApp();
            } else
            {
                PauseApp();
            }
        }

        DisplayGodModeText();
    }

    void DisplayGodModeText()
    {
        if (manager.isGodMode)
        {
            GodModeText.text = "God";
        } else
        {
            GodModeText.text = "";
        }
    }

    void PauseApp()
    {
        escapeMenu.SetActive(true);
        Time.timeScale = 0.0f;
        Cursor.visible = true;
        isPaused = true;
    }
    
    void ResumeApp()
    {
        escapeMenu.SetActive(false);
        Time.timeScale = 1.0f;
        Cursor.visible = false;
        isPaused = false;
    }

    
}
