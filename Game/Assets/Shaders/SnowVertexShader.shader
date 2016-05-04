Shader "Custom/SnowVertexShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
        _Snow ("Snow Level", Range(0,1) ) = 0
        _SnowColor ("Snow Color", Color) = (1.0,1.0,1.0,1.0)
        _SnowDirection ("Snow Direction", Vector) = (0,1,0)
        // _SnowDepth ("Snow Depth", Range(0,0.5)) = 0.1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float r;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
        float _Snow;
        float4 _SnowColor;
        float4 _SnowDirection;
        float _SnowDepth;
		
		 float rand(float3 co)
		{
			return frac(sin( dot(co.xyz ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
		}

        void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
            //Convert the normal to world coortinates
            float3 snormal = normalize(_SnowDirection.xyz);
            float3 sn = mul((float3x3)_World2Object, snormal).xyz;
 
 			float r = rand(v.vertex.xyz);
			o.r = r;
            // if(dot(v.normal, sn) >= lerp(1,-1, (_Snow*2)/3)
			// && r >= 0.5)
            // {
            //    v.vertex.xyz += normalize(sn + v.normal) * _SnowDepth * _Snow;
            // }
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			if(dot(WorldNormalVector(IN, o.Normal), _SnowDirection.xyz)>=lerp(1,-1,_Snow)
				&& IN.r >= (1 - _Snow))
            {
                o.Albedo = _SnowColor.rgb;
				o.Metallic = 0.0;
				o.Smoothness = 0.0;
				o.Alpha = 0.0;
            }
            else {
                o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
            }
			// Metallic and smoothness come from slider variables
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
