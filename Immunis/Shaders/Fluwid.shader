Shader "Projet Junior/Fluwid"
{
	Properties
	{
		[HideInInspector] _Mode("__mode", Float) = 0.0
		
		_Color("Color", Color) = (1,1,1,1)
		
		/*
		_ReflDistort ("Reflection distort", Range (0,1.5)) = 0.44
		[NoScaleOffset] _BumpMap ("Normalmap ", 2D) = "bump" {}
		[NoScaleOffset] _ReflectiveColor ("Reflective color (RGB) fresnel (A) ", 2D) = "" {}
		*/

		_FluwidHeight("FluwidHeight", float) = 0
		_CurveFirstDegree("Curve First Degree", float) = 0
		_CurveSecDegree("Curve Sec Degree", float) = 0
		_CurveSinFactor("Curve Sin Factor", float) = 1
		_CurveSinDelta("Curve Sin Delta", float) = 0
		
		//_ReflectionTex ("Internal Reflection", 2D) = "black" {}
	}
	SubShader
	{
		
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		//#pragma surface surf 

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
			
			//float4 ref : TEXCOORD0;
			//float2 bumpuv : TEXCOORD1;
			//float3 viewDir : TEXCOORD2;
		};

		fixed4 _Color;
		/*
		sampler2D _ReflectionTex;
		sampler2D _ReflectiveColor;
		sampler2D _BumpMap;
		
		uniform float _ReflDistort;
		*/

		half _FluwidHeight;
		half _CurveFirstDegree;
		half _CurveSecDegree;
		half _CurveSinFactor;
		half _CurveSinDelta;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			
			//o.Smoothness = _Glossiness;
			//o.Specular = _SpecularMap.a;

			half curveX = sin(IN.worldPos.x * _CurveSinFactor + _CurveSinDelta);
			curveX = curveX * curveX * _CurveSecDegree + curveX * _CurveFirstDegree + _FluwidHeight;
			half curveZ = sin(IN.worldPos.z * _CurveSinFactor + _CurveSinDelta);
			curveZ = curveZ * curveZ * _CurveSecDegree + curveZ * _CurveFirstDegree + _FluwidHeight;

			half curveY = (curveX + curveZ) / 2;

			if (IN.worldPos.y < curveY)
			{
				o.Alpha = _Color.a;
				
				//half4 color;
				
				/*
				half3 bump = UnpackNormal(tex2D(_BumpMap, IN.bumpuv)).rgb;
				float4 uv1 = IN.ref; uv1.xy += bump * _ReflDistort;
				
				half fresnelFac = dot(IN.viewDir, bump);
				half4 refl = tex2Dproj( _ReflectionTex, UNITY_PROJ_COORD(uv1) );
				
				half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
				color.rgb = lerp( _Color.rgb, refl.rgb, water.a / 3);
				color.a = refl.a * water.a;
				
				o.Albedo = color;
				*/
				
				o.Smoothness = 0;
			}
			else
			{
				o.Alpha = 0;
				
				o.Albedo.r = 0;
				o.Albedo.g = 0;
				o.Albedo.b = 0;
				
				o.Specular = 0;
				o.Smoothness = 0;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"

}
