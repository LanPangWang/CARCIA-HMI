Shader "Unlit/postProcessShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TAAAlpha ("TAA alpha", Range(0.01, 1)) = 0.1
        _AABBClipSize ("AABB Clip Size", Range(0, 1)) = 1
		// _BlurMask("Blur Mask", 2D) = "white"{}
		_BlurPixels ("Blur Pixels", Range(1,21)) = 1
        _EdgeOffset ("Edge Offset", Range(0, 10)) = 1
        _EdgeThreshold ("Edge Threshold", Range(-10, 10)) = 0
        _Sigma ("Sigma", Range(0.1, 10)) = 1
        // _LplsOffset ("lpls offset", Range(1, 16)) = 1
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    #define kr 0.299
    #define kb 0.114
    #define kg 0.587

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	sampler2D _BlurMask;
	fixed4 _BlurMask_TexelSize;
	fixed _BlurPixels;
    fixed _EdgeOffset;
    fixed _EdgeThreshold;
    fixed _Sigma;
    fixed _LplsOffset;


    float4x4 _MainWorldToCamera;
    float4x4 _MainCameraToWorld;
    float4x4 _MainCameraProjection;
    float4x4 _MainCameraInvProjection;
    sampler2D _MainCameraRGBAPre;
    float4 _MainCameraRGBAPre_TexelSize;
    sampler2D _MainCameraDepthTexture;
    sampler2D _CameraDepthTexture;
    float _MainCameraFarClip;
    fixed _TAAAlpha;
    fixed _AABBClipSize;
    // sampler2D _MainCameraOceanDepth;
    // float _SSRDistance;
    
    sampler2D _RefractionTex_Ocean_back;
    float4 _RefractionTex_Ocean_back_TexelSize;

	struct v2f_blur
	{
		float4 pos : SV_POSITION;
		float2 uv  : TEXCOORD0;
		//float4 uv01 : TEXCOORD1;
		//float4 uv23 : TEXCOORD2;
		//float4 uv45 : TEXCOORD3;
	};


    struct v2f
    {
        float4 pos : SV_POSITION;
        float2 uv  : TEXCOORD0;
    };
    
    v2f vert(appdata_img v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv.xy = v.texcoord.xy;

        return o;
    }

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

    fixed3 RGBtoYCbCr(fixed3 color)
    {
        // fixed Y = kr * color.r + kg * color.g + kb * color.b;
        // fixed Cb = 0.5 * ((1 - kb) * (color.b - Y));
        // fixed Cr = 0.5 * ((1 - kr) * (color.r - Y));
        fixed Y = 0.299 * color.r + 0.587 * color.g + 0.114 * color.b;
        fixed Cb = 0.564 * (color.b - Y);
        fixed Cr = 0.713 * (color.r - Y);
        return fixed3(Y, Cb, Cr);
        // return color;
    }

    fixed3 YCbCrtoRGB(fixed3 YCbCr)
    {
        fixed Y = YCbCr.r;
        fixed Cb = YCbCr.g;
        fixed Cr = YCbCr.b;
        // fixed R = Y + (1 - kr) * 0.5 * Cr;
        // fixed B = Y + (1 - kb) * 0.5 * Cb;
        // fixed G = (Y - kr * R - kb * B) / kg;
        fixed R = Y + (1 / 0.713) * Cr;
        fixed B = Y + (1 / 0.564) * Cb;
        fixed G = (Y - 0.299 * R - 0.114 * B) / 0.587;
        return fixed3(R, G, B);
        // return YCbCr;
    }

    float3 ClipAABB(float3 AABBMin, float3 AABBMax, float3 src, float3 dst)
    {
        float3 dir = dst - src;
        float3 clip = dst;
        clip = clip.x < AABBMin.x ? (smoothstep(clip.x, src.x, AABBMin.x) * dir + src) : clip;
        clip = clip.x > AABBMax.x ? (smoothstep(src.x, clip.x, AABBMax.x) * dir + src) : clip;
        clip = clip.y < AABBMin.y ? (smoothstep(clip.y, src.y, AABBMin.y) * dir + src) : clip;
        clip = clip.y > AABBMax.y ? (smoothstep(src.y, clip.y, AABBMax.y) * dir + src) : clip;
        clip = clip.z < AABBMin.z ? (smoothstep(clip.z, src.z, AABBMin.z) * dir + src) : clip;
        clip = clip.z > AABBMax.z ? (smoothstep(src.z, clip.z, AABBMax.z) * dir + src) : clip;
        return clip;
    }

    half Sobel(float2 mainUV)
    {
        const half Gx[9] = {-1, 0, 1,
                            -2, 0, 2,
                            -1, 0, 1};
        const half Gy[9] = {-1, -2, -1,
                            0, 0, 0,
                            1, 2, 1};
        
        // half3 texColor;
        // half3 edgeX = half3(0, 0, 0);
        // half3 edgeY = half3(0, 0, 0);
        half texColor;
        half edgeX = 0;
        half edgeY = 0;
        int it = 0;
        for (int i = -1; i < 2; i += 1)
        {
            for (int j = -1; j < 2; j += 1)
            {
                texColor = luminance(tex2Dlod(_MainTex, float4(mainUV + _MainTex_TexelSize.xy * half2(i, j), 0, 0)));
                // texColor = tex2Dlod(_MainTex, float4(mainUV + _MainTex_TexelSize.xy * half2(i, j), 0, 0)).rgb;
                edgeX += texColor * Gx[it];
                edgeY += texColor * Gy[it];
                it += 1;
            }
        }
        return 1 - abs(edgeX) - abs(edgeY);
        // return 1 - max(abs(edgeX.r) - abs(edgeY.r), max(abs(edgeX.g) - abs(edgeY.g), abs(edgeX.b) - abs(edgeY.b)));
    }

	fixed4 frag_blur(v2f_blur input) : SV_Target
	{
        half edge = Sobel(input.uv) * _EdgeOffset - _EdgeThreshold;
        // return fixed4(1 - saturate(edge), 0, 0, 1);
        float samplerPixels = lerp(1, _BlurPixels, 1 - saturate(edge));
        // float samplerPixels = _BlurPixels;
        // return fixed4(samplerPixels / _BlurPixels, 0, 0, 1);
        fixed4 final = fixed4(0, 0, 0, 1);
        float alpha = 0;
        for(fixed i = 0; i < samplerPixels; i += 1)
        {
            for(fixed j = 0; j < samplerPixels; j += 1)
            {
                float2 maskUV = float2((i + 0.5) / samplerPixels, (j + 0.5) / samplerPixels);
                maskUV -= float2(0.5, 0.5);
                maskUV *= samplerPixels;

                // float2 mainUV = float2(input.uv.x + ((maskUV.x - 0.5) * samplerPixels * _MainTex_TexelSize.x), input.uv.y + ((maskUV.y - 0.5) * samplerPixels * _MainTex_TexelSize.y));
                float2 mainUV = float2(input.uv.x + (maskUV.x * _MainTex_TexelSize.x), input.uv.y + (maskUV.y * _MainTex_TexelSize.y));
                // float2 mainUV = float2(i.uv.x , i.uv.y );
                // float alphaBlur = tex2Dlod(_BlurMask, float4(maskUV, 0, 0)).a;

                float alphaBlur = (pow(2.7182818, -(maskUV.x * maskUV.x + maskUV.y * maskUV.y)/(2 * _Sigma * _Sigma)))/(2 * 3.1415926 * _Sigma * _Sigma);
                alpha += alphaBlur;
                final.rgb += tex2Dlod(_MainTex, float4(mainUV, 0, 0)).rgb * alphaBlur;
            }
        }
        final.rgb *= 1 / alpha;

        // fixed4 final = tex2Dlod(_MainTex, float4(input.uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y), 0, 0)) * 0.0625;
        // final += tex2Dlod(_MainTex, float4(input.uv + float2(0, -_MainTex_TexelSize.y), 0, 0)) * 0.125;
        // final += tex2Dlod(_MainTex, float4(input.uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y), 0, 0)) * 0.0625;

        // final += tex2Dlod(_MainTex, float4(input.uv + float2(-_MainTex_TexelSize.x, 0), 0, 0)) * 0.125;
        // final += tex2Dlod(_MainTex, float4(input.uv + float2(0, 0), 0, 0)) * 0.25;
        // final += tex2Dlod(_MainTex, float4(input.uv + float2(_MainTex_TexelSize.x, 0), 0, 0)) * 0.125;
        
        // final += tex2Dlod(_MainTex, float4(input.uv + float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y), 0, 0)) * 0.0625;
        // final += tex2Dlod(_MainTex, float4(input.uv + float2(0, _MainTex_TexelSize.y), 0, 0)) * 0.125;
        // final += tex2Dlod(_MainTex, float4(input.uv + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y), 0, 0)) * 0.0625;
        
		return final;
        // return lerp(tex2Dlod(_MainTex, float4(input.uv, 0, 0)), final, 1 - saturate(edge));
	}

	fixed4 frag_lpls(v2f_blur input) : SV_Target
    {
        const fixed lpls[3][3] = {{-1, -1, -1}, 
                                {-1, 8 + _LplsOffset, -1}, 
                                {-1, -1, -1}};
        fixed4 final = fixed4(0, 0, 0, 1);
        for (fixed i = -1; i < 2; i += 1)
        {
            for (fixed j = -1; j < 2; j += 1)
            {
                final.rgb += lpls[i + 1][j + 1] * tex2Dlod(_MainTex, float4(input.uv + (float2(i, j) * _MainTex_TexelSize.xy), 0, 0)).rgb;
            }
        }
        final.rgb /= _LplsOffset;
        return final;
        // return tex2Dlod(_MainTex, float4(input.uv, 0, 0));
    }

    fixed4 frag_TAA(v2f i) : SV_Target
    {
        // return fixed4(SAMPLE_DEPTH_TEXTURE(_MainCameraDepthTexture, i.uv), 0, 0, 1);
        // float thisDepth = m_DecodeFloatRG(tex2Dlod(_MainCameraOceanDepth, float4(i.uv.xy, 0, 0)));
        // float thisDepth = tex2Dlod(_MainCameraDepthTexture, float4(i.uv.xy, 0, 0)).r;
        float thisDepth = Linear01Depth(tex2Dlod(_CameraDepthTexture, float4(i.uv.xy, 0, 0)).r);
        // float thisDepth = 1;
        // float thisDepth = (tex2Dlod(_MainCameraOceanDepth, float4(i.uv.xy, 0, 0)).x - 1) / _SSRDistance;
        float2 thisNdcPos = i.uv.xy * 2 - 1;
        float3 thisClipVec = float3(thisNdcPos.x, thisNdcPos.y, -1);
        float3 thisViewVec = mul(_MainCameraInvProjection, thisClipVec.xyzz).xyz;
        float3 thisViewPos = thisViewVec * _MainCameraFarClip * thisDepth;
        float3 thisWorldPos = mul(_MainCameraToWorld, float4(thisViewPos, 1)).xyz;
        // return fixed4(thisWorldPos.xyz > 0, 1);
        // thisWroldPos = thisWroldPos - _WorldSpaceCameraPos.xyz;

        float3 lastViewVec = 2 * mul(_MainWorldToCamera, float4(thisWorldPos, 1)).xyz;
        // lastViewVec.z *= -1;
        fixed height = lastViewVec.z / _MainCameraProjection._m11;
        fixed width = _ScreenParams.x / _ScreenParams.y * height;

        float2 lastUV = float2(lastViewVec.x / width, lastViewVec.y / height);
        if (abs(lastUV.x) > 1 || abs(lastUV.y) > 1)
        {
            return tex2Dlod(_MainTex, float4(i.uv, 0, 0));
        }
        lastUV = (lastUV.xy + 1) / 2;
        fixed4 final = tex2Dlod(_MainCameraRGBAPre, float4(lastUV, 0, 0));
        // return fixed4(lastUV.xy > 0.5, 0, 1);
        // return final;
        // float4 final = float4(1, finalEncoded.x % 1, 1, finalEncoded.y % 1);
        // final.x = (finalEncoded.x - final.y) / 255;
        // final.z = (finalEncoded.z - final.w) / 255;
        // final.yw *= 2;
        fixed4 inputFinal = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
        
        float3 AABBMin = RGBtoYCbCr(inputFinal.rgb);
        // float3 AABBMin = inputFinal.rgb;
        float3 AABBMax = AABBMin;
        // float3 avgYCbCr = fixed3(0, 0, 0);

        for (int j = -1; j < 2; j += 1)
        {
            for (int k = -1; k < 2; k += 1)
            {
                float3 C = RGBtoYCbCr(tex2Dlod(_MainTex, float4(i.uv + _MainTex_TexelSize.xy * float2(j, k), 0, 0)).rgb);
                // float3 C = tex2Dlod(_MainTex, float4(i.uv + _MainTex_TexelSize.xy * float2(j, k), 0, 0)).rgb;
                AABBMin = min(AABBMin, C);
                AABBMax = max(AABBMax, C);
                // avgYCbCr += C;
            }
        }
        
        // avgYCbCr /= 9;
        float3 AABBavg = (AABBMin + AABBMax) / 2;
        float3 AABBDiv = ((AABBMax - AABBMin) / 2) * _AABBClipSize;

        float3 finalYCbCr = RGBtoYCbCr(final.rgb);
        // float3 finalYCbCr = final.rgb;
        float3 outYCbCr =  clamp(finalYCbCr, AABBavg - AABBDiv, AABBavg + AABBDiv);
        // float3 outYCbCr = ClipAABB(AABBavg - AABBDiv, AABBavg + AABBDiv, RGBtoYCbCr(inputFinal.rgb), finalYCbCr);
        
        final.rgb = YCbCrtoRGB(outYCbCr);
        // final.rgb = outYCbCr;
        final = (final * (1 - _TAAAlpha)) + (inputFinal * _TAAAlpha);
        
        return final;
    }

    fixed4 frag_MSAA(v2f i) : SV_Target
    {
        fixed4 final = tex2Dlod(_MainCameraRGBAPre, float4(i.uv, 0, 0));
        fixed4 inputFinal = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
        final = (final * (1 - _TAAAlpha)) + (inputFinal * _TAAAlpha);
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
        Pass
        {
			ZTest Off
			Cull Off
			ZWrite Off
			Fog{ Mode Off }
 
			CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_lpls
			ENDCG
        }
        pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
			Fog{ Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_TAA
			ENDCG
		}
        Pass
        {
			ZTest Off
			Cull Off
			ZWrite Off
			Fog{ Mode Off }
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_MSAA
			ENDCG
        }
    }
}
