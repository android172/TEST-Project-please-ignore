using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanSphere : MonoBehaviour {
    [SerializeField, HideInInspector]
    private MeshFilter mesh_filter;

    // settings
    [Range(20, 50000)]
    public int resolution = 50;
    public Material ocean_material;
    public OceanShapeSettings shape_settings;

    [ContextMenu("initialize")]
    void initialize() {
        initialize_mesh_filter();
    }
    void initialize_mesh_filter() {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        
        GameObject meshObj = new GameObject("Mesh");
        meshObj.transform.parent = transform;
        meshObj.transform.localPosition = Vector3.zero;
        meshObj.AddComponent<MeshRenderer>().sharedMaterial = ocean_material;

        mesh_filter = meshObj.AddComponent<MeshFilter>();
        mesh_filter.sharedMesh = new Mesh();

        transform.localPosition = Vector3.zero;
    }

    [ContextMenu("generate")]
    public void generate_ocean() {
        initialize();
        generate_mesh();
    }

    private void generate_mesh() {
        if (shape_settings == null) {
            Debug.Log("Shape settings not set!");
            return;
        }
        SphereMeshGenerator.construct_mesh(mesh_filter.sharedMesh, resolution, shape_settings, false);
    }

    public void OnShapeSettingsUpdated() {
        generate_mesh();
    }
}
