Shader "Lynx/Util/Modifier"
{
    Properties
    {
        _Threhsold("Threshold", Range(0, 2)) = 0.03
        _Cutoff("alpha cutout", Range(0.0, 1.0)) = 0.5

        _LeftTex("Texture", 2D) = "white" { }
        _RightTex("Texture", 2D) = "white" { }

        _IRes("ResolutionInPX", Vector) = (1536.0, 2106.0, 0.0, 0.0)
        [Toggle]_EnableChromaKey("Enable chroma key", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            float _Threhsold;
            float _Cutoff;
            float _EnableChromaKey;

            sampler2D _LeftTex;
            float4 _LeftTex_ST;

            sampler2D _RightTex;
            float4 _RightTex_ST;

            float4 _IRes;

            #include "UnityCG.cginc"
            #include "YUVRGB.cginc"

            float3 rgb2hsv(float4 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _LeftTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                //return lerp(_LeftEyeColor, _RightEyeColor, unity_StereoEyeIndex);
                i.uv.y = 1.0 - i.uv.y;
                fixed4 col = lerp(YUV2RGB(i.uv, _LeftTex, _IRes.xy), YUV2RGB(i.uv, _RightTex, _IRes.xy), unity_StereoEyeIndex);

                float3 hsv = rgb2hsv(col);
                float mask = step(abs(hsv.r - 0.33) > _Threhsold || hsv.g < 0.15 || hsv.b < 0.15, 0);
                col.w = lerp(1.0, 1.0 - mask, _EnableChromaKey);

                clip(col.a - _Cutoff);

                return col;
            }
            ENDCG
        }
    }

}
