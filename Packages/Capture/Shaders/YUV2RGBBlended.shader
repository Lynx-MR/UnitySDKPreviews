Shader "Lynx/Util/YUV2RGB Blended"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _TexVR("Texture", 2D) = "white" {}
        _IRes("ResolutionInPX", Vector) = (1536.0,2106.0,0.0,0.0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        

        Pass
        {
            CGPROGRAM
            #pragma exclude_renderers gles
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"
            #include "YUVRGB.cginc"

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
            sampler2D _TexVR;
            float4 _TexVR_ST;
            float4 _IRes;



            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float colAR = tex2D(_MainTex, i.uv);
                float2 colVR = RGB2YUV(float2(i.uv.x, 1.0-i.uv.y), _TexVR, _IRes);

                float4 col = lerp(colAR, colVR.x, colVR.y);
                col = pow(col,2.2);
                col.w = 1.0;
                return col;
            }
            ENDCG
        }
    }
}
