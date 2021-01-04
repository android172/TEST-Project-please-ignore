using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AsteroidShapeSettings : ShapeSettings {
    // Asteroid shape
    public NoiseSettings shape_noise;
    // general noise
    public NoiseSettings general_noise;
    // craters
    public CraterNoiseSettings crater_noise;

    // Constructors
    public override void set_settings(ShapeSettings settings_in) {
        if (!(settings_in is AsteroidShapeSettings)) throw new UnityException("Error in :: ShapeSettings :: set_settings :: cannot set settings to the settings of wrong type.");
        AsteroidShapeSettings settings = (AsteroidShapeSettings) settings_in;
        base.set_settings(settings);
        shape_noise = new NoiseSettings(settings.shape_noise);
        general_noise = new NoiseSettings(settings.general_noise);
        crater_noise = new CraterNoiseSettings(settings.crater_noise);
    }

    public override void randomize_seed() {
        base.randomize_seed();
        shape_noise.randomize_seed();
        general_noise.randomize_seed();
        crater_noise.randomize_seed();
    }

    public override Vector3[] apply_noise(Vector3[] vertices_in, int number_of_points) {
        Vector3[] vertices = (Vector3[])vertices_in.Clone();

        // // calculate each points height
        float[] heights = new float[number_of_points];
        int kernel_id = noise_compute_shader.FindKernel("AsteroidShapeCompute");

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
        noise_compute_shader.SetFloats("noise_settings_shape", get_shape_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_general", get_general_noise_settings());
        noise_compute_shader.SetFloats("noise_settings_crater", get_crater_noise_settings());

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
            shape_noise.enable? 1 : 0,
            general_noise.enable? 1 : 0,
            crater_noise.enable? 1 : 0
        };
    }

    float[] get_shape_noise_settings() {
        return new float[] {
            shape_noise.number_of_layers,
            shape_noise.amplitude_fading,
            shape_noise.base_frequency,
            shape_noise.frequency_multiplier,
            shape_noise.strength,
            shape_noise.base_height,
            shape_noise.seed.x,
            shape_noise.seed.y,
            shape_noise.seed.z
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
