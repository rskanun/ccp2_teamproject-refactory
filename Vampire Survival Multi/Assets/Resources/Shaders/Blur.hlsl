#pragma once

void Blur_float(float2 uv, float2 offset, float radius, UnityTexture2D tex, UnitySamplerState ss, out float3 Out)
{
    float3 totalColor = float3(0.0, 0.0, 0.0);
    float totalWeight = 0.0;

    // 중심 픽셀 샘플링 (중심 가중치)
    float centerWeight = 1.0;
    totalColor += SAMPLE_TEXTURE2D(tex, ss, uv).rgb * centerWeight;
    totalWeight += centerWeight;

    // 16~24개 고정 샘플로 텍스처 캐시 보호 및 균일한 원형 분포 형성
    const int SAMPLES = 20;

    // 표준편차 sigma: radius에 비례하도록 설정
    float sigma = max(radius * 0.5, 0.001);
    float twoSigmaSq = 2.0 * sigma * sigma;

    [unroll(20)]
    for (int i = 1; i <= SAMPLES; i++)
    {
        // 1. 원형 영역 내에 고르게 퍼지도록 반경 및 각도 계산
        float distFactor = sqrt((float)i / (float)SAMPLES); // 중심 밀도를 자연스럽게 유지
        float currentDist = distFactor * radius;
        float angle = (float)i * 2.39996323;

        // 2. 2D 방향 벡터 생성
        float2 dir = float2(cos(angle), sin(angle));
        float2 sampleUV = uv + dir * (currentDist * offset);

        // 3. 실제 가우시안 가중치 계산 (e^(-r^2 / 2sigma^2))
        float weight = exp(-(currentDist * currentDist) / twoSigmaSq);

        // 4. 가중 누적
        totalColor += SAMPLE_TEXTURE2D(tex, ss, sampleUV).rgb * weight;
        totalWeight += weight;
    }

    // 정규화 (에너지 보존)
    Out = totalColor / totalWeight;
}

void Blur_half(float2 uv, float2 offset, float radius, UnityTexture2D tex, UnitySamplerState ss, out float3 Out)
{
    Blur_float(uv, offset, radius, tex, ss, Out);
}