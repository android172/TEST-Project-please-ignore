#include "./SimplexNoise.cginc"
#include "./ValueNoise.cginc"
#include "./CellularNoise.cginc"

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
    return ridge_noise(point_v, (int)settings[0].x, settings[0].y, settings[0].z, settings[0].w, settings[1].x, settings[1].y, float3(settings[1].z, settings[1].w, settings[2].x), 
                       settings[2].y, settings[2].z);
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

// returns 3D fbm and its 3 derivatives
const float3x3 m3  = float3x3( 0.00,  0.80,  0.60,
                      -0.80,  0.36, -0.48,
                      -0.60, -0.48,  0.64 );
float fractal_noise_dir(float3 point_v, int number_of_layers, float amplitude_fading, float base_frequency, float frequency_multiplier, float strength, float3 seed) {
    float amplitude = 1;
	float frequency = base_frequency;
    float base_noise_acc = 0.0;
    float3 derivatives_acc = float3(0.0, 0.0, 0.0);

    for( int i=0; i < number_of_layers; i++ ) {
        float4 noise_val = noised(point_v * frequency + seed);
        derivatives_acc += noise_val.yzw; // accumulate derivatives
        base_noise_acc  += amplitude * noise_val.x / (1.0 + dot(derivatives_acc, derivatives_acc));   // accumulate values
        amplitude *= amplitude_fading;
		frequency *= frequency_multiplier;
        point_v = mul(m3, point_v);
    }
    return base_noise_acc * strength;
}
float fractal_noise_d(float3 point_v, float4 settings[3]) {
	return fractal_noise_dir(point_v, (int)settings[0].x, settings[0].y, settings[0].z, settings[0].w, settings[1].x, float3(settings[1].z, settings[1].w, settings[2].x));
}

// crater biuse
float crater_noise(float3 point_v, int number_of_layers, float base_depth, float depth_fading, float base_frequency, float frequency_multiplier, float radius,
                   int crater_slope, float ce_height, float ce_width, float outside_slope, float jitter_amount, float strength, float base_height, float3 seed) {
	float frequency = base_frequency;
	float depth = base_depth;
    float3 offset = seed + snoise(point_v * 5) * jitter_amount * 0.01;
	float noise_sum = 0.0;
	for(int i = 0; i < number_of_layers; i++) {
		float x = radius * cellular(point_v * frequency + offset).x;
		// general crater shape
        float crater_hole = pow(abs(x), crater_slope) * depth;
        // mountain in the middle
		float central_elevation = (- x*x*x*x / (ce_width * 0.01) + 0.1) * ce_height * depth;
        // elevation outside of the crater
        float ol = (1.0 + outside_slope*0.1) - x;
		float outside_land = max(depth + ol*ol*ol, depth);
        // total
		noise_sum += min(max(central_elevation, crater_hole), outside_land) - depth;
		frequency *= frequency_multiplier;
		depth *= depth_fading;
	}
	return noise_sum * strength + base_height;
}

float crater_noise(float3 point_v, float4 settings[4]) {
    return crater_noise(point_v, (int) settings[0].x, settings[0].y, settings[0].z, settings[0].w, settings[1].x, settings[1].y, (int) settings[1].z, settings[1].w,
                 settings[2].x, settings[2].y, settings[2].z, settings[2].w, settings[3].x, settings[3].yzw);
}