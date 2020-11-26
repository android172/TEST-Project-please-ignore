using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanSphere : MonoBehaviour {
    bool initialized = false;
    [SerializeField, HideInInspector]
    MeshFilter mesh_filter;
    public SphereMeshGenerator generator;

    // settings
    [Range(20, 65535)]
    public int resolution = 50;
    public OceanShapeSettings shape_settings;

    void initialize() {
        if (transform.Find("Mesh") != null) return;

        GameObject meshObj = new GameObject("Mesh");
        meshObj.transform.parent = transform;
        meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));

        mesh_filter = meshObj.AddComponent<MeshFilter>();
        mesh_filter.sharedMesh = new Mesh();

        // generator = new SphereMeshGenerator(mesh_filter.sharedMesh);
    }

    public void generate_ocean() {
        if (!initialized) {
            initialize();
            initialized = true;
        }
        generate_mesh();
    }
    private void generate_mesh() {
        if (generator == null) initialize();
        generator.construct_mesh(resolution, shape_settings);
    }

    public void OnShapeSettingsUpdated() {
        generate_mesh();
    }
}
