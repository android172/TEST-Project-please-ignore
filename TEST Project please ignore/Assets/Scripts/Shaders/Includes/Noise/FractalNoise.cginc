#include "./SimplexNoise.cginc"

// fractal noise
float fractal_noise(float3 point_v, int number_of_layers, float amplitude_fading, float base_frequency, float frequency_multiplier, float strength, float base_height, float3 seed) {
	float noise_sum = 0;
	float amplitude = 1;
	float frequency = base_frequency;

	for (int i = 0; i < number_of_layers; i++) {
		noise_sum += snoise(point_v * frequency + seed) * amplitude;
		frequency *= frequency_multiplier;
		amplitude *= amplitude_fading;
	}
	noise_sum /= number_of_layers;

	return noise_sum * strength + base_height;
}
float fractal_noise(float3 point_v, float4 settings[3]) {
    return fractal_noise(point_v, (int)settings[0].x, settings[0].y, settings[0].z, settings[0].w, settings[1].x, settings[1].y, float3(settings[1].z, settings[1].w, settings[2].x));
}

// ridghe noise
float ridge_noise(float3 point_v, int number_of_layers, float amplitude_fading, float base_frequency, float frequency_multiplier, float strength, float base_height, float3 seed, 
				  float power, float gain) {
	float noise_sum = 0;
	float amplitude = 1;
	float frequency = base_frequency;
	float ridge_weight = 1;

	for (int i = 0; i < number_of_layers; i++) {
        float noise_val = 1.0 - abs(snoise(point_v * frequency + seed));
		noise_val = pow(abs(noise_val), power) * ridge_weight;
		ridge_weight = saturate(ridge_weight * gain);
		
		noise_sum += noise_val * amplitude;
		frequency *= frequency_multiplier;
		amplitude *= amplitude_fading;
	}

	return noise_sum * strength + base_height;
}
float ridge_noise(float3 point_v, float4 settings[3]) {
    return ridge_noise(point_v, (int)settings[0].x, settings[0].y, settings[0].z, settings[0].w, settings[1].x, settings[1].y, float3(settings[1].z, settings[1].w, settings[2].x), settings[2].y, settings[2].z);
}
float ridge_noise_2(float3 pos, float4 settings[3]) {
    float3 sphereNormal = normalize(pos);
    float3 axisA = cross(sphereNormal, float3(0,1,0));
    float3 axisB = cross(sphereNormal, axisA);

    float offsetDst = settings[2].w * 0.01;
    float sample0 = ridge_noise(pos, settings);
    float sample1 = ridge_noise(pos - axisA * offsetDst, settings);
    float sample2 = ridge_noise(pos + axisA * offsetDst, settings);
    float sample3 = ridge_noise(pos - axisB * offsetDst, settings);
    float sample4 = ridge_noise(pos + axisB * offsetDst, settings);
    return (sample0 + sample1 + sample2 + sample3 + sample4) / 5.0;
}