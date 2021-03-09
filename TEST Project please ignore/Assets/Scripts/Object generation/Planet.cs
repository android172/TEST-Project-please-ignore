using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {
    // components
    [SerializeField, HideInInspector]
    private MeshFilter[] mesh_filters;
    private Mesh[] mesh_list;
    private MeshCollider sphere_collider;

    // settings
    [Range(20, 10000000)]
    public int resolution = 50;
    public bool slow_normals_calculation = true;
    public Material planet_material;
    public ShapeSettings shape_settings;
    public ColorSettings color_settings;

    

    [ContextMenu("initialize")]
    void initialize() {
        initialize_mesh_filters();
        initialize_collider();
    }

    private void initialize_mesh_filters() {
        // clear all sub meshes
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
            
        // initialize mesh filters
        int mesh_filter_count = SphereMeshGenerator.get_no_of_groups(resolution);
        mesh_filters = new MeshFilter[mesh_filter_count];
        mesh_list = new Mesh[mesh_filter_count];
        for (int i = 0; i < mesh_filter_count; i++) {
            GameObject meshObj = new GameObject("SubMesh" + (i + 1));
            meshObj.transform.parent = transform;
            meshObj.transform.localPosition = Vector3.zero;
            meshObj.AddComponent<MeshRenderer>().sharedMaterial = planet_material;

            mesh_filters[i] = meshObj.AddComponent<MeshFilter>();
            mesh_filters[i].sharedMesh = new Mesh();
            mesh_list[i] = mesh_filters[i].sharedMesh;
        }
        transform.localPosition = Vector3.zero;
    }
    private void initialize_collider() {
        foreach (var cl in this.gameObject.GetComponents<Collider>())
            DestroyImmediate(cl);
        sphere_collider = this.gameObject.AddComponent<MeshCollider>();
        set_collider();
    }
    private void set_collider() {
        Mesh[] m_list = new Mesh[] {new Mesh()};
        SphereMeshGenerator.construct_mesh(m_list, 50000, shape_settings, false);
        sphere_collider.sharedMesh = m_list[0];
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
        SphereMeshGenerator.construct_mesh(mesh_list, resolution, shape_settings, slow_normals_calculation);
        set_collider();
    }
    private void generate_color() {
        if (color_settings == null) {
            Debug.Log("Color settings not set!");
            return;
        }
        for (int i = 0; i < mesh_filters.Length; i++)
            mesh_filters[i].GetComponent<MeshRenderer>().sharedMaterial.color = color_settings.planet_color;
    }

    public void OnShapeSettingsUpdated() {
        generate_mesh();
    }
    public void OnColorSettingsUpdated() {
        generate_color();
    }
}
