Shader "Custom/Fisheye"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strength ("Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
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
            float _Strength;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv = uv * 2.0 - 1.0;
                float r = length(uv);
                uv = uv / (1.0 + _Strength * r * r);
                uv = (uv + 1.0) * 0.5;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
