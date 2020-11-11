using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    bool first = false;

    [Range(20, 20000)]
    public int resolution = 50;

    [SerializeField, HideInInspector]
    MeshFilter mesh_filter;

    private void OnValidate() {
        initialize();
        generate();
    }

    void initialize() {
        if (first) return;
        first = true;
        GameObject meshObj = new GameObject("Mesh");
        meshObj.transform.parent = transform;
        meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));

        mesh_filter = meshObj.AddComponent<MeshFilter>();
        mesh_filter.sharedMesh = new Mesh();
    }

    void generate() {
        SphereMeshGenerator.construct_mesh(mesh_filter.sharedMesh, resolution);
    }
}
