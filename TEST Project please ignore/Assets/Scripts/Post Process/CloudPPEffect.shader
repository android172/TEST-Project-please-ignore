Shader "Hidden/Shader/CloudPPEffect"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
    #include "../Shaders/Includes/Noise/CloudsNoise.cginc"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        float3 view_dir   : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    // properties required for vert shader
    float4x4 _ViewProjectInverse;

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);

        // additions
        // camera directon texture coordinates
        float4 camera_foward_dir = mul(_ViewProjectInverse, float4(0.0, 0.0, 0.5, 1.0));
        camera_foward_dir.xyz /= camera_foward_dir.w;
        camera_foward_dir.xyz -= _WorldSpaceCameraPos;

        float4 camera_local_dir = mul(_ViewProjectInverse, float4(output.texcoord.x * 2.0 - 1.0, output.texcoord.y * 2.0 - 1.0, 0.5, 1.0));
        camera_local_dir.xyz /= camera_local_dir.w;
        camera_local_dir.xyz -= _WorldSpaceCameraPos;

        output.view_dir = camera_local_dir.xyz / length(camera_foward_dir.xyz);
        
        return output;
    }

    // List of properties to control your post process effect
    float _Intensity;
    TEXTURE2D_X(_InputTexture);

    // helper functions
    float2 ray_box_distance(float3 bounds_min, float3 bounds_max, float3 ray_origin, float3 ray_dir) {
        float3 t0 = (bounds_min - ray_origin) / ray_dir;
        float3 t1 = (bounds_max - ray_origin) / ray_dir;

        float3 t_min = min(t0, t1);
        float3 t_max = max(t0, t1);

        float distance_A = max(max(t_min.x, t_min.y), t_min.z);
        float distance_B = min(min(t_max.x, t_max.y), t_max.z);

        float distance_to_box = max(0, distance_A);
        float distance_inside_box = max(0, distance_B - distance_to_box);
        
        return float2(distance_to_box, distance_inside_box);
    }

    // fragment shader properties
    float3 _box_dimensions;
    float _cloud_density_factor;
    float _coverage;
    float3 _sun_position;
    float3 _sun_color;
    float3 _offset;
    float4 _phase_params;

    // Henyey-Greenstein
    float hg(float a, float g) {
        float g2 = g*g;
        return (1 - g2) / (12.566 * pow(abs(1 + g2 - 2 * g * (a)), 1.5));
    }

    float phase(float a) {
        float blend = .5;
        float hg_blend = hg(a, _phase_params.x) * (1 - blend) + hg(a, -_phase_params.y) * blend;
        return _phase_params.z + hg_blend * _phase_params.w;
    }

    float calculate_coverage(float val) {
        return (clamp(val, -_coverage, 1) + _coverage) / (1 + _coverage);
    }

    float calculate_density(float3 at_point) {
        // float3 _offset = 0;
        
        // float gradient = (at_point.y + _box_dimensions.y / 2) / _box_dimensions.y;
        // gradient = 1;

        float x = -0.05;
        float region_cutoff = 0.1;
        float in_region = (clamp(fractal_noise(at_point, 4, 0.5, 0.005, 2, 1, 0, _offset / 10), x, x + region_cutoff) - x) / region_cutoff;
        float height_factor = (1 - pow(abs(2 * at_point.y / _box_dimensions.y), 12)) / (1 + exp(12 / _box_dimensions.y * (at_point.y - _coverage * 8))) - 1;
        float density = in_region * calculate_coverage(fractal_noise(at_point, 3, 0.5, 0.05, 2, 1, 0, _offset) + height_factor/4);
        // float on_cloud = clamp(density, 0, 0.05) / 0.05;
        // density += on_cloud * (worley_noise(at_point,0.4,0.2));

        // density *= height_factor;
        return saturate(density);
    }
    
    float light_march(float3 position, float3 sun_direction) {
        float distance_inside_box = ray_box_distance(-_box_dimensions / 2, _box_dimensions / 2, position, sun_direction).y;
        float step_size = distance_inside_box / 8;

        float density = 0;
        for (int step = 0; step < 8; step ++) {
            position += sun_direction * step_size;
            density += calculate_density(position);
        }

        float transmittance = exp(-density * step_size * _cloud_density_factor);
        return transmittance; // ...
    }

    float4 CustomPostProcess(Varyings input) : SV_Target {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // setup
        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float3 color = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
        // ray
        float3 ray_origin = _WorldSpaceCameraPos;
        float3 ray_dir = normalize(input.view_dir);
        // depth
        float depth = LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS).x;
        depth = LinearEyeDepth(depth, _ZBufferParams) * length(input.view_dir);

        // box distance
        float2 ray_box_info = ray_box_distance(-_box_dimensions / 2, _box_dimensions / 2, ray_origin, ray_dir);
        float distance_to_box = ray_box_info.x;
        float distance_inside_box = ray_box_info.y;

        // ray didn't hit box
        if (distance_inside_box <= 0 || distance_to_box > depth)
            return float4(color, 1);
        
        // march trough cloud
        float extra_distance = blue_noise(ray_dir) * 0.1;
        // float step_size = distance_inside_box / 18; // _step_size = 8
        float step_size = 10 / 8;
        if (distance_inside_box / step_size > 50) step_size = distance_inside_box / 50;

        float transmittance = 1;
        float3 light_energy = 0;

        while(extra_distance < distance_inside_box && transmittance > 0.01) {
            float3 pixel_coord = ray_origin + ray_dir * (distance_to_box + extra_distance);
            extra_distance += step_size;

            float cloud_density = calculate_density(pixel_coord);
            if (cloud_density <= 0) continue;

            float3 sun_direction = normalize(_sun_position - pixel_coord);

            float light_transimttance  = light_march(pixel_coord, sun_direction);
            light_energy += cloud_density * step_size * transmittance * light_transimttance * phase(dot(ray_dir, sun_direction));
            transmittance *= exp(-cloud_density * step_size * _cloud_density_factor);
        }
        color = color * transmittance + light_energy * _sun_color;

        return float4(color, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "CloudPPEffect"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
