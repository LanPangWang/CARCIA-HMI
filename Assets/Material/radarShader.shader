Shader "Unlit/radarShader"
{
    Properties
    {
		[Enum(front, 0, back, 1, double, 2)] _Side ("Side", float) = 1
		[Enum(Off, 0, On, 1)] _ZWriteMode ("ZWriteMode", float) = 1
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("CullMode", float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTestMode ("ZTestMode", float) = 4
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", float) = 10
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("BlendOp", float) = 0
        _Color ("Color", Color) = (1.0,1.0,1.0,1.0)
        _Color2 ("Color2", Color) = (1.0,1.0,1.0,1.0)
        _DisplaySpeed ("DisplaySpeed", Range(-100, 100)) = 1
        _Rate ("Rate", Range(0.1, 10)) = 1
        _AlphaCull ("Alpha Cull", Range(0, 1)) = 0.5
        _AngleStartCull ("Angle Start Cull", Range(0, 1)) = 0
        _AngleEndCull ("Angle End Cull", Range(0, 1)) = 0
        _DistanceStartCrossFade("Distance Start Cross Fade", Range(0, 1)) = 1
        _DistanceEndCrossFade("Distance End Cross Fade", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags {"RenderType"="transparent" "QUEUE"="transparent+1" "PerformanceChecks"="False"}
        LOD 100

		ZWrite [_ZWriteMode]
		ZTest [_ZTestMode]
        Pass
        {
			Cull [_CullMode]
			Blend [_SrcBlend] [_DstBlend]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
			#pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 objectPos : TEXCOORD1;
            };

			fixed4 _Color;
			fixed4 _Color2;
            float _Rate;
            float _DisplaySpeed;
            float _Side;
            float _AlphaCull;
            float _AngleStartCull;
            float _AngleEndCull;
            float _DistanceStartCrossFade;
            float _DistanceEndCrossFade;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.objectPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float distance = 1 - i.uv.x;
                float distanceOffset = distance - _Time.x * _DisplaySpeed;
                fixed rate = 1 / _Rate;
                float AlphaCompute = smoothstep(0, rate, abs(distanceOffset) % rate);
                AlphaCompute = _Side > 0 ? (_Side > 1 ? max(AlphaCompute, 1 - AlphaCompute) : 1 - AlphaCompute) : AlphaCompute;
                AlphaCompute = smoothstep(_AlphaCull, 1, AlphaCompute);

                float uvyCompute = (i.uv.y * 4) % 2;
                uvyCompute = uvyCompute > 1 ? 2 - uvyCompute : uvyCompute;
                AlphaCompute *= smoothstep(_AngleStartCull, _AngleEndCull, uvyCompute);
                AlphaCompute *= 1 - smoothstep(_DistanceStartCrossFade, _DistanceEndCrossFade, distance);

                fixed4 col = lerp(_Color, _Color2, distance);
                col.a *= AlphaCompute;
                return col;
            }
            ENDCG
        }
    }
}
