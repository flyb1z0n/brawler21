// Doodle-style sprite shader.
// Applies a UV-space sine wobble every frame so sprite edges shimmer
// like a hand-drawn animation. Drop onto any SpriteRenderer material.
Shader "Custom/DoodleSprite"
{
    Properties
    {
        _MainTex      ("Sprite Texture", 2D)   = "white" {}
        _Color        ("Tint",           Color) = (1,1,1,1)
        _WobbleAmt    ("Wobble Amount",  Float) = 0.018
        _WobbleSpeed  ("Wobble Speed",   Float) = 2.8
        _WobbleFreq   ("Wobble Freq",    Float) = 9.0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
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
            fixed4    _Color;
            float     _WobbleAmt;
            float     _WobbleSpeed;
            float     _WobbleFreq;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color    = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;

                // Horizontal wobble driven by vertical position and time
                uv.x += sin(uv.y * _WobbleFreq + _Time.y * _WobbleSpeed)
                        * _WobbleAmt;
                // Subtle vertical wobble for a second harmonic
                uv.y += sin(uv.x * _WobbleFreq * 0.7 + _Time.y * _WobbleSpeed * 0.6)
                        * _WobbleAmt * 0.4;

                fixed4 c = tex2D(_MainTex, uv);
                c *= i.color;
                c.rgb *= c.a;   // pre-multiply alpha (required for One/OneMinusSrcAlpha)
                return c;
            }
            ENDCG
        }
    }
}
