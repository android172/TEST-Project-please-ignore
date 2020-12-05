using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeSettings : ScriptableObject {
    [System.Serializable]
    public struct NoiseSettings {
        public bool enable;
        [Range(1, 10)]
        public int number_of_layers;
        public float amplitude_fading;
        public float base_frequency;
        public float frequency_multiplier;
        public float strength;
        public float base_height;
        public Vector3 seed;
    }

    public ComputeShader noise_compute_shader;
    public float radius = 1f;

    public void OnValidate() {
        if (radius < .5f) radius = .5f;
    }

    public virtual Vector3[] apply_noise(Vector3[] vertices_in, int number_of_points) {
        return vertices_in;
    }
}
