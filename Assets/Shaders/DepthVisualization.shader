Shader "ZBuffer/DepthVisualization"
{
    Properties
    {
        _MainTex ("Source (RGB)", 2D) = "white" {}
        _DepthRangeNear ("Near Display Distance", Float) = 0.5
        _DepthRangeFar ("Far Display Distance", Float) = 65.0
        _InvertDepth ("Invert Depth (0: Near Dark / Far Bright, 1: Near Bright / Far Dark)", Float) = 1.0
        _DepthMode ("Depth Mode (0: Linear Eye Range, 1: Linear 0-1, 2: Raw Hardware Z)", Float) = 0.0
        _Contrast ("Contrast Exponent", Range(0.2, 4.0)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Overlay" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "DepthBlit"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            float _DepthRangeNear;
            float _DepthRangeFar;
            float _InvertDepth;
            float _DepthMode;
            float _Contrast;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 depthUV : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.depthUV = v.uv;

                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0.0)
                {
                    o.depthUV.y = 1.0 - o.depthUV.y;
                }
                #endif

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample raw depth from camera depth buffer
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.depthUV);

                // Detect sky / background void where nothing was rendered into depth buffer
                #if defined(UNITY_REVERSED_Z)
                bool isSky = (rawDepth <= 0.00002);
                #else
                bool isSky = (rawDepth >= 0.99998);
                #endif

                float depthValue = 0.0;

                if (isSky)
                {
                    // Far infinity / background void
                    depthValue = 1.0;
                }
                else if (_DepthMode > 1.5)
                {
                    // Mode 2: Raw non-linear hardware Z-Buffer value
                    #if defined(UNITY_REVERSED_Z)
                    depthValue = 1.0 - rawDepth;
                    #else
                    depthValue = rawDepth;
                    #endif
                }
                else if (_DepthMode > 0.5)
                {
                    // Mode 1: Normalized Linear01Depth (0 at near clip, 1 at far clip)
                    depthValue = Linear01Depth(rawDepth);
                }
                else
                {
                    // Mode 0: LinearEyeDepth mapped to [_DepthRangeNear, _DepthRangeFar]
                    // This provides optimal contrast for scene objects located between near drone and distant space station
                    float eyeDepth = LinearEyeDepth(rawDepth);
                    float range = max(_DepthRangeFar - _DepthRangeNear, 0.01);
                    depthValue = saturate((eyeDepth - _DepthRangeNear) / range);
                }

                // Apply tunable contrast curve
                if (_Contrast != 1.0)
                {
                    depthValue = pow(max(depthValue, 0.0), _Contrast);
                }

                // Invert depth:
                // Default (_InvertDepth = 1.0): Near objects are BRIGHT (White), Far objects are DARK (Black).
                // Classic computer graphics visualization where high intensity = close to camera.
                // If _InvertDepth = 0.0: Near objects are DARK (Black), Far objects are BRIGHT (White).
                if (_InvertDepth > 0.5)
                {
                    depthValue = 1.0 - depthValue;
                }

                return fixed4(depthValue, depthValue, depthValue, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}
