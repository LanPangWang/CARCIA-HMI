// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/groudShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1.0,1.0,1.0,1.0)
        _FarColor ("Far Color", Color) = (1.0,1.0,1.0,1.0)
        _CrossFadeStartDistance ("Cross Fade Start Distance", range(0, 1000)) = 200
        _CrossFadeEndDistance ("Cross Fade End Distance", range(0, 1000)) = 1000
        _Specular ("Specular", Range(0, 1)) = 1
		_SpecularScale ("Specular Scale", Range(0,1)) = 0.02
		_SpecularSmoothness ("Specular Smoothness", Range(0,1)) = 0.1
        _Rim ("Rim", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "QUEUE"="geometry-10" "PerformanceChecks"="False"}
		LOD 200

		ZWrite off
		ZTest Less
        Pass
        {
            Cull off
			Tags { "LightMode" = "ForwardBase" "SHADOWSUPPORT"="true" "RenderType"="Opaque" "PerformanceChecks"="False"}
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
				float3 normal : NORMAL;
            };

            struct v2f
            {
				float2 uv : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				SHADOW_COORDS(3)
				UNITY_FOG_COORDS(4)
				float4 scrPos : TEXCOORD5;
				float4 pos : SV_POSITION;
				float depth : DEPTH;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _Color;
            fixed4 _FarColor;
            float4 _RefractionTex_Ocean_TexelSize;
            float _CrossFadeStartDistance;
            float _CrossFadeEndDistance;
            fixed _Specular;
            fixed _SpecularScale;
            fixed _SpecularSmoothness;
            fixed _Rim;

            v2f vert (appdata v)
            {
				v2f o;
                float3 offsetVertex = v.vertex;
                offsetVertex.y += 0.01;
				o.pos = UnityObjectToClipPos(offsetVertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.worldNormal = mul(v.normal,(float3x3)unity_WorldToObject);
				o.worldPos = mul(unity_ObjectToWorld, offsetVertex).xyz;

				UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_SHADOW(o);
                
                // o.scrPos = ComputeScreenPos(o.pos);
				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // float2 srcPosFrac = i.scrPos.xy / i.scrPos.w;

				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				fixed3 worldHalfDir = normalize(worldLightDir + worldViewDir);
                
				fixed diffValue = dot(worldNormal, worldLightDir);
				fixed rimValue = 1 - dot(worldNormal,worldViewDir);
                // rimValue = saturate(-diffValue * rimValue * _Rim);
				fixed rcvShadow = SHADOW_ATTENUATION(i);
                diffValue = min(rcvShadow, saturate(diffValue));
                
				fixed spec = dot(worldNormal, worldHalfDir);
				fixed specular = lerp(0,1,smoothstep(-_SpecularSmoothness,_SpecularSmoothness,spec+_SpecularScale-1)) * step(0.001,_SpecularScale);
                fixed4 specCol = _LightColor0 * specular * _Specular;

                
                fixed3 lightCompute = (_LightColor0 * (diffValue + rimValue * _Rim)) + UNITY_LIGHTMODEL_AMBIENT.rgb;
                // sample the texture
                fixed4 col = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
                col *= _Color;
                col.rgb *= lightCompute;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                col.rgb += specCol.rgb;

                float distance = length(i.worldPos.xz);
                col = lerp(col, _FarColor, smoothstep(_CrossFadeStartDistance, _CrossFadeEndDistance, distance));
                // return fixed4(abs(i.worldPos.x) > 50, 0, 0, 1);
                return col;
            }
            ENDCG
        }
    }
	FallBack "Transparent/Cutout/VertexLit"
    // FallBack "Diffuse"
}
