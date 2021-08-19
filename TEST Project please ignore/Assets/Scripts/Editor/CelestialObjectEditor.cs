using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CelestialObject))]
public class CelestialObjectEditor : Editor {

    protected CelestialObject co;

    // sub-editors
    Editor shape_editor;

    private void OnEnable() {
        co = (CelestialObject) target;
    }

    protected void draw_settings_editor(ref Editor editor, Object settings, System.Action on_settings_updated) {
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
        }

        draw_settings_editor(ref shape_editor, co.ShapeSettings, co.OnShapeSettingsUpdated);
    }
}