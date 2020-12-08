using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {
    [SerializeField, HideInInspector]
    MeshFilter[] mesh_filters;
    int mesh_filter_count;
    [SerializeField, HideInInspector]
    SphereMeshGenerator generator = new SphereMeshGenerator();

    // settings
    [Range(20, 1000000)]
    public int resolution = 50;
    public Material planet_material;
    public ShapeSettings shape_settings;
    public ColorSettings color_settings;

    [ContextMenu("initialize")]
    void initialize() {
        initialize_mesh_filters();
    }
    void initialize_mesh_filters() {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
            
        mesh_filters = new MeshFilter[mesh_filter_count];
        Mesh[] mesh_list = new Mesh[mesh_filter_count];
        for (int i = 0; i < mesh_filter_count; i++) {
            GameObject meshObj = new GameObject("Mesh");
            meshObj.transform.parent = transform;
            meshObj.AddComponent<MeshRenderer>().sharedMaterial = planet_material;

            mesh_filters[i] = meshObj.AddComponent<MeshFilter>();
            mesh_filters[i].sharedMesh = new Mesh();
            mesh_list[i] = mesh_filters[i].sharedMesh;
        }
        generator.set_target_mesh(mesh_list);
    }

    [ContextMenu("generate")]
    public void generate_planet() {
        initialize();
        generate_mesh();
        generate_color();
    }
    private void generate_mesh() {
        int group_count = generator.get_no_of_groups(resolution);
        if (group_count != mesh_filter_count) {
            mesh_filter_count = group_count;
            initialize_mesh_filters();
        }
        if (shape_settings == null) {
            Debug.Log("Shape settings not set!");
            return;
        }
        generator.construct_mesh(resolution, shape_settings);
    }
    private void generate_color() {
        if (color_settings == null) {
            Debug.Log("Color settings not set!");
            return;
        }
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
