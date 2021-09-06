using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialObjectGenerator : MonoBehaviour {
    
    public enum COType {
        Asteroid,
        Moon,
        RockyDryPlanet,
        RockyWetPlanet,
        GasPlanet,
        Star
    }

    public AsteroidShapeSettings DefaultAsteroidShapeSettings;
    public RockyPlanetShapeSettings DefaultMoonShapeSettings;
    public RockyPlanetShapeSettings DefaultRockyPlanetDryShapeSettings;
    public RockyPlanetShapeSettings DefaultRockyPlanetWetShapeSettings;
    public OceanShapeSettings DefaultOceanShapeSettings;

    [SerializeField]
    Material SurfaceMaterial;
    [SerializeField]
    Material OceanMaterial;
    [SerializeField]
    Material StarMaterial;

    public string ObjectName = "Celestial Object";
    public COType ObjectType = COType.Asteroid;
    [Min(0.5f)]
    public float ObjectRadius = 1f;

    private int SphereResolution = 1000000;

    public void generate_object() {
        GameObject celestial_body = new GameObject(ObjectName);

        GameObject surface = new GameObject("surface");
        surface.transform.SetParent(celestial_body.transform);

        // if object is has no surface
        if (ObjectType == COType.Star) {
            // star script
            StarSphere starS = surface.AddComponent<StarSphere>();
            // resolution
            starS.Resolution = 25000;
            // material
            starS.Material = StarMaterial;
            // radius
            starS.Radius = this.ObjectRadius;
            starS.OnRadiusUpdate();
            // tag
            starS.gameObject.tag = "StarSurface";
            // initialize
            starS.initialize();
            starS.OnShapeSettingsUpdated();

            // light
            GameObject light = Instantiate<GameObject>(Resources.Load<GameObject>("Starlight"));
            light.transform.SetParent(celestial_body.transform);
            light.name = ObjectName + " " + "Light";

            return;
        }
        if (ObjectType == COType.GasPlanet) {
            return;
        }

        // if object is solid
        // planet script
        Planet planetS = surface.AddComponent<Planet>();
        // resolution
        planetS.Resolution = SphereResolution;
        // material
        planetS.Material = SurfaceMaterial;
        // shape
        switch (ObjectType) {
            case COType.Asteroid:
                planetS.ShapeSettings = ScriptableObject.CreateInstance<AsteroidShapeSettings>();
                planetS.ShapeSettings.set_settings(DefaultAsteroidShapeSettings);
                break;
            case COType.Moon:
                planetS.ShapeSettings = ScriptableObject.CreateInstance<RockyPlanetShapeSettings>();
                planetS.ShapeSettings.set_settings(DefaultMoonShapeSettings);
                break;
            case COType.RockyDryPlanet:
                planetS.ShapeSettings = ScriptableObject.CreateInstance<RockyPlanetShapeSettings>();
                planetS.ShapeSettings.set_settings(DefaultRockyPlanetDryShapeSettings);
                break;
            case COType.RockyWetPlanet:
                planetS.ShapeSettings = ScriptableObject.CreateInstance<RockyPlanetShapeSettings>();
                planetS.ShapeSettings.set_settings(DefaultRockyPlanetWetShapeSettings);
                break;
        }
        planetS.ShapeSettings.radius = ObjectRadius;
        planetS.ShapeSettings.randomize_seed();
        // color
        planetS.ColorSettings = null;
        // tag
        planetS.gameObject.tag = "Surface";
        // initialize
        planetS.generate_planet();

        // if object has an ocean
        if (ObjectType == COType.RockyWetPlanet) {
            GameObject ocean = new GameObject("ocean");
            // ocean script
            OceanSphere oceanS = ocean.AddComponent<OceanSphere>();
            // resolution
            oceanS.Resolution = SphereResolution;
            // material
            oceanS.Material = OceanMaterial;
            // shape
            oceanS.ShapeSettings = ScriptableObject.CreateInstance<OceanShapeSettings>();
            oceanS.ShapeSettings.set_settings(DefaultOceanShapeSettings);
            oceanS.ShapeSettings.radius = ObjectRadius;
            oceanS.ShapeSettings.randomize_seed();
            // tag
            oceanS.gameObject.tag = "Ocean";
            // initialize
            oceanS.generate_ocean();

            // Set colors
            oceanS.set_mesh_wave_color_mask(planetS.get_vertices(), 8);

            ocean.transform.SetParent(celestial_body.transform);
        }
    }
}
