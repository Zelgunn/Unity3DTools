// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Scene Preview"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0,0,0,0)

		_Rim1Color("Rim1 Color", Color) = (0,0,0,0)
		_Rim1Width("Rim1 width", Range(0,1)) = 0.1
		_Rim1Strength("Rim1 strength", Range(0,1)) = 0.5
		_Rim1WaveSpeed("Rim1 Wave Speed", Float) = 1
		_Rim1WaveCount("Rim1 Wave Count", Float) = 18
		_Rim1WaveScale("Rim1 Wave Scale", Range(0 , 0.1)) = 0.01

		_Rim2Color("Rim2 Color", Color) = (0,0,0,0)
		_Rim2Width("Rim2 width", Range(0,1)) = 0.1
		_Rim2Strength("Rim2 strength", Range(0,1)) = 0.5
		_Rim2WaveSpeed("Rim2 Wave Speed", Float) = -2
		_Rim2WaveCount("Rim2 Wave Count", Float) = 18
		_Rim2WaveScale("Rim2 Wave Scale", Range(0 , 0.1)) = 0.01
	}

	SubShader
	{
		Blend One One
		ZWrite Off
		Cull Off

		Tags
		{
			"RenderType"="Transparent"
			"Queue"="Transparent"
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

			fixed4 _Rim1Color;
			fixed _Rim1Width;
			fixed _Rim1Strength;
			fixed _Rim1WaveSpeed;
			fixed _Rim1WaveCount;
			fixed _Rim1WaveScale;

			fixed4 _Rim2Color;
			fixed _Rim2Width;
			fixed _Rim2Strength;
			fixed _Rim2WaveSpeed;
			fixed _Rim2WaveCount;
			fixed _Rim2WaveScale;

			fixed getGlow(fixed2 uv, fixed rimWidth, fixed rimStrength, fixed waveSpeed, fixed waveCount, fixed waveScale)
			{
				fixed2 center = fixed2(0.5, 0.5);
				fixed2 delta = uv - center;

				fixed angle = atan2(delta.x, delta.y);
				angle = (angle + 3.14159265358979) / 6.28318530718;
				angle *= 3.14 * waveCount;
				angle = abs(sin(angle + _Time.y * waveSpeed));

				delta *= 1 + (angle - 0.5) * waveScale;

				fixed glow = sqrt(delta.x * delta.x + delta.y * delta.y) * 2;

				if (glow > (1 - rimWidth))
				{
					glow += rimWidth - 1;
					glow /= rimWidth;
					glow *= rimStrength;
				}
				else
				{
					glow = 0;
				}

				return glow;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 rim1 = _Rim1Color * getGlow(i.uv, _Rim1Width, _Rim1Strength, _Rim1WaveSpeed, _Rim1WaveCount, _Rim1WaveScale);
				fixed4 rim2 = _Rim2Color * getGlow(i.uv, _Rim2Width, _Rim2Strength, _Rim2WaveSpeed, _Rim2WaveCount, _Rim2WaveScale);

				fixed4 mainTex = tex2D(_MainTex, i.uv);
				fixed4 col = mainTex * _Color * _Color.a + rim1 + rim2;
				return col;
			}
			ENDCG
		}
	}
}
