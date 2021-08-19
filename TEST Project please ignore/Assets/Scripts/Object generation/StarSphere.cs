using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSphere : CelestialObject {
    
    // settings
    public float Radius = 10;

    public void OnRadiusUpdate() {
        if (ShapeSettings == null)
            ShapeSettings = ScriptableObject.CreateInstance<ShapeSettings>();
        ShapeSettings.radius = this.Radius;
        // OnShapeSettingsUpdated();
    }
}
