using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanSphere : CelestialObject {

    [ContextMenu("generate")]
    public void generate_ocean() {
        initialize();
        OnShapeSettingsUpdated();
    }

    public void set_mesh_wave_color_mask(Vector3[] vertices, float max_depth) {
        Color[] colors = new Color[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++) {
            float zone = ShapeSettings.radius - vertices[i].magnitude;
            zone /= max_depth;
            colors[i] = new Color(Mathf.Clamp01(zone), 0, 0);
        }

        MeshFilter.sharedMesh.colors = colors;
    }
}
