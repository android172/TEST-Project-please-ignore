using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class OceanShapeSettings : ShapeSettings {
    // TODO: add custom settings

    public override Vector3[] apply_noise(Vector3[] vertices_in, int number_of_points) {
        Vector3[] vertices = (Vector3[]) vertices_in.Clone();
        int kernel_id = noise_compute_shader.FindKernel("WaveCompute");

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
        vertices_buf.GetData(vertices);

        // release buffers
        vertices_buf.Release();

        return vertices;
    }
}
