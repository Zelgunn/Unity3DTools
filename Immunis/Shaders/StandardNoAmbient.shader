Shader "Custom/Standard (No ambient)"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}

    SubShader
	{
		Tags  {"Queue" = "Geometry"}
		CGPROGRAM
		#pragma surface surf Lambert noambient fullforwardshadows
		struct Input
		{
		  float4 color : COLOR;
		};
		
		fixed4 _Color;
		
		void surf (Input IN, inout SurfaceOutput o)
		{
		  o.Albedo = _Color;
		}
		ENDCG
    }
    Fallback "Diffuse"
}