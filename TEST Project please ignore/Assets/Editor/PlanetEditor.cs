using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor {

    Planet planet;

    // sub-editors
    Editor shape_editor;
    Editor color_editor;

    private void OnEnable() {
        planet = (Planet) target;
    }

    private void draw_settings_editor(ref Editor editor, Object settings, System.Action on_settings_updated) {
        if (settings == null) return;

        using (var check = new EditorGUI.ChangeCheckScope()) {
            EditorGUILayout.InspectorTitlebar(true, settings);
            CreateCachedEditor(settings, null, ref editor);
            editor.OnInspectorGUI();

            if (check.changed) on_settings_updated?.Invoke();
        }
    }

    public override void OnInspectorGUI() {
        using (var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
            if (check.changed) planet.generate_planet();
        }

        draw_settings_editor(ref shape_editor, planet.shape_settings, planet.OnShapeSettingsUpdated);
        draw_settings_editor(ref color_editor, planet.color_settings, planet.OnColorSettingsUpdated);
    }
}