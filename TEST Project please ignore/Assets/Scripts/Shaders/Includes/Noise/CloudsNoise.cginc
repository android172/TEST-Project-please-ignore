#include "./SimplexNoise.cginc"
#include "./ValueNoise.cginc"
#include "./CellularNoise.cginc"

// fractal noise
float fractal_noise(float3 point_v, int number_of_layers, float amplitude_fading, float base_frequency, float frequency_multiplier, float strength, float base_height, float3 seed) {
	float noise_sum = 0;
	float amplitude = 1;
	float total_amplitude = 0;
	float frequency = base_frequency;

	for (int i = 0; i < number_of_layers; i++) {
		noise_sum += snoise(point_v * frequency + seed) * amplitude;
		total_amplitude += amplitude;
		amplitude *= amplitude_fading;
		frequency *= frequency_multiplier;
	}
	noise_sum /= number_of_layers;

	return noise_sum / total_amplitude * strength + base_height;
}
float fractal_noise(float3 point_v, float4 settings[3]) {
    return fractal_noise(point_v, (int)settings[0].x, settings[0].y, settings[0].z, settings[0].w, settings[1].x, settings[1].y, float3(settings[1].z, settings[1].w, settings[2].x));
}

float worley_noise(float3 point_v, float frequency, float strength) {
	float noise_sum = 0;
	float amplitude = 1;
	float total_amplitude = 0;
	for (int i = 0; i < 5; i++) {
		noise_sum += cellular(point_v * frequency).x * amplitude;
		total_amplitude += amplitude;
		amplitude *= 0.5;
		frequency *= 1.5;
	}

    return (0.5 - noise_sum / total_amplitude) * strength;
}

float blue_noise(float3 point_v) {
	float noise_val = snoise(point_v * 1000);
	noise_val = (noise_val > 0)? 1 : 0;
	return noise_val;
}