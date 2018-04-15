Shader "Hidden/UIMaterial"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}


		CGINCLUDE

#include "UnityCG.cginc" 

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		UNITY_FOG_COORDS(1)
			float4 vertex : SV_POSITION;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;

	v2f vertMult(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		UNITY_TRANSFER_FOG(o, o.vertex);
		return o;
	}

	fixed4 fragR(v2f i) : SV_Target
	{
		// sample the texture
		fixed4 col = tex2D(_MainTex, i.uv).r;
		col.a = 1;
	
		return col;
	}
	fixed4 fragRGB(v2f i) : SV_Target
	{
		// sample the texture
		fixed4 col = tex2D(_MainTex, i.uv);
		col.a = 1;
	
		return col;
	}
	ENDCG

	SubShader
	{
//		Tags{ "RenderType" = "Opaque" }
		LOD 100
		Pass
		{ //0
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragR
			ENDCG
		}
		Pass
		{//1
			ZTest Always Cull Off ZWrite Off 

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragRGB
			ENDCG
		}
	}
	Fallback off

}
