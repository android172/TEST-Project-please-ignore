using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeSettings : ScriptableObject {
    [System.Serializable]
    public class NoiseSettings {
        public bool enable;
        [Range(1, 10)]
        public int number_of_layers = 1;
        [Min(0f)]
        public float amplitude_fading;
        [Min(0f)]
        public float base_frequency = 1f;
        [Min(0f)]
        public float frequency_multiplier;
        public float strength = 1f;
        public float base_height;
        public Vector3 seed;
    }

    [System.Serializable]
    public class MNoiseSettings : NoiseSettings {
        public float power = 1f;
        [Range(0,1)]
        public float gain = 1f;
    }

    [System.Serializable]
    public class UMNoiseSettings : MNoiseSettings {
        public float offset;
    }

    [System.Serializable]
    public class CraterNoiseSettings : NoiseSettings {
        [Range(0, 1)]
        public float depth = 0.5f;
        [Min(1f)]
        public float radius = 2f;
        [Min(1)]
        public int slope = 12;
        [Min(0f)]
        public float central_elevation_height;
        [Min(0f)]
        public float central_elevation_width;
        [Min(0f)]
        public float outside_slope;
        public float jitter;
    }

    public ComputeShader noise_compute_shader;

    [Min(0.5f)]
    public float radius;

    // public float radius = 1f;

    public virtual Vector3[] apply_noise(Vector3[] vertices_in, int number_of_points) {
        return vertices_in;
    }
}
