Shader "Unlit/avmSlotShader"
{
    Properties
    {
		[Enum(Off, 0, On, 1)] _ZWriteMode ("ZWriteMode", float) = 1
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("CullMode", float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTestMode ("ZTestMode", float) = 4
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", float) = 10
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("BlendOp", float) = 0
        _MainTex ("Texture", 2D) = "white" {}
        _Color1 ("Color1", Color) = (1.0,1.0,1.0,1.0)
        _Color2 ("Color2", Color) = (1.0,1.0,1.0,1.0)
    }
    SubShader
    {
        Tags {"RenderType"="transparent" "QUEUE"="transparent+1" "PerformanceChecks"="False" }
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
                float3 worldpos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _Color1;
			fixed4 _Color2;
            sampler2D _AvmCameraFreeSpaceTexture;
            float4 _AVMCameraPos;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed alpha = tex2Dlod(_MainTex, float4(i.uv, 0, 0)).a;
                float2 scrpos = float2(0, 0);
                scrpos.x = ((i.worldpos.x - _AVMCameraPos.x) + _AVMCameraPos.w) / (2 * _AVMCameraPos.w);
                scrpos.y = ((i.worldpos.z - _AVMCameraPos.z) + _AVMCameraPos.w) / (2 * _AVMCameraPos.w);
                fixed freeSpaceAlpha = tex2Dlod(_AvmCameraFreeSpaceTexture, float4(scrpos, 0, 0)).a;
                // fixed3 col = lerp(_Color1, _Color2, freeSpaceAlpha);
                fixed3 col = _Color1 * (1 - freeSpaceAlpha) + _Color2 * freeSpaceAlpha;
                return fixed4(col, alpha);
                // return fixed4(i.worldpos.x > -8, 0, 0, 1);
            }
            ENDCG
        }
    }
}
