﻿Shader "Hidden/Shader/AtmospherePP"
{
	HLSLINCLUDE

	#pragma target 4.5

	#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
	//
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"

	#define FLT_MAX 3.402823466e+38
	#define FLT_MIN 1.175494351e-38

	struct Attributes
	{
		uint vertexID : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Varyings
	{
		float4 positionCS : SV_POSITION;
		float2 texcoord   : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	Varyings Vert(Attributes input)
	{
		Varyings output;
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
		output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
		return output;
	}

	// List of properties to control your post process effect
	float _Intensity;
	float3 _Position;
	float3 _DirToSun;
	float _PlanetRadius;
	float _AtmosphereRadius;
	float _DepthOffset;

	int _ScatterPoints = 8;
	int _OpticalDepthPoints = 8;
	float3 _ScatterCoefficients;
	float _DensityFalloff;
	TEXTURE2D_X(_InputTexture);

	float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir) {
		float3 offset = rayOrigin - sphereCentre;
		float a = 1;
		float b = 2 * dot(offset, rayDir);
		float c = dot(offset, offset) - sphereRadius * sphereRadius;
		float d = b * b - 4 * a * c;

		if (d > 0) {
			float s = sqrt(d);
			float distNear = max(0, (-b - s) / (2 * a));
			float distFar = (-b + s) / (2 * a);

			if (distFar >= 0) {
				return float2(distNear, distFar - distNear);
			}
		}

		return float2(FLT_MAX, 0);
	}

	float densityAtPoint(float3 samplePoint) {
		float heightAboveSurf = distance(samplePoint, _Position);
		float height01 = heightAboveSurf / (_AtmosphereRadius - _PlanetRadius);
		float localDensity = exp(-height01 * _DensityFalloff);// * (1 - height01);
		return localDensity;
	}

	float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength) {
		float3 densitySamplePoint = rayOrigin;
		float step = rayLength / (_OpticalDepthPoints - 1);
		float opticalDepth = 0;

		for (int i = 0; i < _OpticalDepthPoints; i+= 1) {
			float localDensity = densityAtPoint(densitySamplePoint);
			opticalDepth += 0.5 * step;
			densitySamplePoint += rayDir * step;
		}
		return opticalDepth;
	}

	float3 calcLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 oColor) {
		float3 inPoint = rayOrigin;
		float step = rayLength / (_ScatterPoints - 1);
		float3 inScatteredLight = 0;
		float viewRayOpticalDepth = 0;

		for (int i = 0; i < _ScatterPoints; i++) {
			float sunRayLength = raySphere(_Position, _AtmosphereRadius, inPoint, _DirToSun).y;
			float sunRayOpticalDepth = opticalDepth(inPoint, _DirToSun, sunRayLength);
			viewRayOpticalDepth = opticalDepth(inPoint, -rayDir, step * i);
			float3 transmit = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * _ScatterCoefficients);
			float localDensity = densityAtPoint(inPoint);

			inScatteredLight += localDensity * transmit * _ScatterCoefficients * step;
			inPoint += rayDir * step;
		}

		float oColTransmit = exp(-viewRayOpticalDepth);
		return oColor * oColTransmit + inScatteredLight;
	}

	float4 CustomPostProcess(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		uint2 positionSS = input.texcoord * _ScreenSize.xy;
		float3 oColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;

		float3 cameraPos = -_WorldSpaceCameraPos;
		float depth = LoadCameraDepth(input.positionCS.xy);
		PositionInputs posInput = GetPositionInput(input.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
		float3 rayDir = GetWorldSpaceNormalizeViewDir(posInput.positionWS);

		float2 hit = raySphere(-_Position, _AtmosphereRadius, cameraPos, rayDir);

		float depthNonLinear = LoadCameraDepth(positionSS);
		float linearEyeDepth = LinearEyeDepth(depthNonLinear, _ZBufferParams);

		float distToSurf = (linearEyeDepth) * length(posInput.positionWS);
		float distToAtmo = hit.x;
		//float fixedDepth = distance(cameraPos, _Position) * _DepthOffset;
		float distThroughAtmo = min(hit.y, distToSurf - distToAtmo) / (_AtmosphereRadius * 2);

		//float3 newColor = (distThroughAtmo );

		//return distThroughAtmo;

		//return float4(lerp(oColor, newColor, _Intensity), 1);

		if (distThroughAtmo > 0) {
			const float eps = 0.0001;
			float3 pointInAtmo = cameraPos + rayDir * (distToAtmo + eps);
			float3 light = calcLight(pointInAtmo, rayDir, distThroughAtmo - eps * 2, oColor);
			return float4(light, 1);
		}

		return float4(oColor.rgb, 1);
	}
	ENDHLSL


	SubShader
	{
		Pass
		{
			Name "Atmosphere"
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