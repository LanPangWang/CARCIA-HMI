// SimpleSonarShader 脚本和着色器由 Drew Okenfuss 编写。
// 要使此着色器工作，必须从 SimpleSonarShader_Parent.cs 脚本中向对象传递值。
// 默认情况下，通过将对象设置为 SimpleSonarShader_Parent 的子对象来实现这一点。
Shader "Custom/SimpleSonarShader" {
	Properties{
		// 主颜色
        _Color("Color", Color) = (1,1,1,1)
        // 主纹理
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        // 光滑度
        _Glossiness("Smoothness", Range(0,1)) = 1.0
        // 金属度
        _Metallic("Metallic", Range(0,1)) = 0.0
        // 环颜色
        _RingColor("Ring Color", Color) = (1,1,1,1)
        // 环颜色强度
        _RingColorIntensity("Ring Color Intensity", float) = 2
        // 环速度
        _RingSpeed("Ring Speed", float) = 1
        // 环宽度
        _RingWidth("Ring Width", float) = 0.1
        // 环范围
        _RingIntensityScale("Ring Range", float) = 1
        // 环纹理
        _RingTex("Ring Texture", 2D) = "white" {}
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
        // 基于物理的标准光照模型，并在所有光源类型上启用阴影
#pragma surface surf Standard fullforwardshadows

        // 使用着色器模型3.0目标，以获得更好的光照效果
#pragma target 3.0

	sampler2D _MainTex;
	sampler2D _RingTex;


	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
	};

    // 这些数组的大小是一次可以渲染的环数。
    // 如果要更改此设置，还必须更改 SimpleSonarShader_Parent.cs 中的 QueueSize
	half4 _hitPts[20];
	half _StartTime;
	half _Intensity[20];

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;
	fixed4 _RingColor;
	half _RingColorIntensity;
	half _RingSpeed;
	half _RingWidth;
	half _RingIntensityScale;

	// Surface函数，用于渲染物体表面
    void surf(Input IN, inout SurfaceOutputStandard o) {
        // 通过主纹理和主颜色计算表面颜色
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        // 将颜色赋值给表面输出的漫反射颜色
        o.Albedo = c.rgb;

        // 初始化与环颜色的差异
        half DiffFromRingCol = abs(o.Albedo.r - _RingColor.r) + abs(o.Albedo.b - _RingColor.b) + abs(o.Albedo.g - _RingColor.g);

        // 遍历环数据数组中的每个点
        for (int i = 0; i < 20; i++) {
            // 计算当前点与世界坐标位置的距离 -1.0是为了让初始圆环的大小不是0而是1
            half d = distance(_hitPts[i], IN.worldPos) - 1.0;
            // 计算环的强度
            half intensity = _Intensity[i] * _RingIntensityScale;
            // 计算当前点的值
            half val = (1 - (d / intensity));

            // 检查当前点是否在环的范围内
            if (d < (_Time.y - _hitPts[i].w) * _RingSpeed && d >(_Time.y - _hitPts[i].w) * _RingSpeed - _RingWidth && val > 0) {
                // 计算当前点在环中的位置比例
                half posInRing = (d - ((_Time.y - _hitPts[i].w) * _RingSpeed - _RingWidth)) / _RingWidth;

                // 计算采样环纹理径向的预测 RGB 值
                float angle = acos(dot(normalize(IN.worldPos - _hitPts[i]), float3(1,0,0)));
                val *= tex2D(_RingTex, half2(1 - posInRing, angle));
                // 计算表面颜色
                half3 tmp = _RingColor * val + c * (1 - val);

                // 计算当前预测值与环颜色之间的差异
                half tempDiffFromRingCol = abs(tmp.r - _RingColor.r) + abs(tmp.b - _RingColor.b) + abs(tmp.g - _RingColor.g);
                // 如果当前预测值更接近环颜色，则更新表面颜色
                if (tempDiffFromRingCol < DiffFromRingCol) {
                    DiffFromRingCol = tempDiffFromRingCol;
                    o.Albedo.r = tmp.r;
                    o.Albedo.g = tmp.g;
                    o.Albedo.b = tmp.b;
                    // 调整表面颜色的强度
                    o.Albedo.rgb *= _RingColorIntensity;
                }
            }
        }

        // 设置表面的金属度和光滑度
        o.Metallic = _Metallic;
        o.Smoothness = _Glossiness;
    }

	ENDCG
	}
		FallBack "Diffuse"
}