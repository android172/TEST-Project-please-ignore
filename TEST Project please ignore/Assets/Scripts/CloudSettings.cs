using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSettings : MonoBehaviour {
    
    [Header("General")]
    public float scale = 1;
    public Vector3 Location = Vector3.zero;
    [Min(0)]
    public float Radius = 1;
    public Vector4 GetShapeParametars {
        get => new Vector4(Location.x, Location.y, Location.z, Radius);
    }
    [Min(0)]
    public float CloudDensityFactor = 1f;
    [SerializeField, Range(0, 100)]
    private float coverage;
    public float Coverage {
        get => coverage / 200 - 0.3f;
        set => coverage = value;
    }

    // Lighting
    [Header("Lighting Settings")]
    [Range(0,1)]
    public float DarknessThreshold = 0.1f;
    [Range(-1,1)]
    public float ForwardsScattering = 0.9f;
    [Range(-1,1)]
    public float BackwardsScattering = 0.1f;
    [Min(0)]
    public float BaseBrightness = 0f;
    [Min(0)]
    public float PhaseFactor = 1f;
    public Vector4 PhaseParametars {
        get => new Vector4(ForwardsScattering, BackwardsScattering, BaseBrightness, PhaseFactor);
    }

    // Sun
    [Header("Sun Settings")]
    public Vector3 SunPosition = new Vector3(100, 200, 100);
    public Color SunColor = new Color(1, 1, 1);

    // Noise
    [Header("Noise Section")]
    [Min(0)]
    public int NoiseTextureResolution = 512;
    public int NumberOfCellsPerAxis = 100;
    private RenderTexture noise_texture = null;
    public RenderTexture NoiseTexture {
        get {
            if (noise_texture == null)
                create_w_noise_texture();
            return noise_texture;
        }
    }
    public ComputeShader NoiseComputeShader = null;

    private List<Vector3> scatter_points(int cells_per_axis) {
        // scatter points randomly
        var points = new List<Vector3>();
        float cell_size = 1f / cells_per_axis;

        for (int x = 0; x < cells_per_axis; x++)
            for (int y = 0; y < cells_per_axis; y++)
                for (int z = 0; z < cells_per_axis; z++) {
                    var random_offset = new Vector3(Random.value, Random.value, Random.value);
                    points.Add((new Vector3(x, y, z) + random_offset) * cell_size);
                }
        return points;
    }

    [ContextMenu("Regenerate")]
    private void create_w_noise_texture() {
        float frequency = NumberOfCellsPerAxis;
        List<Vector3> points_l = new List<Vector3>();
        for (int i = 0; i < 4; i++) {;
            int cpa = (int)frequency;
            points_l.AddRange(scatter_points(cpa));
            frequency *= 1.5f;
        }
        var points = points_l.ToArray();

        // initialize texture
        noise_texture = new RenderTexture(NoiseTextureResolution, NoiseTextureResolution, 0);
        noise_texture.format = RenderTextureFormat.ARGB64;
        noise_texture.enableRandomWrite = true;
        noise_texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        noise_texture.volumeDepth = NoiseTextureResolution;
        noise_texture.Create();

        // load in compute shader
        if (NoiseComputeShader == null) {
            Debug.LogError("Noise Compute Shader not set! :: CloudSettings.cs :: create_w_noise_texture()");
            return;
        }
        int kernel_id = NoiseComputeShader.FindKernel("CSMain");
        
        // =send data=
        // send resolution
        NoiseComputeShader.SetInt("resolution", NoiseTextureResolution);
        // send points
        ComputeBuffer random_points_buffer = new ComputeBuffer(points.Length, sizeof(float) * 3);
        random_points_buffer.SetData(points);
        NoiseComputeShader.SetBuffer(kernel_id, "points", random_points_buffer);
        // send texture
        NoiseComputeShader.SetTexture(kernel_id, "noise_texture", noise_texture);
        // send cells per axis
        NoiseComputeShader.SetInt("cells_per_axis", NumberOfCellsPerAxis);

        // run
        NoiseComputeShader.Dispatch(kernel_id, NoiseTextureResolution / 16 + 1, NoiseTextureResolution / 8 + 1, NoiseTextureResolution / 8 + 1);

        // release buffers
        random_points_buffer.Release();

    }

}
