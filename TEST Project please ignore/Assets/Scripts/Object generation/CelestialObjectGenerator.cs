using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialObjectGenerator : MonoBehaviour {
    
    public enum COType {
        Asteroid,
        Moon,
        RockyDryPlanet,
        RockyWetPlanet,
        GasPlanet
    }

    public AsteroidShapeSettings DefaultAsteroidShapeSettings;
    public RockyPlanetShapeSettings DefaultMoonShapeSettings;
    public RockyPlanetShapeSettings DefaultRockyPlanetDryShapeSettings;
    public RockyPlanetShapeSettings DefaultRockyPlanetWetShapeSettings;
    public OceanShapeSettings DefaultOceanShapeSettings;

    [HideInInspector, SerializeField]
    Material SurfaceMaterial;
    [HideInInspector, SerializeField]
    Material OceanMaterial;

    [HideInInspector]
    public string ObjectName = "Celestial Object";
    [HideInInspector]
    public COType ObjectType = COType.Asteroid;
    [HideInInspector]
    [Min(0.5f)]
    public float ObjectRadius = 1f;

    // [ContextMenu("Generate")]
    public void generate_object() {
        GameObject celestial_body = new GameObject(ObjectName);

        // if object is has no surface
        if (ObjectType == COType.GasPlanet) {

        }
        // if object is solid
        else {
            GameObject surface = new GameObject("surface");
            // planet script
            Planet p = surface.AddComponent<Planet>();
            // resolution
            p.resolution = 1000000;
            // material
            p.planet_material = SurfaceMaterial;
            // shape
            switch (ObjectType) {
                case COType.Asteroid:
                    p.shape_settings = ScriptableObject.CreateInstance<AsteroidShapeSettings>();
                    p.shape_settings.set_settings(DefaultAsteroidShapeSettings);
                    break;
                case COType.Moon:
                    p.shape_settings = ScriptableObject.CreateInstance<RockyPlanetShapeSettings>();
                    p.shape_settings.set_settings(DefaultMoonShapeSettings);
                    break;
                case COType.RockyDryPlanet:
                    Debug.Log("BeenThere");
                    p.shape_settings = ScriptableObject.CreateInstance<RockyPlanetShapeSettings>();
                    p.shape_settings.set_settings(DefaultRockyPlanetDryShapeSettings);
                    break;
                case COType.RockyWetPlanet:
                    p.shape_settings = ScriptableObject.CreateInstance<RockyPlanetShapeSettings>();
                    p.shape_settings.set_settings(DefaultRockyPlanetWetShapeSettings);
                    break;
            }
            p.shape_settings.radius = ObjectRadius;
            p.shape_settings.randomize_seed();
            // color
            p.color_settings = null;
            // tag
            p.gameObject.tag = "Surface";
            // initialize
            p.generate_planet();

            surface.transform.SetParent(celestial_body.transform);

            // if object has an ocean
            if (ObjectType == COType.RockyWetPlanet) {
                GameObject ocean = new GameObject("ocean");
                // ocean script
                OceanSphere o = ocean.AddComponent<OceanSphere>();
                // resolution
                o.resolution = 50000;
                // material
                o.ocean_material = OceanMaterial;
                // shape
                o.shape_settings = ScriptableObject.CreateInstance<OceanShapeSettings>();
                o.shape_settings.set_settings(DefaultOceanShapeSettings);
                o.shape_settings.radius = ObjectRadius;
                o.shape_settings.randomize_seed();
                // tag
                o.gameObject.tag = "Ocean";
                // initialize
                o.generate_ocean();

                ocean.transform.SetParent(celestial_body.transform);
            }
        }
    }
}
