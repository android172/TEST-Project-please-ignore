using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSettings : MonoBehaviour {
    
    [Header("General")]
    [Min(0)]
    public Vector3 BoxDimensions = new Vector4(1, 1, 1);
    [Min(0)]
    public float CloudDensityFactor = 1f;
    [SerializeField, Range(0, 100)]
    private float coverage;
    public float Coverage {
        get => coverage / 200 - 0.3f;
        set => coverage = value;
    }

    // Lighting
    [Header("Lighting Settings")]
    [Range(-1,1)]
    public float ForwardsScattering = 0.9f;
    [Range(-1,1)]
    public float BackwardsScattering = 0.1f;
    [Min(0)]
    public float BaseBrightness = 0f;
    [Min(0)]
    public float PhaseFactor = 1f;
    public Vector4 PhaseParametars {
        get => new Vector4(ForwardsScattering, BackwardsScattering, BaseBrightness, PhaseFactor);
    }

    // Sun
    [Header("Sun Settings")]
    public Vector3 SunPosition = new Vector3(100, 200, 100);
    public Color SunColor = new Color(1, 1, 1);
}
