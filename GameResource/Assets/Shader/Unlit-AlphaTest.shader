// Unlit alpha-cutout shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/Unlit/Transparent Cutout" {
Properties {
	_MainTex("Base (RGB)", 2D) = "white" {}
	_AlphaTex("Alpha (A)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 100

	// Render both front and back facing polygons.
	Cull Off

		// first pass:
		//   render any pixels that are more than [_Cutoff] opaque
		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		float4 color : COLOR;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 vertex : POSITION;
		float4 color : COLOR;
		float2 texcoord : TEXCOORD0;
	};

	sampler2D _MainTex;
	sampler2D _AlphaTex;
	float4 _MainTex_ST;
	float4 _AlphaTex_ST;
	float _Cutoff;

	v2f vert(appdata_t v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.color = v.color;
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		return o;
	}

	float4 _Color;
	half4 frag(v2f i) : COLOR
	{
		half4 col = tex2D(_MainTex, i.texcoord);
		col.a = tex2D(_AlphaTex, i.texcoord).a;
		clip(col.a - _Cutoff);
		return col;
	}
		ENDCG
	}
}
}