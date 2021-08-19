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

        public NoiseSettings(NoiseSettings settings) {
            enable = settings.enable;
            number_of_layers = settings.number_of_layers;
            amplitude_fading = settings.amplitude_fading;
            base_frequency = settings.base_frequency;
            frequency_multiplier = settings.frequency_multiplier;
            strength = settings.strength;
            base_height = settings.base_height;
            seed.x = settings.seed.x;
            seed.y = settings.seed.y;
            seed.z = settings.seed.z;
        }

        private static System.Random r = new System.Random();
        public void randomize_seed() {
            seed.x = rand_to_float(r.NextDouble(), r.Next(15));
            seed.y = rand_to_float(r.NextDouble(), r.Next(15));
            seed.z = rand_to_float(r.NextDouble(), r.Next(15));
        }
        private float rand_to_float(double mantissa, int exponent) {
            double mn = 2.0 * mantissa - 1.0;
            double ex = System.Math.Pow(2.0, exponent);
            return (float) (mn * ex);
        }
    }

    [System.Serializable]
    public class MNoiseSettings : NoiseSettings {
        public float power = 1f;
        [Range(0,1)]
        public float gain = 1f;

        public MNoiseSettings(MNoiseSettings settings) : base(settings) {
            power = settings.power;
            gain = settings.gain;
        }
    }

    [System.Serializable]
    public class UMNoiseSettings : MNoiseSettings {
        public float offset;

        public UMNoiseSettings(UMNoiseSettings settings) : base(settings) {
            offset = settings.offset;
        }
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

        public CraterNoiseSettings(CraterNoiseSettings settings) : base(settings) {
            depth = settings.depth;
            radius = settings.radius;
            slope = settings.slope;
            central_elevation_height = settings.central_elevation_height;
            central_elevation_width = settings.central_elevation_width;
            outside_slope = settings.outside_slope;
            jitter = settings.jitter;
        }
    }

    public ComputeShader noise_compute_shader;

    [Min(0.5f)]
    public float radius = 1f;

    public virtual void set_settings(ShapeSettings settings) {
        noise_compute_shader = settings.noise_compute_shader;
        radius = settings.radius;
    }
    public virtual void randomize_seed() {}
    public virtual Vector3[] apply_noise(Vector3[] vertices_in, int number_of_points) {
        Vector3[] vertices = (Vector3[])vertices_in.Clone();
        for (int i = 0; i < number_of_points; i++)
            vertices[i] *= radius;
        return vertices;
    }
}
