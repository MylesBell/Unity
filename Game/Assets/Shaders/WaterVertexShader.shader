Shader "Custom/WaterVertexShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
        _WaveWavelength ("Wave Wavelength", Range(0,1) ) = 0.1
        _WaveMagnitude ("Wave Magnitude", Range(0,1)) = 0.1
        _WaveDirection ("Wave Direction", Vector) = (0,1,0)
        // _SnowDepth ("Snow Depth", Range(0,0.5)) = 0.1
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		
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
		float4 _WaveDirection;
		
		 float rand(float3 co)
		{
			return frac(sin( dot(co.xyz ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
		}

        void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
            //Convert the normal to world coortinates
            float3 wnormal = normalize(_WaveDirection.xyz);
            float3 wn = mul((float3x3)_World2Object, wnormal).xyz;
 
 			float r = rand(v.vertex.xyz);
			v.vertex.y += r;
            // if(dot(v.normal, sn) >= lerp(1,-1, (_Snow*2)/3)
			// && r >= 0.5)
            // {
            //    v.vertex.xyz += normalize(sn + v.normal) * _SnowDepth * _Snow;
            // }
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
