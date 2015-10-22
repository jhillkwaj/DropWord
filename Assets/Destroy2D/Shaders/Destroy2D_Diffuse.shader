Shader "Destroy2D/Diffuse" {
	Properties {
		_Ramp ("Light Ramp", 2D) = "white" {}
		_ColorMask ("Color Mask", 2D) = "black" {}
		_Tex1 ("Color 1", 2D) = "black" {}
		_Tex2 ("Color 2", 2D) = "black" {}
	}
	 
	SubShader
	{
		Cull off
		
		CGPROGRAM
		#pragma surface surf SimpleLambert
		#pragma target 2.0  		
		#include "UnityCG.cginc"

		sampler2D _Ramp;
		sampler2D _ColorMask;
		sampler2D _Tex1;
		sampler2D _Tex2;
		uniform float4 _Tex1_ST;
		uniform float4 _Tex2_ST;
		
		
		struct Input 
		{
			float2 uv_Ramp;
			float2 uv2_ColorMask;
		};
		
		

		half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
			
			half NdotL = dot (lightDir, s.Normal);
			
			half4 c;
			
			c.rgb = s.Albedo * _LightColor0.rgb * NdotL * atten;
			c.a = s.Alpha;
			return c;
			
			
		}
		
		float4 blendColors(float4 tex1, float a1, float4 tex2, float a2) {
		
			float depth = 0.1;
		    float ma = max(tex1.a + a1, tex2.a + a2) - depth;
		
		    float b1 = max(tex1.a + a1 - ma, 0);
		    float b2 = max(tex2.a + a2 - ma, 0);			
			
			return (tex1 * b1 + tex2 * b2) / (b1 + b2);
		}
				
		void surf(Input i, inout SurfaceOutput o)
		{ 
			float4 shade = tex2D(_Ramp, i.uv_Ramp);
			float4 mask = tex2D(_ColorMask, i.uv2_ColorMask);
			float4 color1 = tex2D(_Tex1,  TRANSFORM_TEX(i.uv2_ColorMask, _Tex1));
			float4 color2 = tex2D(_Tex2,  TRANSFORM_TEX(i.uv2_ColorMask, _Tex2));
			
			o.Albedo = blendColors(color1, mask.r, color2, mask.g) * shade.x;
			
		}
		ENDCG
		
	}
	
	FallBack "Diffuse"
}
