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

    // old
    // float3 _box_dimensions;
    // Texture3D _worley_noise_text;
    // uint _wnt_resolution;
    // SamplerState sampler_worley_noise_text;

    // sun
    float3 _sun_position;
    float3 _sun_color;

    // shape
    float4 _shape_parametars;
    float _width;
    float _scale;

    // cloud settings
    float _cloud_density_factor;
    float _darkness_threshold;
    float3 _offset;
    float4 _phase_params;

    // helper functions
    float2 ray_box_distance(float3 ray_origin, float3 ray_dir) {
        float r = _shape_parametars.w;
        float3 q = ray_origin - (float3)_shape_parametars;

        float r1 = (r - _width/2.0);
        float r2 = (r + _width/2.0);

        float a = dot(ray_dir, ray_dir);
        float b = dot(ray_dir, q);
        float c1 = dot(q, q) - r1*r1;
        float c2 = dot(q, q) - r2*r2;

        float D1 = b*b - a*c1;
        float D2 = b*b - a*c2;

        float d1 = 0;
        float d2 = 0;
        float d3 = 0;
        float d4 = 0;

        float distance_to_box = 0;
        float distance_inside_box = 0;

        if (D1 < 0) {
            if (D2 < 0) return float2(0, 0);

            d3 = (-b + sqrt(D2)) / a;
            d4 = (-b - sqrt(D2)) / a;

            if (d3 < 0) d3 = 0;
            if (d4 < 0) d4 = 0;

            distance_to_box = min(d3, d4);
            distance_inside_box = max(d3, d4) - distance_to_box;

            return float2(distance_to_box, distance_inside_box);
        }

        d1 = (-b + sqrt(D1)) / a;
        d2 = (-b - sqrt(D1)) / a;
        d3 = (-b + sqrt(D2)) / a;
        d4 = (-b - sqrt(D2)) / a;

        int g = 0;
        if (d1 < 0) {
            d1 = Max_float();
            g += 1;
        }
        if (d2 < 0) {
            d2 = Max_float();
            g += 1;
        }
        if (d3 < 0) {
            d3 = Max_float();
            g += 1;
        }
        if (d4 < 0) {
            d4 = Max_float();
            g += 1;
        }

        if (g == 4) return float2(0, 0);

        float min_d = min(min(min(d1, d2), d3), d4);

        if (g % 2 == 1) {
            distance_inside_box = min_d;
            return float2(distance_to_box, distance_inside_box);
        }

        distance_to_box = min_d;

        d1 = (d1 == min_d)? Max_float() : d1 - min_d;
        d2 = (d2 == min_d)? Max_float() : d2 - min_d;
        d3 = (d3 == min_d)? Max_float() : d3 - min_d;
        d4 = (d4 == min_d)? Max_float() : d4 - min_d;

        distance_inside_box = min(min(min(d1, d2), d3), d4);

        return float2(distance_to_box, distance_inside_box);

        // old
        // float3 t0 = (bounds_min - ray_origin) / ray_dir;
        // float3 t1 = (bounds_max - ray_origin) / ray_dir;

        // float3 t_min = min(t0, t1);
        // float3 t_max = max(t0, t1);

        // float distance_A = max(max(t_min.x, t_min.y), t_min.z);
        // float distance_B = min(min(t_max.x, t_max.y), t_max.z);

        // float distance_to_box = max(0, distance_A);
        // float distance_inside_box = max(0, distance_B - distance_to_box);
        
        // return float2(distance_to_box, distance_inside_box);
    }

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

    float calculate_coverage(float val, float coverage) {
        return (clamp(val, - coverage, 1) + coverage) / (1 + coverage);
    }

    float region_sample(float3 at_point) {
        return fractal_noise(at_point, 6, 0.5, 0.0005 * _scale, 2, 1, 0, _offset / 10);
    }

    float calculate_density(float3 at_point) {
        // calculate regions
        float r1 = -0.05;
        float r2 = -0.015;
        float r3 = 0.02;
        float r4 = 0.06;
        float slope = 300;
        float region = region_sample(at_point);
        // calculete coverage of region
        float coverage = 0.5 * (
            0.4  / (1 + exp(-slope * (region - r1))) +
            0.15 / (1 + exp(-slope * (region - r2))) +
            0.25 / (1 + exp(-slope * (region - r3))) +
            0.1  / (1 + exp(-slope * (region - r4)))
        ) - 0.3;
        
        // general height factors
        float height = distance((float3)_shape_parametars, at_point) - _shape_parametars.w;
        float height_factor = (1 - pow(abs(2 * height / _width), 12)) / (1 + exp(12 / _width * (height - coverage * 8))) - 1;
        float height_fade = 1 - pow(abs((2 * height + 1) / (_width + 1)), 12);

        // cloud shape
        float density = height_fade * calculate_coverage(fractal_noise(at_point, 3, 0.5, 0.05 * _scale, 2, 1, 0, _offset) + height_factor/4, coverage);
        
        return saturate(density);
    }
    float calculate_density_ww(float3 at_point) {
        // cloud base shape
        float density = calculate_density(at_point);
        // base shape mask
        float on_cloud = clamp(density, 0, 0.05) / 0.05;
        // add cloud details
        density += on_cloud * (worley_noise(at_point, 0.3 * _scale, 0.1, _offset));

        // worley noise alternative
        // float f = 10;
        // float3 sl = at_point * f;
        // sl += f * max(_box_dimensions.x, _box_dimensions.y);
        // sl = sl % _wnt_resolution / _wnt_resolution;
        // density = on_cloud * tex3D(sampler_worley_noise_text, sl).x * 0.2;
        // density = on_cloud * _worley_noise_text.SampleLevel(sampler_worley_noise_text, sl, 0).x * 0.2;

        return saturate(density);
    }
    
    float light_march(float3 position, float3 sun_direction) {
        float distance_inside_box = ray_box_distance(position, sun_direction).y;
        float step_size = distance_inside_box / 8;

        float density = 0;
        for (int step = 0; step < 8; step ++) {
            position += sun_direction * step_size;
            density += calculate_density(position);
        }

        float transmittance = exp(-density * step_size * _cloud_density_factor * _scale);
        return _darkness_threshold + (1 - _darkness_threshold) * transmittance;
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

        // float c =  LOAD_TEXTURE2D_X(_worley_noise_text, positionSS % _wnt_resolution).x;
        // return float4(c,c,c,1);

        // box distance
        float2 ray_box_info = ray_box_distance(ray_origin, ray_dir);
        float distance_to_box = ray_box_info.x;
        float distance_inside_box = ray_box_info.y;

        // ray didn't hit box
        if (distance_inside_box <= 0 || distance_to_box > depth)
            return float4(color, 1);
        
        // march trough cloud
        float step_size = _width / 8;
        if (distance_inside_box / step_size > 50) step_size = distance_inside_box / 50;
        float extra_distance = blue_noise(ray_dir) * 0.1;

        float transmittance = 1;
        float3 light_energy = 0;

        while(extra_distance < distance_inside_box && transmittance > 0.01) {
            float3 pixel_coord = ray_origin + ray_dir * (distance_to_box + extra_distance);
            extra_distance += step_size;

            float cloud_density = calculate_density_ww(pixel_coord);
            if (cloud_density <= 0) continue;

            float3 sun_direction = normalize(_sun_position - pixel_coord);
            float dot_angle = dot(ray_dir, sun_direction);

            float light_transimttance  = light_march(pixel_coord, sun_direction);
            light_energy += cloud_density * step_size * transmittance * _scale * light_transimttance * 
                phase(dot_angle) * (1.0 - exp(-2 * pow(abs(cloud_density * step_size * _cloud_density_factor), abs(dot_angle))));
            transmittance *= exp(-cloud_density * step_size * _cloud_density_factor * _scale);
        }

        // calculate cloud color
        float3 cloud_coord = ray_origin + ray_dir * distance_to_box;
        float region = region_sample(cloud_coord);
        float cloud_color_multiplier = 1 - (0.6 / (1 + exp(-200 * (region - 0.06))));
        // planet side
        float side_coef = dot(
            normalize(cloud_coord - (float3)_shape_parametars), 
            normalize(_sun_position - (float3)_shape_parametars));
        if (side_coef < 0) {
            side_coef = 1 - side_coef;
            side_coef *= side_coef*side_coef*side_coef*side_coef*side_coef;
            cloud_color_multiplier *= 1/side_coef;
        }

        color = color * transmittance + light_energy * (_sun_color * cloud_color_multiplier);

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
