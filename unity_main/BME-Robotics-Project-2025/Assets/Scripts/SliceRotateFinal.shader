Shader "Custom/SliceRotateFinal"
{
    Properties
    {
        _MainTex ("Slice Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Base 90° CCW rotation: (1 - v, u)
                float2 rotatedUV = float2(1.0 - v.uv.y, v.uv.x);

                // Now flip horizontally and vertically
                rotatedUV.x = 1.0 - rotatedUV.x; // mirror
                rotatedUV.y = 1.0 - rotatedUV.y; // upside-down

                o.uv = rotatedUV;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
