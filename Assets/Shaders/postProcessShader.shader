Shader "Unlit/postProcessShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_BlurMask("Blur Mask", 2D) = "white"{}
		_BlurPixels ("Blur Pixels", Range(1,21)) = 1
    }

    CGINCLUDE
    #include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	sampler2D _BlurMask;
	fixed4 _BlurMask_TexelSize;
	fixed _BlurPixels;

	struct v2f_blur
	{
		float4 pos : SV_POSITION;
		float2 uv  : TEXCOORD0;
		//float4 uv01 : TEXCOORD1;
		//float4 uv23 : TEXCOORD2;
		//float4 uv45 : TEXCOORD3;
	};

	v2f_blur vert_blur(appdata_img v)
	{
		v2f_blur o;
		//_offsets *= _MainTex_TexelSize.xyxy;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
 	
		//o.uv01 = v.texcoord.xyxy + _offsets.xyxy * float4(1, 1, -1, -1);
		//o.uv23 = v.texcoord.xyxy + _offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
		//o.uv45 = v.texcoord.xyxy + _offsets.xyxy * float4(1, 1, -1, -1) * 3.0;
 
		return o;
	}
 
    fixed luminance(fixed4 color)
    {
        return 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
    }

    half Sobel(float2 mainUV)
    {
        const half Gx[9] = {-1, 0, 1,
                            -2, 0, 2,
                            -1, 0, 1};

        const half Gy[9] = {-1, -2, -1,
                            0, 0, 0,
                            1, 2, 1};
        
        half texColor;
        half edgeX = 0;
        half edgeY = 0;
        int it = 0;
        for (int i = -1; i < 2; i += 1)
        {
            for (int j = -1; j < 2; j += 1)
            {
                texColor = luminance(tex2Dlod(_MainTex, float4(mainUV + _MainTex_TexelSize.xy * half2(i, j), 0, 0)));
                edgeX += texColor * Gx[it];
                edgeY += texColor * Gy[it];
                it += 1;
            }
        }
        return 1 - abs(edgeX) - abs(edgeY);
    }

	fixed4 frag_blur(v2f_blur input) : SV_Target
	{
        half edge = Sobel(input.uv);
        // return fixed4(1 - saturate(edge), 0, 0, 1);
        float samplerPixels = lerp(1, _BlurPixels, 1 - saturate(edge));
        fixed4 final = fixed4(0, 0, 0, 1);
        float alpha = 0;
        for(fixed i = 0; i < samplerPixels; i += 1)
        {
            for(fixed j = 0; j < samplerPixels; j += 1)
            {
                float2 maskUV = float2((i + 0.5) / samplerPixels, (j + 0.5) / samplerPixels);
                float2 mainUV = float2(input.uv.x + ((maskUV.x - 0.5) * samplerPixels * _MainTex_TexelSize.x), input.uv.y + ((maskUV.y - 0.5) * samplerPixels * _MainTex_TexelSize.y));
                // float2 mainUV = float2(i.uv.x , i.uv.y );
                float alphaBlur = tex2Dlod(_BlurMask, float4(maskUV, 0, 0)).a;
                alpha += alphaBlur;
                final.rgb += tex2Dlod(_MainTex, float4(mainUV, 0, 0)).rgb * alphaBlur;
            }
        }
        final.rgb *= 1 / alpha;
        
		return final;
	}

	ENDCG
    SubShader
    {
        Tags { "RenderType" = "Opaque" "PerformanceChecks"="False" }
        LOD 100


        Pass
        {
			ZTest Off
			Cull Off
			ZWrite Off
			Fog{ Mode Off }
 
			CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_blur
			ENDCG
        }
    }
}
