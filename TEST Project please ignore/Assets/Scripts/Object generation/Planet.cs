using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {
    bool initialized = false;
    [SerializeField, HideInInspector]
    MeshFilter[] mesh_filters;
    int mesh_filter_count;
    [SerializeField, HideInInspector]
    SphereMeshGenerator generator;

    // settings
    [Range(20, 65535)]
    public int resolution = 50;
    public RockyPlanetShapeSettings shape_settings;
    public ColorSettings color_settings;



    void initialize() {
        if (transform.Find("Mesh") != null) return;
        
        mesh_filter_count = resolution / 4000 + 1;
        mesh_filters = new MeshFilter[mesh_filter_count];
        Mesh[] mesh_list = new Mesh[mesh_filter_count];
        for (int i = 0; i < mesh_filter_count; i++) {
            GameObject meshObj = new GameObject("Mesh");
            meshObj.transform.parent = transform;
            meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));

            mesh_filters[i] = meshObj.AddComponent<MeshFilter>();
            mesh_filters[i].sharedMesh = new Mesh();
            mesh_list[i] = mesh_filters[i].sharedMesh;
        }

        generator = new SphereMeshGenerator(mesh_list);
    }

    public void generate_planet() {
        // if (!initialized) {
            initialize();
            initialized = true;
        // }
        generate_mesh();
        generate_color();
    }
    private void generate_mesh() {
        if (generator == null) {
            Mesh[] mesh_list = new Mesh[mesh_filter_count];
            for (int i = 0; i < mesh_filter_count; i++) {
                mesh_list[i] = mesh_filters[i].sharedMesh;
            }
            generator = new SphereMeshGenerator(mesh_list);
        }
        generator.construct_mesh(resolution, shape_settings);
    }
    private void generate_color() {
        for (int i = 0; i < mesh_filter_count; i++)
            mesh_filters[i].GetComponent<MeshRenderer>().sharedMaterial.color = color_settings.planet_color;
    }

    public void OnShapeSettingsUpdated() {
        generate_mesh();
    }
    public void OnColorSettingsUpdated() {
        generate_color();
    }
}
