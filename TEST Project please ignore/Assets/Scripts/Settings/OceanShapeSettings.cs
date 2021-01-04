using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class OceanShapeSettings : ShapeSettings {
    // TODO: add custom settings

    public override void set_settings(ShapeSettings settings_in) {
        if (!(settings_in is OceanShapeSettings)) throw new UnityException("Error in :: ShapeSettings :: set_settings :: cannot set settings to the settings of wrong type.");
        OceanShapeSettings settings = (OceanShapeSettings) settings_in;
        base.set_settings(settings);
        // TODO: same
    }

    public override void randomize_seed() {
        base.randomize_seed();
    }

    public override Vector3[] apply_noise(Vector3[] vertices_in, int number_of_points) {
        Vector3[] vertices = (Vector3[])vertices_in.Clone();

        // // calculate each points height
        float[] heights = new float[number_of_points];
        int kernel_id = noise_compute_shader.FindKernel("WaveCompute");

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
}
