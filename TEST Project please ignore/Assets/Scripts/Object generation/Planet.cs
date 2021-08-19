using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : CelestialObject {

    // settings
    public ColorSettings ColorSettings;

    [ContextMenu("generate")]
    public void generate_planet() {
        initialize();
        OnShapeSettingsUpdated();
        OnColorSettingsUpdated();
    }

    private void generate_color() {
        if (ColorSettings == null) {
            Debug.Log("Color settings not set!");
            return;
        }
        // mesh_filter.GetComponent<MeshRenderer>().materials[0].SetColor("BaseMap", color_settings.planet_color);
    }

    public void OnColorSettingsUpdated() {
        generate_color();
    }
}
