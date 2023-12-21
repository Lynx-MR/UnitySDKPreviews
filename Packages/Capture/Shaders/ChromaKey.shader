Shader "Unlit/ChromaKey"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threhsold("Threshold", Range(0, 2)) = 0.01
        _Cutoff("alpha cutout", Range(0.0,1.0)) = 0.5

    }
    SubShader
    {
        Tags {
            "Queue" = "AlphaTest"
            "RenderType" = "TransparentCutout"
        }
        LOD 100
        Cull Off
        ZWrite On
        AlphaTest Greater[_Cutoff]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag alphatest:_Cutoff

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Threhsold;
            float _Cutoff;

            float3 rgb2hsv(float4 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float3 hsv = rgb2hsv(col);
                float mask = step(abs(hsv.r - 0.33) > _Threhsold || hsv.g < 0.15 || hsv.b < 0.15, 0);
                col.w = 1.0-mask;
                
                clip(col.a - _Cutoff);
                
                return col;
            }
            ENDCG
        }
    }
}
