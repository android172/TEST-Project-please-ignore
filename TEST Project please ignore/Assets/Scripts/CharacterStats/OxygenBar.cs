using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OxygenBar : MonoBehaviour
{

    public Slider o2Bar;
    public HudController playerOxygen;

    // Start is called before the first frame update
    void Start()
    {
        playerOxygen = GameObject.FindGameObjectWithTag("HUD").GetComponent<HudController>();
        o2Bar = GetComponent<Slider>();
        o2Bar.maxValue = playerOxygen.maxO2;
        o2Bar.value = playerOxygen.maxO2;
    }

    public void SetOxygen(float amount)
    {
        o2Bar.value = amount;
    }
}
