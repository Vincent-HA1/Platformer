Shader "Custom/SpriteFlashWhite"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color      ("Tint Color", Color) = (1,1,1,1)
        _Flash      ("Flash Amount", Range(0,1)) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        // <- use standard alpha blending:
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4   _MainTex_ST;
            fixed4   _Color;
            float    _Flash;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color    = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // sample full RGBA
                fixed4 samp = tex2D(_MainTex, IN.texcoord) * IN.color;

                // keep its alpha always
                fixed alpha = samp.a;

                // when _Flash=0 -> normal RGB; when _Flash=1 -> white
                fixed3 outRGB = lerp(samp.rgb, fixed3(1,1,1), _Flash);

                return fixed4(outRGB, alpha);
            }
            ENDCG
        }
    }
}
