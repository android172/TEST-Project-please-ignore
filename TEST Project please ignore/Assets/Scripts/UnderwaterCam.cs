using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class UnderwaterCam : MonoBehaviour
{
    public Volume volume;
    public float distortionFrequency;

    private bool isUnderwater;
    private Color normalCol;
    private Color uwCol;
    private LensDistortion lensDistortion;
    private Fog fog;
    private float time;
    

    // Start is called before the first frame update
    void Start()
    {
        time = Time.deltaTime;
        
        if(volume == null)
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>() ;
        foreach(GameObject go in allObjects)
            if(go.name == "SceneSettings")
                volume = go.GetComponent<Volume>();
        }
        
        volume.profile.TryGet<LensDistortion>(out lensDistortion);
        volume.profile.TryGet<Fog>(out fog);

        lensDistortion.intensity.value = 0.0f;
        fog.enabled.value = false;
        fog.albedo = new ColorParameter(new Color(0.0f, 108.0f/255.0f, 1.0f));
        
    }

    // Update is called once per frame
    void Update()
    {
        isUnderwater = CheckUnderwater();
        //Debug.Log(isUnderwater);
        if(isUnderwater)
        {
            lensDistortion.intensity.value = 0.7f;
            time += Time.deltaTime;
            lensDistortion.xMultiplier.value = Mathf.Sin(time/2 * distortionFrequency)/1.5f;
            lensDistortion.yMultiplier.value = Mathf.Sin(time/3 * distortionFrequency)/1.5f;

            fog.enabled.value = true;
        }
        else
        {
            lensDistortion.intensity.value = 0.0f;
            fog.enabled.value = false;
        }
    }

    bool CheckUnderwater()
    {

        foreach(GameObject w in GameObject.FindGameObjectsWithTag("Ocean"))
        {
            float radius = w.GetComponent<CelestialObject>().ShapeSettings.radius;
            float dx = transform.position.x - w.transform.position.x;
            float dy = transform.position.y - w.transform.position.y;
            float dz = transform.position.z - w.transform.position.z;
            float temp = Mathf.Sqrt(dx*dx + dy*dy + dz*dz);
            
            if(temp < radius)
                return true;
        }

        return false;
    }
}
