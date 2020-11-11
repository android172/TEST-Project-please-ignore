using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor {

    Planet planet;

    private void OnEnable() {
        planet = (Planet) target;
    }

    private void draw_settings_editor(Object settings, System.Action on_settings_updated) {
        using (var check = new EditorGUI.ChangeCheckScope()) {
            EditorGUILayout.InspectorTitlebar(true, settings);
            Editor editor = CreateEditor(settings);
            editor.OnInspectorGUI();

            if (check.changed) on_settings_updated?.Invoke();
        }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        draw_settings_editor(planet.shape_settings, planet.OnShapeSettingsUpdated);
        draw_settings_editor(planet.color_settings, planet.OnColorSettingsUpdated);
    }
}
