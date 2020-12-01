using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterCam : MonoBehaviour
{
    public float waterHeight;

    private bool isUnderwater;
    private Color normalCol;
    private Color uwCol;

    // Start is called before the first frame update
    void Start()
    {
        normalCol = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        uwCol = new Color (0.22f, 0.65f, 0.77f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if((transform.position.y < waterHeight) != isUnderwater)
        {
            isUnderwater = transform.position.y < waterHeight;
            if(isUnderwater)
            {
                RenderSettings.fogColor = normalCol;
                RenderSettings.fogDensity = 0.01f;
            }
            if(!isUnderwater)
            {
                RenderSettings.fogColor = uwCol;
                RenderSettings.fogDensity = 0.1f;
            }
        }
    }
}
