Shader "Custom/WaterVertexShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
        _WaveWavelength ("Wave Wavelength", Range(0,1) ) = 0.1
        _WaveMagnitude ("Wave Magnitude", Range(0,1)) = 0.1
		_WaveSpeed ("Wave Speed", Range(0,1)) = 0.1
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "ForceNoShadowCasting"="True"}
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _WaveWavelength;
		float _WaveMagnitude;
		float _WaveSpeed;
		
		 float rand(float3 co)
		{
			return frac(sin( dot(co.xyz ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
		}

        void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			float4 v0 = mul(_Object2World, v.vertex);

			float phase0 = (_WaveMagnitude)* sin((_Time[1] * _WaveSpeed) + (v0.x * _WaveWavelength) + (v0.z * _WaveWavelength) + rand(v0.xzz));
			float phase0_1 = (_WaveMagnitude)*sin(cos(rand(v0.xzz) * _WaveMagnitude * cos(_Time[1] * _WaveSpeed * sin(rand(v0.xxz)))));			
			v0.y += phase0 + phase0_1;

			v.vertex.xyz = mul(_World2Object, v0);
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			// Metallic and smoothness come from slider variables
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
