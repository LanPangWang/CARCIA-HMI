Shader "Unlit/shapeGeometryShader"
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
        _Color ("Color", Color) = (1.0,1.0,1.0,1.0)
        _CrossFadeLength ("Cross Fade Length", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags {"RenderType"="transparent" "QUEUE"="transparent" "LightMode" = "ForwardBase" "PerformanceChecks"="False" "DisableBatching"="true"}
        LOD 100

		ZWrite [_ZWriteMode]
		ZTest [_ZTestMode]
        Pass
        {
			Cull [_CullMode]
			Blend [_SrcBlend] [_DstBlend]
            CGPROGRAM
            #pragma vertex vert
            #pragma hull HullS
            #pragma domain DomainS
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
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord: TEXCOORD0;
                uint vid : SV_VertexID;
            };
            struct Attribute
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord: TEXCOORD0;
                uint vid : VID;
            };
            struct TrianglePatchTess
            {
                float edgeTess[3] : SV_TessFactor; 
                float insideTess : SV_InsideTessFactor;
            };
            struct HullOutput
            {
                float4 positionOS : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 uv : TEXCOORD1;
                uint index : INDEX;
            };

            struct DomainOutput
            {
                float4 positionOS : SV_POSITION;
                float3 normal : TEXCOORD0;
                float4 tangent : TEXCOORD1;
                float4 uv : TEXCOORD2;
                bool isEdge1 : ISEDGE1;
                bool isEdge2 : ISEDGE2;
                bool isEdge3 : ISEDGE3;
                float3 objPos : TEXCOORD3;
                float3 triPoints[3] : TRIPOINTS;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _Color;
            float _CrossFadeLength;
    
            Attribute vert(appdata v)
            {

                Attribute output;
                output.vertex = v.vertex;
                output.normal = v.normal;
                output.tangent = v.tangent;
                output.texcoord = v.texcoord;
                output.vid = v.vid;
                return output;
            }

            [domain("tri")] 
            [partitioning("integer")] 
            [outputtopology("triangle_cw")]  
            [outputcontrolpoints(3)] 
            [patchconstantfunc("ComputeTessFactor")] 
            [maxtessfactor(64.0)] 

            HullOutput HullS(InputPatch<Attribute, 3> input, uint controlPointId : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
            {
                HullOutput output;
    
                output.positionOS = input[controlPointId].vertex;
                // output.uv = input[controlPointId].uv;
                output.uv.xy = input[controlPointId].texcoord;
                output.uv.zw = 0;
                output.normal = input[controlPointId].normal;
                output.tangent = input[controlPointId].tangent;
                // output.index = 0;
                output.index = input[controlPointId].vid;
                return output;
            }

            TrianglePatchTess ComputeTessFactor(InputPatch<Attribute, 3> patch, uint patchId : SV_PrimitiveID)
            {
                TrianglePatchTess output;
                output.edgeTess[0] = 1;
                output.edgeTess[1] = 1;
                output.edgeTess[2] = 1;
                output.insideTess = 1;

                return output;
            }
            
            [domain("tri")]
            DomainOutput DomainS(TrianglePatchTess patchTess, float3 bary: SV_DomainLocation, const OutputPatch<HullOutput, 3> patch)
            {
                DomainOutput output;

				float4 positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z; 
				float4 uv = patch[0].uv * bary.x + patch[1].uv * bary.y + patch[2].uv * bary.z; 
				float4 tangentOS = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z; 
				float3 normalOS = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z; 
                bool isEdge1 = (abs(patch[1].index - patch[0].index) == 1) || (patch[1].uv.y == 0.5 && patch[0].uv.y == 0.5);
                bool isEdge2 = (abs(patch[2].index - patch[0].index) == 1) || (patch[2].uv.y == 0.5 && patch[0].uv.y == 0.5);
                bool isEdge3 = (abs(patch[2].index - patch[1].index) == 1) || (patch[2].uv.y == 0.5 && patch[1].uv.y == 0.5);

                output.positionOS = UnityObjectToClipPos(positionOS);
                output.uv.xy = 0;
                output.uv.zw = 0;
                output.normal = normalOS;
                output.tangent = tangentOS;
                output.isEdge1 = isEdge1;
                output.isEdge2 = isEdge2;
                output.isEdge3 = isEdge3;
                output.objPos = positionOS.xyz;
                output.triPoints[0] = patch[0].positionOS.xyz;
                output.triPoints[1] = patch[1].positionOS.xyz;
                output.triPoints[2] = patch[2].positionOS.xyz;

				return output;
            }
            fixed4 frag (DomainOutput i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2Dlod(_MainTex, float4(i.uv.xy, 0, 0));
                col *= _Color;
                float minDis = length(i.objPos - i.triPoints[0]);
                minDis = min(minDis, length(i.objPos - i.triPoints[1]));
                minDis = min(minDis, length(i.objPos - i.triPoints[2]));
                col.a *= smoothstep(0, _CrossFadeLength, minDis);
                return col;
            }
            ENDCG
        }
    }
}
