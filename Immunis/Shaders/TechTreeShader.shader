// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TechTree"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		_Color("Color", Color) = (0,0,0,0)
		_ResearchedColor ("Color (researched)", Color) = (0,0,0,0)
		_Factor ("Blend factor", Float) = 0

		_WaveStrength ("Wave strength", Float) = 0
		_WaveSpeed ("Wave speed", Float) = 5
		_AlphaOffset ("Alpha offset", Float) = 0.05

		_HiddenFactor("Hidden factor", Float) = 1
	}

	SubShader
	{
		Blend One One
		ZWrite Off
		Cull Off

		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 screenuv : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				float3 objectPos : TEXCOORD3;
				float4 vertex : SV_POSITION;
				float depth : DEPTH;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.screenuv = ((o.vertex.xy / o.vertex.w) + 1)/2;
				o.screenuv.y = 1 - o.screenuv.y;
				o.depth = -mul(UNITY_MATRIX_MV, v.vertex).z *_ProjectionParams.w;

				o.objectPos = v.vertex.xyz;		
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex)));

				return o;
			}
			
			sampler2D _CameraDepthNormalsTexture;
			fixed4 _Color;
			fixed4 _ResearchedColor;
			fixed _Factor;
			fixed _WaveStrength;
			fixed _WaveSpeed;
			fixed _AlphaOffset;
			fixed _HiddenFactor;

			float triWave(float t, float offset, float yOffset)
			{
				return saturate(abs(frac(offset + t) * 2 - 1) + yOffset);
			}

			fixed4 texColor(v2f i)
			{
				fixed4 color;
				fixed timeReverse;
				
				if (i.uv.x < _Factor)
				{
					color = _ResearchedColor;
					timeReverse = -1;
				}
				else
				{
					color = _Color;
					timeReverse = 1;
				}

				fixed waveFactor = triWave(_Time.y * _WaveSpeed * timeReverse, i.uv.x, -_WaveStrength) + _AlphaOffset;
				color.r *= waveFactor;
				color.g *= waveFactor;
				color.b *= waveFactor;

				return color;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				if (i.uv.x >= _HiddenFactor)
				{
					return fixed4(0, 0, 0, 1);
				}

				float screenDepth = DecodeFloatRG(tex2D(_CameraDepthNormalsTexture, i.screenuv).zw);
				float diff = screenDepth - i.depth;
				float intersect = 0;
				
				if (diff > 0)
					intersect = 1 - smoothstep(0, _ProjectionParams.w * 0.5, diff);

				float rim = 1 - abs(dot(i.normal, normalize(i.viewDir))) * 2;
				float northPole = (i.objectPos.y - 0.45) * 20;
				float glow = max(max(intersect, rim), northPole);

				fixed4 glowColor = fixed4(lerp(_Color.rgb, fixed3(1, 1, 1), pow(glow, 4)), 1);
				
				fixed4 mainTex = texColor(i);

				fixed4 col = mainTex * mainTex.a + glowColor * glow + mainTex;
				return col;
			}
			ENDCG
		}
	}
}
