using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    bool initialized = false;
    [SerializeField, HideInInspector]
    MeshFilter mesh_filter;
    public SphereMeshGenerator generator;

    // settings
    [Range(20, 65535)]
    public int resolution = 50;
    public ShapeSettings shape_settings;
    public ColorSettings color_settings;



    void initialize() {
        if (transform.Find("Mesh") != null) return;

        GameObject meshObj = new GameObject("Mesh");
        meshObj.transform.parent = transform;
        meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));

        mesh_filter = meshObj.AddComponent<MeshFilter>();
        mesh_filter.sharedMesh = new Mesh();

        generator = new SphereMeshGenerator(mesh_filter.sharedMesh);
    }

    public void generate_planet() {
        if (!initialized) {
            initialize();
            initialized = true;
        }
        generate_mesh();
        generate_color();
    }
    private void generate_mesh() {
        if (generator == null) generator = new SphereMeshGenerator(mesh_filter.sharedMesh);
        generator.construct_mesh(resolution, shape_settings);
    }
    private void generate_color() {
        mesh_filter.GetComponent<MeshRenderer>().sharedMaterial.color = color_settings.planet_color;
    }

    public void OnShapeSettingsUpdated() {
        generate_mesh();
    }
    public void OnColorSettingsUpdated() {
        generate_color();
    }
}
