using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CelestialObject : MonoBehaviour {
    // components
    [SerializeField, HideInInspector]
    protected MeshFilter MeshFilter;
    private MeshCollider SphereCollider;
    public ShapeSettings ShapeSettings;

    // settings
    [Range(20, 10000000)]
    public int Resolution = 1000;
    public bool LowResCollider = true;
    public Material Material;

    [ContextMenu("initialize")]
    public void initialize() {
        initialize_mesh_filter();
        initialize_collider();
    }

    private void initialize_mesh_filter() {
        // clear all sub meshes
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
            
        // initialize mesh filter
        GameObject mesh_obj = new GameObject("Mesh");
        mesh_obj.transform.parent = transform;
        mesh_obj.transform.localPosition = Vector3.zero;
        mesh_obj.AddComponent<MeshRenderer>().sharedMaterial = Material;

        MeshFilter = mesh_obj.AddComponent<MeshFilter>();
        MeshFilter.sharedMesh = new Mesh();
        MeshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        transform.localPosition = Vector3.zero;
    }
    private void initialize_collider() {
        foreach (var cl in this.gameObject.GetComponents<Collider>())
            DestroyImmediate(cl);
        SphereCollider = this.gameObject.AddComponent<MeshCollider>();
        set_collider();
    }
    private void set_collider() {
        Mesh colider_mesh = new Mesh();
        colider_mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        int col_res = 1000000;
        if (LowResCollider)
            col_res = 500000;
        SphereMeshGenerator.construct_mesh(colider_mesh, col_res, ShapeSettings);
        SphereCollider.sharedMesh = colider_mesh;
    }

    private void generate_mesh() {
        if (ShapeSettings == null) {
            Debug.Log("Shape settings not set!");
            return;
        }
        SphereMeshGenerator.construct_mesh(MeshFilter.sharedMesh, Resolution, ShapeSettings);
        set_collider();
    }

    public void OnShapeSettingsUpdated() {
        generate_mesh();
    }

    public Vector3[] get_vertices() {
        return MeshFilter.sharedMesh.vertices;
    }
}
