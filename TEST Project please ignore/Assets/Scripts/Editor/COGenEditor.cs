using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CelestialObjectGenerator))]
public class COGenEditor : Editor {
    CelestialObjectGenerator COG;
    SerializedObject COG_serialized;

    private void OnEnable() {
        COG = (CelestialObjectGenerator) target;
        COG_serialized = new SerializedObject(COG);
    }

    public override void OnInspectorGUI() {
        EditorGUILayout.LabelField("Default Settings");
        base.OnInspectorGUI();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Materials Used");
        if (COG.ObjectType != CelestialObjectGenerator.COType.GasPlanet) {
            EditorGUILayout.PropertyField(COG_serialized.FindProperty("SurfaceMaterial"), true);
            if (COG.ObjectType == CelestialObjectGenerator.COType.RockyWetPlanet)
                EditorGUILayout.PropertyField(COG_serialized.FindProperty("OceanMaterial"), true);
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Object Settings");

        EditorGUILayout.PropertyField(COG_serialized.FindProperty("ObjectName"), true);
        EditorGUILayout.PropertyField(COG_serialized.FindProperty("ObjectType"), true);
        EditorGUILayout.PropertyField(COG_serialized.FindProperty("ObjectRadius"), true);
        
        if (GUILayout.Button("Generate")) {
            COG.generate_object();
        }

        COG_serialized.ApplyModifiedProperties();
    }
}