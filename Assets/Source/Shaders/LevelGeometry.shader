﻿Shader "Custom/LevelGeometry"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_MinSize("Min Size", float) = 0.3
		_MaxSize("Max Size", float) = 1.0
		_FalloffDistance("Falloff", float) = 5.0
		_FalloffScale("Falloff Scale", float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		#pragma vertex vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _MinSize;
		float _MaxSize;
		float _FalloffDistance;
		float _FalloffScale;
		uniform float3 _PlayerPosition;
		uniform float3 _PlayerLook;
		uniform int _PlayMode;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

			
		float Remap(float value, float low1, float high1, float low2, float high2)
		{
			return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
		}

		void vert(inout appdata_full v)
		{
			float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
			float dist = distance(worldPos, _PlayerPosition);
			dist = pow(dist, _FalloffScale);
			float scaleInterpolant = 1.0 - saturate(dist / _FalloffDistance);
			float3 dirToPlayer = normalize(worldPos - _PlayerPosition);
			
			// Default to max size if not in play mode
			scaleInterpolant = lerp(scaleInterpolant, 1.0, 1 - _PlayMode);
			v.vertex *= lerp(_MinSize, _MaxSize, scaleInterpolant);
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
