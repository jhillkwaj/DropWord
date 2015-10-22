Shader "Destroy2D/Bumped Specular" {
	Properties {
		_Ramp ("Light Ramp", 2D) = "white" {}
		_ColorMask ("Color Mask", 2D) = "black" {}
		_Tex1 ("Color 1", 2D) = "black" {}
		_Tex2 ("Color 2", 2D) = "black" {}
		_Tex3 ("Color 3", 2D) = "black" {}
		_Tex4 ("Normal Map", 2D) = "black" {}
		_NormalFactor("Normal factor", Float) = 1
		_SpecPower("Specular Power", Float) = 50
		_SpecAtt("Spec Attenuation", Float) = 2
	}
	 
	SubShader
	{
		Cull off
		
		CGPROGRAM
		#pragma surface surf SimpleLambert
		#pragma target 3.0  		
		#include "UnityCG.cginc"

		sampler2D _Ramp;
		sampler2D _ColorMask;
		sampler2D _Tex1;
		sampler2D _Tex2;
		sampler2D _Tex3;
		sampler2D _Tex4;
		sampler2D _NodeValues;
		uniform float4 _Tex1_ST;
		uniform float4 _Tex2_ST;
		uniform float4 _Tex3_ST;
		uniform float4 _Tex4_ST;
		float _NormalFactor;
		float _SpecPower;
		float _SpecAtt;
		
		
		struct Input 
		{
			float2 uv_Ramp;
			float2 uv2_ColorMask;
		};
		
		

		half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
						
			half3 h = normalize (lightDir + viewDir);

			half diff = max (0, dot (s.Normal, lightDir));
			
			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, _SpecPower);
			
			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten * _SpecAtt);
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
			float4 color3 = tex2D(_Tex3,  TRANSFORM_TEX(i.uv2_ColorMask, _Tex3));
			float2 color4Tex = TRANSFORM_TEX(i.uv2_ColorMask, _Tex4);
			float4 color4 = tex2D(_Tex4,  float2(color4Tex.x, 1-color4Tex.y));
			
			o.Albedo = blendColors(color1, mask.r, color2, mask.g) * shade.x; 
			o.Normal = UnpackNormal(color4);
			
		}
		ENDCG
		
	}
	
	FallBack "Diffuse"
}
