Shader "Hidden/getFreeSpaceShader"
{
    SubShader
    {
        Tags {"QUEUE"="Transparent" "RenderType" = "Transparent"}   
        Blend  SrcAlpha OneMinusSrcAlpha 
		Cull off 
		Lighting Off 
		ZTest less
		ZWrite off
		Blend one Zero
		Fog{ Mode Off }
        LOD 100

        Pass
        {
            Tags{"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				return o;
			}

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1,1,1,1);
            }
            ENDCG
        }
    } 
}
