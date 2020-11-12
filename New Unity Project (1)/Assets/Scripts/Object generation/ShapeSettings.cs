using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject {
    [System.Serializable]
    public struct NoiseSettings {
        // Layering
        [Range(1,10)]
        public int number_of_layers;
        [Range(0.01f, 1f)]
        public float amplitude_fading;
        public float frequency_multiplier;

        // General
        public float strength;
        public float base_frequency;
        public Vector3 center;
    }

    public float radius = 1f;

    public ComputeShader noise_shader;

    public NoiseSettings[] noise_settings;


    public void OnValidate() {
        if (radius < .5f) radius = .5f;
        for (int i = 0; i < noise_settings.Length; i++) {
            if (noise_settings[i].frequency_multiplier < 1f) noise_settings[i].frequency_multiplier = 1f;
            if (noise_settings[i].strength < 0f) noise_settings[i].strength = 0f;
            if (noise_settings[i].base_frequency < 0f) noise_settings[i].base_frequency = 0f;
        }
    }
}
