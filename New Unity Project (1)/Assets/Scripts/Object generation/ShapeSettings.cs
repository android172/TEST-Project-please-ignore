using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject {
    [System.Serializable]
    public struct NoiseSettings {
        [Range(1, 10)]
        public int number_of_layers;
        public float amplitude_fading;
        public float base_frequency;
        public float frequency_multiplier;
        public float strength;
        public float base_height;
        public Vector3 seed;
    }

    public float radius = 1f;
    public ComputeShader noise_compute_shader;

    // Continent shape
    [Range(0, 1)]
    public float continent_ratio;
    [Range(0, 1)]
    public float ocean_depth;
    [Range(0, 1)]
    public float flatness;
    // continent noise settings
    public NoiseSettings continent_noise;
    // flatness noise
    public NoiseSettings flatness_noise;
    // general noise
    public NoiseSettings general_noise;
    // mountains
    public NoiseSettings mountains_noise;
    public float power;
    [Range(0,1)]
    public float gain;
    // underwater mountains
    public NoiseSettings underwater_mountains_noise;
    public float u_power;
    [Range(0, 1)]
    public float u_gain;
    public float offset;

    public void OnValidate() {
        if (radius < .5f) radius = .5f;
    }

    public float[] get_continent_noise_settings() {
        return new float[] {
            continent_noise.number_of_layers,
            continent_noise.amplitude_fading,
            continent_noise.base_frequency,
            continent_noise.frequency_multiplier,
            continent_noise.strength,
            continent_noise.base_height,
            continent_noise.seed.x,
            continent_noise.seed.y,
            continent_noise.seed.z
        };
    }
    public float[] get_flatness_noise_settings() {
        return new float[] {
            flatness_noise.number_of_layers,
            flatness_noise.amplitude_fading,
            flatness_noise.base_frequency,
            flatness_noise.frequency_multiplier,
            flatness_noise.strength,
            flatness_noise.base_height,
            flatness_noise.seed.x,
            flatness_noise.seed.y,
            flatness_noise.seed.z
        };
    }
    public float[] get_general_noise_settings() {
        return new float[] {
            general_noise.number_of_layers,
            general_noise.amplitude_fading,
            general_noise.base_frequency,
            general_noise.frequency_multiplier,
            general_noise.strength,
            general_noise.base_height,
            general_noise.seed.x,
            general_noise.seed.y,
            general_noise.seed.z
        };
    }
    public float[] get_mountains_noise_settings()
    {
        return new float[] {
            mountains_noise.number_of_layers,
            mountains_noise.amplitude_fading,
            mountains_noise.base_frequency,
            mountains_noise.frequency_multiplier,
            mountains_noise.strength,
            mountains_noise.base_height,
            mountains_noise.seed.x,
            mountains_noise.seed.y,
            mountains_noise.seed.z,
            power,
            gain
        };
    }
    public float[] get_underwater_mountains_noise_settings() {
        return new float[] {
            underwater_mountains_noise.number_of_layers,
            underwater_mountains_noise.amplitude_fading,
            underwater_mountains_noise.base_frequency,
            underwater_mountains_noise.frequency_multiplier,
            underwater_mountains_noise.strength,
            underwater_mountains_noise.base_height,
            underwater_mountains_noise.seed.x,
            underwater_mountains_noise.seed.y,
            underwater_mountains_noise.seed.z,
            u_power,
            u_gain,
            offset
        };
    }
}
