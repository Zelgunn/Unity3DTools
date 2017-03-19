Shader "Custom/Standard transparent (No ambient)"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}

    SubShader
	{
		Tags  {"Queue" = "Transparent" "RenderType"="Transparent" } 
		CGPROGRAM
		#pragma surface surf Lambert noambient alpha fullforwardshadows
		struct Input
		{
		  float4 color : COLOR;
		};
		
		fixed4 _Color;
		
		void surf (Input IN, inout SurfaceOutput o)
		{
		  o.Albedo = _Color;
		  o.Alpha = _Color.a;
		}
		ENDCG
    }
    Fallback "Diffuse"
}