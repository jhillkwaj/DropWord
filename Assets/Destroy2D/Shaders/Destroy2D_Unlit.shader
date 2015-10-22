Shader "Destroy2D/Unlit" {
	Properties {
		_Ramp ("Light Ramp", 2D) = "white" {}
		_ColorMask ("Color Mask", 2D) = "black" {}
		_Tex1 ("Texture", 2D) = "black" {}
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
			uniform float4 _Tex1_ST;
			
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
				o.uv2 = TRANSFORM_TEX(v.texcoord1, _Tex1);
				 
				return o;
			}
			
						
			float4 frag(v2f i) : COLOR
			{ 
				float4 shade = tex2D(_Ramp, i.uv);
				float4 color1 = tex2D(_Tex1,  i.uv2);
				
				return color1 * shade.x * 1.3; 
				
			}
			ENDCG
		}
		
	}
	
}
