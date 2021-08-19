﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OceanSphere))]
public class OceanEditor : CelestialObjectEditor {

    private void OnEnable() {
        co = (OceanSphere) target;
    }

    public override void OnInspectorGUI() {
        OceanSphere ocean = (OceanSphere) co;

        using (var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
            if (check.changed) ocean.generate_ocean();
        }
    }
}