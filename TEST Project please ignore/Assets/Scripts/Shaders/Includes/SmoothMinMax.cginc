// Created by evilryu
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

float smin(float a, float b, float k)
{
    float x = exp(-k * a);
    float y = exp(-k * b);
    return (a * x + b * y) / (x + y);
}

float smax(float a, float b, float k) {
    return smin(a, b, -k);
}