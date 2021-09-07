using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{

    private GameObject menu;
    private float originalTimeScale;

    private bool isPaused = false;
    private bool atmoshpereOn = true;
    private bool cloudOn = true;
    private bool cloudDetailOn = true;

    private Slider CSSlider;
    private Text CSText;


    void Awake()
    {
        menu = GameObject.Find("MainMenuPanel");
        originalTimeScale = Time.timeScale;


        CSSlider = GameObject.Find("CloudSamples").GetComponentInChildren<Slider>();
        //CSSlider.value = 8;
        //CSSlider.maxValue = 64;
        //CSSlider.minValue = 2;
        

        CSText = GameObject.Find("NumOfSamples").GetComponent<Text>();
        CSText.text = "" + CSSlider.value;

        

        menu.SetActive(false);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                ResumeGame();
            }
            else
            {
                Time.timeScale = 0;
                menu.SetActive(true);
                isPaused = true;
            }
        }
        
    }

    public void ResumeGame()
    {
        menu.SetActive(false);
        isPaused = false;
        Time.timeScale = originalTimeScale;
    }

    public void AtmosphereSetting()
    {
        if(atmoshpereOn)
        {
            atmoshpereOn = false;
            GameObject.Find("AtmosphereButton").GetComponentInChildren<Text>().text = "Atmoshpere: OFF";
        }
        else
        {
            atmoshpereOn = true;
            GameObject.Find("AtmosphereButton").GetComponentInChildren<Text>().text = "Atmoshpere: ON";
        }
        
        
    }

    public void CloudSetting()
    {
        if(cloudOn)
        {
            cloudOn = false;
            GameObject.Find("CloudButton").GetComponentInChildren<Text>().text = "Clouds: OFF";
        }
        else
        {
            cloudOn = true;
            GameObject.Find("CloudButton").GetComponentInChildren<Text>().text = "Clouds: ON";
        }
    }

    public void HighResSetting()
    {
        if(cloudDetailOn)
        {
            cloudDetailOn = false;
            GameObject.Find("HighResCloudButton").GetComponentInChildren<Text>().text = "Cloud Detail: OFF";
        }
        else
        {
            cloudDetailOn = true;
            GameObject.Find("HighResCloudButton").GetComponentInChildren<Text>().text = "Cloud Detail: ON";
        }
    }

    public void SliderOnChange()
    {
        CSText.text = "" + CSSlider.value;
    }

    
}
