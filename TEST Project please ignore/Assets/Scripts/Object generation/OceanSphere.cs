using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanSphere : MonoBehaviour {
    [SerializeField, HideInInspector]
    MeshFilter[] mesh_filters;
    int mesh_filter_count;
    [SerializeField, HideInInspector]
    SphereMeshGenerator generator = new SphereMeshGenerator();

    // settings
    [Range(20, 50000)]
    public int resolution = 50;
    public Material ocean_material;
    public OceanShapeSettings shape_settings;

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
            meshObj.AddComponent<MeshRenderer>().sharedMaterial = ocean_material;

            mesh_filters[i] = meshObj.AddComponent<MeshFilter>();
            mesh_filters[i].sharedMesh = new Mesh();
            mesh_list[i] = mesh_filters[i].sharedMesh;
        }
        generator.set_target_mesh(mesh_list);
    }

    [ContextMenu("generate")]
    public void generate_ocean() {
        initialize();
        generate_mesh();
    }

    private void generate_mesh() {
        int group_count = generator.get_no_of_groups(resolution);
        if (group_count != mesh_filter_count) {
            mesh_filter_count = group_count;
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
            initialize_mesh_filters();
        }
        if (shape_settings == null) {
            Debug.Log("Shape settings not set!");
            return;
        }
        generator.construct_mesh(resolution, shape_settings);
    }

    public void OnShapeSettingsUpdated() {
        generate_mesh();
    }
}
