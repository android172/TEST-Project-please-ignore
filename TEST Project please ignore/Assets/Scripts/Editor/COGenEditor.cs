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
        switch (COG.ObjectType) {
            case CelestialObjectGenerator.COType.Asteroid:
                EditorGUILayout.PropertyField(COG_serialized.FindProperty("DefaultAsteroidShapeSettings"), true);
                break;
            case CelestialObjectGenerator.COType.Moon:
                EditorGUILayout.PropertyField(COG_serialized.FindProperty("DefaultMoonShapeSettings"), true);
                break;
            case CelestialObjectGenerator.COType.RockyDryPlanet:
                EditorGUILayout.PropertyField(COG_serialized.FindProperty("DefaultRockyPlanetDryShapeSettings"), true);
                break;
            case CelestialObjectGenerator.COType.RockyWetPlanet:
                EditorGUILayout.PropertyField(COG_serialized.FindProperty("DefaultRockyPlanetWetShapeSettings"), true);
                EditorGUILayout.PropertyField(COG_serialized.FindProperty("DefaultOceanShapeSettings"), true);
                break;
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Materials Used");
        if (COG.ObjectType == CelestialObjectGenerator.COType.GasPlanet) {
        }
        else if (COG.ObjectType == CelestialObjectGenerator.COType.Star) {
            EditorGUILayout.PropertyField(COG_serialized.FindProperty("StarMaterial"), true);
        }
        else {
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