using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RockyPlanetShapeSettings : ShapeSettings {
    // Continent shape
    [Range(0, 1)]
    public float continent_ratio = 0;
    [Range(0, 1)]
    public float ocean_depth = 1;
    [Range(0, 1)]
    public float flatness = 0;
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


    public override Vector3[] apply_noise(Vector3[] vertices_in, int number_of_points) {
        // // calculate each points height
        Vector3[] vertices = (Vector3[]) vertices_in.Clone();
        int kernel_id = noise_compute_shader.FindKernel("CSMain");

        // send vertices
        ComputeBuffer vertices_buf = new ComputeBuffer(number_of_points, 3 * sizeof(float));
        vertices_buf.SetData(vertices);
        noise_compute_shader.SetBuffer(kernel_id, "vertices", vertices_buf);

        // send number of vertices and radius
        noise_compute_shader.SetInt("num_of_vertices", number_of_points);
        noise_compute_shader.SetFloat("radius", radius);

        // send noise settings
        noise_compute_shader.SetFloats("noise_settings_continent_shape", get_continent_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_flatness", get_flatness_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_both", get_general_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_mountains", get_mountains_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_ocean_mountains", get_underwater_mountains_noise_settings());

        // set continent height
        float continent_base = (1f - continent_ratio * 2f) * continent_noise.strength + continent_noise.base_height;
        noise_compute_shader.SetFloat("continent_base", continent_base);
        // set ocean depth
        float ocean_depth = continent_base - (continent_base + continent_noise.strength - continent_noise.base_height) * this.ocean_depth;
        noise_compute_shader.SetFloat("ocean_depth", ocean_depth);
        // set flatness
        noise_compute_shader.SetFloat("flatness_ratio", flatness);

        // run
        noise_compute_shader.Dispatch(kernel_id, 1024, 1, 1);

        // return data
        vertices_buf.GetData(vertices);

        // release buffers
        vertices_buf.Release();

        return vertices;
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