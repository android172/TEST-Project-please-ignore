using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/CloudPPEffect")]
public sealed class CloudPPEffect : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    private CloudSettings settings;

    Material m_Material;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Hidden/Shader/CloudPPEffect";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null) {
            m_Material = new Material(Shader.Find(kShaderName));

            settings = GameObject.Find("Cloud Master").GetComponent<CloudSettings>();
        }
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume CloudPPEffect is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination) {
        if (m_Material == null)
            return;

        m_Material.SetMatrix("_ViewProjectInverse", (Camera.current.projectionMatrix * Camera.current.worldToCameraMatrix).inverse);
        m_Material.SetFloat("_Intensity", intensity.value);
        m_Material.SetTexture("_InputTexture", source);
        HDUtils.DrawFullScreen(cmd, m_Material, destination);

        m_Material.SetVector("_box_dimensions", settings.BoxDimensions);

        m_Material.SetFloat("_cloud_density_factor", settings.CloudDensityFactor);
        m_Material.SetFloat("_coverage", settings.Coverage);

        m_Material.SetVector("_sun_position", settings.SunPosition);
        m_Material.SetColor("_sun_color", settings.SunColor);
        m_Material.SetVector("_phase_params", settings.PhaseParametars);

        float t = Time.time / 10.0f;
        // float t = 0;

        m_Material.SetVector("_offset", new Vector3(t,0,0));
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
