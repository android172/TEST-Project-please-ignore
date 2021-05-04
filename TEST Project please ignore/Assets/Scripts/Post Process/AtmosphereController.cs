using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class AtmosphereController : MonoBehaviour
{
    public Transform sun;
    public Transform closestPlanet;

    private Atmosphere atmo;

    void Start()
    {
        Volume vol = GetComponent<Volume>();
        atmo = vol.profile.components.Find(x => x is Atmosphere) as Atmosphere;
    }

    // Update is called once per frame
    void Update()
    {
        atmo.planetPosition.value = closestPlanet.position;
        atmo.directionToSun.value = sun.position - closestPlanet.position;
        //atmo.sunPosition.value = sun.position;
    }
}
