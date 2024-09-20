Shader "Unlit/freeSpaceBlurShader"
{
    Properties
    {
        // _BlurPixels ("Blur Pixels", Range(0, 5)) = 3
        _MainTex ("Texture", 2D) = "white" {}
        
		_BlurPixels ("Blur Pixels", Range(1,121)) = 1
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
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            fixed _BlurPixels;
            fixed _EdgeOffset;
            fixed _EdgeThreshold;

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = v.texcoord.xy;
        
                return o;
            }

            //边缘检测，返回值越小越接近边缘

            fixed4 frag (v2f input) : SV_Target
            {
                float samplerPixels = _BlurPixels;

                fixed distance = 1;

                for(fixed i = 0; i < samplerPixels; i += 1)
                {
                    for(fixed j = 0; j < samplerPixels; j += 1)
                    {
                        float2 maskUV = float2((i + 0.5) / samplerPixels, (j + 0.5) / samplerPixels);
                        maskUV -= float2(0.5, 0.5);
                        maskUV *= samplerPixels;
                        float2 mainUV = float2(input.uv.x + (maskUV.x * _MainTex_TexelSize.x), input.uv.y + (maskUV.y * _MainTex_TexelSize.y));
                        distance = tex2Dlod(_MainTex, float4(mainUV, 0, 0)).a > 0.1 ? min(distance, 2 * length(maskUV) / samplerPixels) : distance;
                    }
                }
                fixed4 final = fixed4(1, 1, 1, 1);
                final *= max(0, (1 - distance));
                
                return final;
            }
            ENDCG
        }
    }
}