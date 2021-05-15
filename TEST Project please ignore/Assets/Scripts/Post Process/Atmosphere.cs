using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Atmosphere")]
public sealed class Atmosphere : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    public Material m_Material;

    [Tooltip("Set the position of the sun.")]
    public Vector3Parameter directionToSun = new Vector3Parameter(new Vector3(0f, 0f, 0f));
    [Tooltip("Set the position of the planet.")]
    public Vector3Parameter planetPosition = new Vector3Parameter(new Vector3(0f, 0f, 0f));
    public FloatParameter planetRadius = new FloatParameter(1f);
    //public FloatParameter depthOffset = new FloatParameter(1f); 
    public ClampedFloatParameter atmosphereScale = new ClampedFloatParameter(0f, 0f, 1f); 

    public ClampedIntParameter scatterPoints = new ClampedIntParameter(8, 0, 128);
    public ClampedIntParameter opticalDepthPoints = new ClampedIntParameter(8, 0, 128);
    public FloatParameter densityFalloff = new ClampedFloatParameter(1f,0f,20f);

    public Vector3Parameter scatterCoefficients = new Vector3Parameter(new Vector3(0f, 0f, 0f));
    public FloatParameter scatterStrength = new FloatParameter(1f);

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/AtmospherePP") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/AtmospherePP"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetMatrix("_ViewProjectInverse", (camera.camera.projectionMatrix * camera.camera.worldToCameraMatrix).inverse);

        m_Material.SetFloat("_Intensity", intensity.value);
        float atmosphereRadius = planetRadius.value * (1 + atmosphereScale.value);
        m_Material.SetFloat("_AtmosphereRadius", atmosphereRadius);
        m_Material.SetFloat("_PlanetRadius", planetRadius.value);
        //m_Material.SetFloat("_DepthOffset", depthOffset.value);
        m_Material.SetFloat("_DensityFalloff", densityFalloff.value);
        m_Material.SetVector("_Position", planetPosition.value);
        m_Material.SetVector("_DirToSun", directionToSun.value.normalized);
        m_Material.SetTexture("_InputTexture", source);
        m_Material.SetInt("_ScatterPoints", scatterPoints.value);
        m_Material.SetInt("_OpticalDepthPoints", opticalDepthPoints.value);

        m_Material.SetVector("_ScatterCoefficients", setCoefficients(scatterCoefficients.value));
        HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }

    private Vector3 setCoefficients(Vector3 v)
    {
        float strength = scatterStrength.value;
        Vector3 o = new Vector3();
        o.x = Mathf.Pow(400 / v.x, 4) * strength;
        o.y = Mathf.Pow(400 / v.y, 4) * strength;
        o.z = Mathf.Pow(400 / v.z, 4) * strength;

        return o;
    }

    public override void Cleanup() => CoreUtils.Destroy(m_Material);

}