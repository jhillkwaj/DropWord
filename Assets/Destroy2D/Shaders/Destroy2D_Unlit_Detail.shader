Shader "Destroy2D/Unlit With Detail" {
	Properties {
		_Ramp ("Light Ramp", 2D) = "white" {}
		_ColorMask ("Color Mask", 2D) = "black" {}
		_Tex1 ("Texture", 2D) = "black" {}
		_Tex2 ("Detail", 2D) = "black" {}
	}
	
	SubShader
	{
		Cull off
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0  		
			#include "UnityCG.cginc"

			sampler2D _Ramp;
			sampler2D _ColorMask;
			sampler2D _Tex1;
			sampler2D _Tex2;
			uniform float4 _Tex1_ST;
			uniform float4 _Tex2_ST;
			
			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2: TEXCOORD1;
			};
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				o.uv2 = v.texcoord1;
				 
				return o;
			}
			
			float4 blendColors(float4 tex1, float a1, float4 tex2, float a2) {
		
				float depth = 0.1;
			    float ma = max(tex1.a + a1, tex2.a + a2) - depth;
			
			    float b1 = max(tex1.a + a1 - ma, 0);
			    float b2 = max(tex2.a + a2 - ma, 0);
				
				//return ((tex1.a + b1) > (tex2.a + b2)) ? tex1.rgb : tex2.rgb;
				return (tex1 * b1 + tex2 * b2) / (b1 + b2);
			}
			
						
			float4 frag(v2f i) : COLOR
			{ 
				float4 shade = tex2D(_Ramp, i.uv);
				float4 mask = tex2D(_ColorMask, i.uv2);
				float4 color1 = tex2D(_Tex1,  TRANSFORM_TEX(i.uv2, _Tex1));
				float4 color2 = tex2D(_Tex2,  TRANSFORM_TEX(i.uv2, _Tex2));
			
				return blendColors(color1, mask.r, color2, mask.g) * shade.x * 1.3; 
				
			}
			ENDCG
		}
		
	}
	
	FallBack "Diffuse"
}
