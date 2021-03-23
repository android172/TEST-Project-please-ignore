using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {
    // components
    [SerializeField, HideInInspector]
    private MeshFilter mesh_filter;
    private MeshCollider sphere_collider;

    // settings
    [Range(20, 10000000)]
    public int resolution = 50;
    public bool slow_normals_calculation = true;
    public bool low_res_collider = true;
    public Material planet_material;
    public ShapeSettings shape_settings;
    public ColorSettings color_settings;

    

    [ContextMenu("initialize")]
    void initialize() {
        initialize_mesh_filter();
        initialize_collider();
    }

    private void initialize_mesh_filter() {
        // clear all sub meshes
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
            
        // initialize mesh filter
        GameObject meshObj = new GameObject("Mesh");
        meshObj.transform.parent = transform;
        meshObj.transform.localPosition = Vector3.zero;
        meshObj.AddComponent<MeshRenderer>().sharedMaterial = planet_material;

        mesh_filter = meshObj.AddComponent<MeshFilter>();
        mesh_filter.sharedMesh = new Mesh();
        mesh_filter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        transform.localPosition = Vector3.zero;
    }
    private void initialize_collider() {
        foreach (var cl in this.gameObject.GetComponents<Collider>())
            DestroyImmediate(cl);
        sphere_collider = this.gameObject.AddComponent<MeshCollider>();
        set_collider();
    }
    private void set_collider() {
        Mesh colider_mesh = new Mesh();
        colider_mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        int col_res = 1000000;
        if (low_res_collider)
            col_res = 500000;
        SphereMeshGenerator.construct_mesh(colider_mesh, col_res, shape_settings, false);
        sphere_collider.sharedMesh = colider_mesh;
    }

    [ContextMenu("generate")]
    public void generate_planet() {
        initialize();
        generate_mesh();
        generate_color();
    }
    private void generate_mesh() {
        if (shape_settings == null) {
            Debug.Log("Shape settings not set!");
            return;
        }
        SphereMeshGenerator.construct_mesh(mesh_filter.sharedMesh, resolution, shape_settings, slow_normals_calculation);
        set_collider();
    }
    private void generate_color() {
        if (color_settings == null) {
            Debug.Log("Color settings not set!");
            return;
        }
        mesh_filter.GetComponent<MeshRenderer>().materials[0].SetColor("BaseMap", color_settings.planet_color);
    }

    public void OnShapeSettingsUpdated() {
        generate_mesh();
    }
    public void OnColorSettingsUpdated() {
        generate_color();
    }
}
