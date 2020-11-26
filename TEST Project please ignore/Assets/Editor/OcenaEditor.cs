using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OceanSphere))]
public class OceanEditor : Editor {

    OceanSphere ocean;

    // sub-editors
    Editor shape_editor;

    private void OnEnable() {
        ocean = (OceanSphere) target;
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
            if (check.changed) ocean.generate_ocean();
        }

        draw_settings_editor(ref shape_editor, ocean.shape_settings, ocean.OnShapeSettingsUpdated);
    }
}