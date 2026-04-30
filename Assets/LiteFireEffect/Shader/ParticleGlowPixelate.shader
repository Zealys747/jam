Shader "LiteParticleEffect/Glow Pixelate"
{
    Properties
    {
        _MainTex ("Glow Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Range(1, 128)) = 32.0
        _Brightness ("Brightness", Range(0.1, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha One   // аддитивный — свечение добавляет свет
        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PixelSize;
            float _Brightness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;      // цвет и прозрачность из Particle System
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                o.color  = v.color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // пикселизация UV
                float2 pixUV = floor(i.uv * _PixelSize) / _PixelSize;

                half4 c = tex2D(_MainTex, pixUV);
                c *= i.color;           // применяем цвет из Particle System
                c.rgb *= _Brightness;   // яркость свечения

                return c;
            }
            ENDCG
        }
    }
}
