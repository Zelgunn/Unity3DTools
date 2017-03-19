// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/PlayerVRCursor"
{
	Properties
	{
		//_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_BaseColor("Base color", Color) = (1, 0.75, 0, 1)

		_OutlineColor("Outline color", Color) = (1, 0.5, 0, 1)
		_OutlineLeft("Outline (left)", Range(0,1)) = 0.1
		_OutlineRight("Outline (right)", Range(0,1)) = 0.9

		_SpaceColor("Space color", Color) = (1, 1, 1, 0)
		_SpacePeriod("Space period", Float) = 10
		_SpaceWidth("Space period", Range(0, 3.14)) = 0.1

		_OutlineSpace("Outline (space)", Range(0, 3.14)) = 0.1

		_Speed("Speed", Float) = 5
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}
		LOD 100
	
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
	
		Pass
		{  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
			
				#include "UnityCG.cginc"

				struct appdata_t
				{
					fixed4 vertex : POSITION;
					fixed2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					fixed4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
				};

				fixed4 _BaseColor;
				fixed4 _OutlineColor;

				fixed _OutlineLeft;
				fixed _OutlineRight;

				fixed4 _SpaceColor;
				fixed _SpacePeriod;
				fixed _SpaceWidth;

				fixed _OutlineSpace;

				fixed _Speed;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = v.texcoord;
					return o;
				}
			
				fixed4 frag (v2f i) : SV_Target
				{
					fixed spacePeriodTest = abs(sin((i.texcoord.y - _Time.x * _Speed) * _SpacePeriod));
					if (spacePeriodTest < _SpaceWidth)
					{
						return fixed4(0, 0, 0, 0);
					}
					if ((i.texcoord.x > _OutlineRight) || (i.texcoord.x < _OutlineLeft) || (spacePeriodTest < (_SpaceWidth + _OutlineSpace)))
					{
						return _OutlineColor;
					}

					return _BaseColor;
				}
			ENDCG
		}
	}
}
