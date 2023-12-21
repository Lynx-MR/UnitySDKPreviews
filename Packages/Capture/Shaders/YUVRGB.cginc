#ifndef YUVRGB
#define YUVRGB


#define Y_THIRD 1.0 / 3.0

float4 YUV_ST;


float4 YUV2RGB(float2 uvCoord, sampler2D YUV, float2 resolution)
{
    //--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y--y
    float2 yUVC = uvCoord;
    yUVC.y /= 1.5;
    float y = tex2D(YUV, yUVC).x;


    //--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv--uv
    float UVYC = (uvCoord.y * Y_THIRD) + 2.0 * Y_THIRD;
    float UVXCBase = (uvCoord.x * 0.5) * resolution.x;

    //--u--u--u--u
    float UVXC = (UVXCBase + floor(UVXCBase.x)) / resolution;
    float u = tex2D(YUV, float2(UVXC, UVYC)).x;
    //--v--v--v--v
    float VUVCx = UVXC + (1.0 / resolution);
    float v = tex2D(YUV, float2(VUVCx, UVYC)).x;

    //--conversion--conversion--conversion--conversion--conversion--conversion--conversion--conversion
    int nY = (int)(floor(y * 256.0) - 16.0);
    int nU = (int)(floor(u * 256.0) - 128.0);
    int nV = (int)(floor(v * 256.0) - 128.0);

    int nR = (int)(1192 * nY + 1634 * nV);
    int nG = (int)(1192 * nY - 833 * nV - 400 * nU);
    int nB = (int)(1192 * nY + 2066 * nU);

    nR = min(262143, max(0, nR));
    nG = min(262143, max(0, nG));
    nB = min(262143, max(0, nB));

    nR = (nR >> 10) & 0xff;
    nG = (nG >> 10) & 0xff;
    nB = (nB >> 10) & 0xff;

    return float4(
        (float)nR / 256.0,
        (float)nG / 256.0,
        (float)nB / 256.0,
        1.0);
}

float2 RGB2YUV(float2 uvCoord, sampler2D rgb, float2 resolution)
{
    uvCoord = abs(uvCoord%1.0);
    //Luminance
    float2 lUV = uvCoord;
    lUV.y /= Y_THIRD*2; 
    lUV.y += 0.5;
    float4 lRGB = floor(tex2D(rgb, lUV)*255);

    //Chrominance
    float2 uvUV = uvCoord;
    uvUV.y *= 3.0;
    uvUV.x = (floor((uvCoord.x * resolution.x)*0.5) * 2.0) / resolution.x;

    float4 uvRGB = floor(tex2D(rgb, uvUV)*255);

    float Y = lRGB.r * 0.299 + lRGB.g * 0.587 + lRGB.b * 0.114;
    float U = uvRGB.r * -0.168736 + uvRGB.g * -0.331264 + uvRGB.b * 0.5 + 128.0;
    float V = uvRGB.r * 0.5 + uvRGB.g * -0.418688 + uvRGB.b * -0.081312 + 128.0;

    //masking 
    float YUVMask = step(0.001, floor(lUV.y));
        //separate Y and UV
    Y *= YUVMask;
    U *= 1.0 - YUVMask;
    V *= 1.0 - YUVMask;
    
        //separate U and V
    float mask = floor((uvCoord.x * resolution.x)%2.0);
    U *= 1.0-mask;
    V *= mask;

    
    //merge result
    float2 result = float2(Y,0.0);
    result.x += U;
    result.x += V;
    result.y = lRGB.w * YUVMask + uvRGB.w *(1.0-YUVMask);

    return result/256;
}

#endif