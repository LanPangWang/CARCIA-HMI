Shader "Unlit/getDepthShader"
{
    SubShader
    {		
        Tags{"QUEUE"="geometry" "RenderType" = "Opaque" }
        Blend  SrcAlpha OneMinusSrcAlpha 
        Cull off 
        Lighting Off 
        ZTest less
        ZWrite on
        Blend one Zero
        Fog{ Mode Off }
        LOD 100

        Pass
        {
            // Tags{"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
      		// #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"

            struct appdata
            {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float4 vertex : SV_POSITION;
				float depth : DEPTH;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.depth = -mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.uv.xy = v.uv;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(i.depth, 0, 0, 1);
            }
            ENDCG
        }
    }
	Fallback "Diffuse"
}
