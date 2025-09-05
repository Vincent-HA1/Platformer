Shader "Custom/CelesteWaterRefract"
{
    Properties
    {
        _MainTex ("Water Tint Texture", 2D) = "white" {}
        _GrabStrength ("Refraction Strength", Range(0,0.1)) = 0.02
        _BandCount ("Number of Bands", Float) = 20      // Used to shift wave frequency
        _BandHeight ("Band Height (UV space)", Range(0,1)) = 0.05 // Height of each band in UV units
        _WaveSpeed ("Wave Speed", Range(0,5)) = 1.0
        _TintColor ("Water Tint Color", Color) = (0.2, 0.6, 1.0, 0.5)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        GrabPass { }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _GrabTexture;

            float _GrabStrength;
            float _BandCount;
            float _BandHeight;
            float _WaveSpeed;
            float4 _TintColor;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 grabPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample tint texture for mask/alpha
                fixed4 texCol = tex2D(_MainTex, i.uv);

                // Screen UV
                float2 uv = i.grabPos.xy / i.grabPos.w;

                // Use custom band height in UV space
                float bandHeight = _BandHeight;

                // Calculate band index and local position
                float bandIdxF = floor(uv.y / bandHeight);
                float localY = frac(uv.y / bandHeight);

                // Choose direction: alternate bands
                float dir = fmod(bandIdxF, 2) * 2 - 1; // yields -1 or +1

                // Continuous wave offset within each band
                // Frequency scaled by band count for effect
                float offset = sin((uv.y + _Time.y * _WaveSpeed) * _BandCount * 3.1415)
                               * _GrabStrength * dir;

                // Apply refraction
                float2 refrUV = uv;
                refrUV.x += offset;
                fixed4 refr = tex2D(_GrabTexture, refrUV);

                // Blend with tint
                fixed4 color = lerp(refr, _TintColor, _TintColor.a * texCol.a);
                return color;
            }
            ENDCG
        }
    }
    FallBack Off
}
