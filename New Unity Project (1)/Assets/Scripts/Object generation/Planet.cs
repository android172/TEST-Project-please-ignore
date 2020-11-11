using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    bool first = true;
    [SerializeField, HideInInspector]
    MeshFilter mesh_filter;

    // settings
    [Range(20, 20000)]
    public int resolution = 50;
    public ShapeSettings shape_settings;
    public ColorSettings color_settings;


    private void OnValidate() {
        if (first) {
            initialize();
            first = false;
        }
        generate_mesh();
        generate_color();
    }

    public void OnShapeSettingsUpdated() {
        generate_mesh();
    }
    public void OnColorSettingsUpdated() {
        generate_color();
    }

    void initialize() {
        
        if (transform.Find("Mesh") != null) return;

        GameObject meshObj = new GameObject("Mesh");
        meshObj.transform.parent = transform;
        meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));

        mesh_filter = meshObj.AddComponent<MeshFilter>();
        mesh_filter.sharedMesh = new Mesh();
    }

    private void generate_mesh() {
        SphereMeshGenerator.construct_mesh(mesh_filter.sharedMesh, resolution, shape_settings.radius);
    }
    private void generate_color() {
        mesh_filter.GetComponent<MeshRenderer>().sharedMaterial.color = color_settings.planet_color;
    }
}
