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
    public MNoiseSettings mountains_noise;
    // underwater mountains
    public UMNoiseSettings underwater_mountains_noise;
    // craters
    public CraterNoiseSettings crater_noise;


    public override Vector3[] apply_noise(Vector3[] vertices_in, int number_of_points) {
        Vector3[] vertices = (Vector3[])vertices_in.Clone();

        // // calculate each points height
        float[] heights = new float[number_of_points];
        int kernel_id = noise_compute_shader.FindKernel("CSMain");

        // send heights
        ComputeBuffer heights_buf = new ComputeBuffer(number_of_points, sizeof(float));
        heights_buf.SetData(heights);
        noise_compute_shader.SetBuffer(kernel_id, "heights", heights_buf);

        // send vertices
        ComputeBuffer vertices_buf = new ComputeBuffer(number_of_points, 3 * sizeof(float));
        vertices_buf.SetData(vertices);
        noise_compute_shader.SetBuffer(kernel_id, "vertices", vertices_buf);

        // send number of vertices and radius
        noise_compute_shader.SetInt("num_of_vertices", number_of_points);
        noise_compute_shader.SetFloat("radius", radius);

        // send noise settings
        noise_compute_shader.SetInts("enabled", get_enables());
        noise_compute_shader.SetFloats("noise_settings_continent_shape", get_continent_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_flatness", get_flatness_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_both", get_general_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_mountains", get_mountains_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_ocean_mountains", get_underwater_mountains_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_crater", get_crater_noise_settings());

        // set continent height
        float continent_base = (1f - continent_ratio * 2f) * continent_noise.strength + continent_noise.base_height;
        noise_compute_shader.SetFloat("continent_base", continent_base);
        // set ocean depth
        float ocean_depth = continent_base - (continent_base + continent_noise.strength - continent_noise.base_height) * this.ocean_depth;
        noise_compute_shader.SetFloat("ocean_depth", ocean_depth);
        // set flatness
        float flatness_ratio = (flatness * 2f - 1f) * flatness_noise.strength + flatness_noise.base_height;
        noise_compute_shader.SetFloat("flatness_ratio", flatness_ratio);

        // run
        noise_compute_shader.Dispatch(kernel_id, 1024, 1, 1);

        // return data
        heights_buf.GetData(heights);

        // release buffers
        heights_buf.Release();
        vertices_buf.Release();

        for (int i = 0; i < number_of_points; i++)
            vertices[i] *= heights[i];
        return vertices;
    }

    int[] get_enables() {
        return new int[] {
            continent_noise.enable? 1 : 0,
            general_noise.enable? 1 : 0,
            mountains_noise.enable? 1 : 0,
            underwater_mountains_noise.enable? 1 : 0,
            flatness_noise.enable? 1 : 0,
            crater_noise.enable? 1 : 0
        };
    }

    float[] get_continent_noise_settings() {
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
    float[] get_flatness_noise_settings() {
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
    float[] get_general_noise_settings() {
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
    float[] get_mountains_noise_settings() {
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
            mountains_noise.power,
            mountains_noise.gain
        };
    }
    float[] get_underwater_mountains_noise_settings() {
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
            underwater_mountains_noise.power,
            underwater_mountains_noise.gain,
            underwater_mountains_noise.offset
        };
    }

    float[] get_crater_noise_settings() {
        return new float[] {
            crater_noise.number_of_layers,
            crater_noise.depth,
            crater_noise.amplitude_fading,
            crater_noise.base_frequency,
            crater_noise.frequency_multiplier,
            crater_noise.radius,
            crater_noise.slope,
            crater_noise.central_elevation_height,
            crater_noise.central_elevation_width,
            crater_noise.outside_slope,
            crater_noise.jitter,
            crater_noise.strength,
            crater_noise.base_height,
            crater_noise.seed.x,
            crater_noise.seed.y,
            crater_noise.seed.z
        };
    }
}