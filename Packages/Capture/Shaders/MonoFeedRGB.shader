Shader "Lynx/Util/MonoCamFeedRGB"
{
    Properties
    {
        _CameraTexture("Texture Left", 2D) = "white" { }
        _IRes("ResolutionInPX", Vector) = (1536.0, 2106.0, 0.0, 0.0)
    }

    SubShader
    { 
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag


            sampler2D _CameraTexture;
            float4 _CameraTexture_ST;

            float4 _IRes;

            #include "UnityCG.cginc"
            #include "YUVRGB.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _CameraTexture);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = YUV2RGB(i.uv, _CameraTexture, _IRes.xy);
                return col;
            }
            ENDCG
        }
    }
}
