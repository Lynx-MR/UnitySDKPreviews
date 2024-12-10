Shader "Lynx/Util/StereoCamFeedRGB"
{
    Properties
    {
        [Toggle] _IsRightEye("Right Eye", Float) = 0
        _CameraTexture("Texture Left", 2D) = "white" { }
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _IRes("ResolutionInPX", Vector) = (1536.0, 2106.0, 0.0, 0.0)
    }

    SubShader
    { 
        Tags 
        {
            "Queue" = "AlphaTest"
            "RenderType" = "TransparentCutout"
        }
        LOD 100
        Cull Off
        ZWrite Off
        AlphaTest Greater[_Cutoff]

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag alphatest:_Cutoff


            sampler2D _CameraTexture;
            float4 _CameraTexture_ST;

            float _IsRightEye;
            float _Cutoff;
            float4 _IRes;

            #include "UnityCG.cginc"
            #include "YUVRGB.cginc"

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
                o.uv = TRANSFORM_TEX(v.uv, _CameraTexture);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                fixed4 col = YUV2RGB(i.uv, _CameraTexture, _IRes.xy);


                col.w = lerp(0.0, 1.0, unity_StereoEyeIndex == _IsRightEye);

                clip(col.a - _Cutoff);

                return col;
            }
            ENDCG
        }
    }
}
