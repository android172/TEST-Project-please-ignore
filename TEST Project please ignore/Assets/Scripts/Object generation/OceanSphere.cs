using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanSphere : CelestialObject {

    [ContextMenu("generate")]
    public void generate_ocean() {
        initialize();
        OnShapeSettingsUpdated();
    }
}
