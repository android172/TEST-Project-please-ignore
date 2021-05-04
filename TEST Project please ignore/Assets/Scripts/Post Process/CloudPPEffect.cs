using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/CloudPPEffect")]
public sealed class CloudPPEffect : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    // Paramethars
    // general
    [Header("General")]
    public MinFloatParameter Scale = new MinFloatParameter(1, 0.001f);
    public Vector3Parameter Location = new Vector3Parameter(Vector3.zero);
    public MinFloatParameter Radius = new MinFloatParameter(10, 1);
    public Vector4 GetShapeParametars {
        get => new Vector4(Location.value.x, Location.value.y, Location.value.z, Radius.value);
    }
    public MinFloatParameter CloudDensityFactor = new MinFloatParameter(5, 0);

    // Lighting
    [Header("Lighting Settings")]
    public ClampedFloatParameter DarknessThreshold = new ClampedFloatParameter(0.085f, 0, 1);
    public ClampedFloatParameter ForwardsScattering = new ClampedFloatParameter(0.9f, -1, 1);
    public ClampedFloatParameter BackwardsScattering = new ClampedFloatParameter(0.1f, -1, 1);
    public MinFloatParameter BaseBrightness = new MinFloatParameter(3, 0);
    public MinFloatParameter PhaseFactor = new MinFloatParameter(5, 0);
    public Vector4 PhaseParametars {
        get => new Vector4(ForwardsScattering.value, BackwardsScattering.value, BaseBrightness.value, PhaseFactor.value);
    }

    // Sun
    [Header("Sun Settings")]
    public Vector3Parameter SunPosition = new Vector3Parameter(new Vector3(100, 200, 100));
    public ColorParameter SunColor = new ColorParameter(new Color(1, 1, 1));

    Material m_Material;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Hidden/Shader/CloudPPEffect";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null) {
            m_Material = new Material(Shader.Find(kShaderName));
        }
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume CloudPPEffect is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination) {
        if (m_Material == null)
            return;

        m_Material.SetMatrix("_ViewProjectInverse", (camera.camera.projectionMatrix * camera.camera.worldToCameraMatrix).inverse);
        m_Material.SetFloat("_Intensity", intensity.value);
        m_Material.SetTexture("_InputTexture", source);
        HDUtils.DrawFullScreen(cmd, m_Material, destination);

        m_Material.SetFloat("_scale", Scale.value);
        m_Material.SetFloat("_width", 20f / Scale.value);
        m_Material.SetVector("_shape_parametars", GetShapeParametars);

        m_Material.SetFloat("_cloud_density_factor", CloudDensityFactor.value);
        m_Material.SetFloat("_darkness_threshold", DarknessThreshold.value);

        m_Material.SetVector("_sun_position", SunPosition.value);
        m_Material.SetColor("_sun_color", SunColor.value);
        m_Material.SetVector("_phase_params", PhaseParametars);

        float t = Time.time / (Radius.value * 10.0f);
        Vector3 offset = new Vector3(
            Radius.value * Mathf.Cos(t),
            t,
            Radius.value * Mathf.Sin(t)
        );

        m_Material.SetVector("_offset", offset);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
