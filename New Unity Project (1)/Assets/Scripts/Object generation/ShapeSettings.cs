using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject {
    public float radius = 1f;

    public void OnValidate() {
        if (radius < .5f) radius = .5f;
    }
}
