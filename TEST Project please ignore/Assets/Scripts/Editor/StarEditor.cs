using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StarSphere))]
public class StarEditor : Editor {

    StarSphere star = null;

    private void OnEnable() {
        star = (StarSphere) target;
    }

    public override void OnInspectorGUI() {

        using (var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
            if (check.changed) star.OnRadiusUpdate();
        }
    }
}