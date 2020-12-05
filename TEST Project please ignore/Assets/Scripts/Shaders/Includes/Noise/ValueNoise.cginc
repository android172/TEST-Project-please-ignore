
// hash function
float hash(float3 p) {
    p  = 50.0*frac( p*0.3183099 + float3(0.71,0.113,0.419));
    return -1.0+2.0*frac( p.x*p.y*p.z*(p.x+p.y+p.z) );
}

// returns 3D value noise and its 3 derivatives
 float4 noised(float3 x )
 {
    float3 p = floor(x);
    float3 w = frac(x);

    // cubic interpolation
    float3 u = w*w*w * (w * (w*6.0 - 15.0) + 10.0);
    float3 du = 30.0 * w*w * (w * (w - 2.0) + 1.0);

    float a = hash( p + float3(0,0,0) );
    float b = hash( p + float3(1,0,0) );
    float c = hash( p + float3(0,1,0) );
    float d = hash( p + float3(1,1,0) );
    float e = hash( p + float3(0,0,1) );
    float f = hash( p + float3(1,0,1) );
    float g = hash( p + float3(0,1,1) );
    float h = hash( p + float3(1,1,1) );

    float k0 =   a;
    float k1 =   b - a;
    float k2 =   c - a;
    float k3 =   e - a;
    float k4 =   a - b - c + d;
    float k5 =   a - c - e + g;
    float k6 =   a - b - e + f;
    float k7 = - a + b + c - d + e - f - g + h;

    return float4( -1.0+2.0*(k0 + k1*u.x + k2*u.y + k3*u.z + k4*u.x*u.y + k5*u.y*u.z + k6*u.z*u.x + k7*u.x*u.y*u.z),
                 2.0* du * float3( k1 + k4*u.y + k6*u.z + k7*u.y*u.z,
                                 k2 + k5*u.z + k4*u.x + k7*u.z*u.x,
                                 k3 + k6*u.x + k5*u.y + k7*u.x*u.y ) );
}