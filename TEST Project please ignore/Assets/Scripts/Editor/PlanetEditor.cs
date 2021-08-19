using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : CelestialObjectEditor {

    // sub-editors
    Editor color_editor;

    private void OnEnable() {
        co = (Planet) target;
    }
    
    public override void OnInspectorGUI() {
        Planet planet = (Planet) co;

        using (var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
            if (check.changed) planet.generate_planet();
        }

        draw_settings_editor(ref color_editor, planet.ColorSettings, planet.OnColorSettingsUpdated);
    }
}