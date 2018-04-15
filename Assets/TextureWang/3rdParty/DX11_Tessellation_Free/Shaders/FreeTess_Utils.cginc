#ifndef TESSUTILS_INCLUDED
#define TESSUTILS_INCLUDED

#include "UnityStandardUtils.cginc"

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
	#define _DETAIL 1
#endif

#if _DETAIL
half FTDetailMask(float2 uv)
{
	half m = tex2D (_MainTex, uv).a;
	m = _DetailAO > 0 ? 1 -  m * _DetailAO :  1 - (1 - m) * -_DetailAO ;
	return m;
}
#endif

half3 FTAlbedo(float4 texcoords, float2 maskcoords)
{
	float d = tex2D (_MainTex, texcoords.xy).a;
	half3 albedo = _Color.rgb * tex2D (_MainTex, texcoords.xy).rgb - ((1 - d) * _DiffuseAO);
#if _DETAIL
	half mask = FTDetailMask(maskcoords);
	half3 detailAlbedo = tex2D (_DetailAlbedoMap, texcoords.zw).rgb;
	#if _DETAIL_MULX2
		albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
	#elif _DETAIL_MUL
		albedo *= LerpWhiteTo (detailAlbedo, mask);
	#elif _DETAIL_ADD
		albedo += detailAlbedo * mask;
	#elif _DETAIL_LERP
		albedo = lerp (albedo, detailAlbedo, mask);
	#endif
#endif
	return albedo;
}

#ifdef _NORMALMAP
half3 FTNormalInTangentSpace(float4 texcoords)
{
	half3 normalTangent = UnpackScaleNormal(tex2D (_BumpMap, texcoords.xy), _BumpScale);
#if _DETAIL
	half mask = FTDetailMask(texcoords.xy);
	half3 detailNormalTangent = UnpackScaleNormal(tex2D (_DetailNormalMap, texcoords.zw), _DetailNormalMapScale);
	#if _DETAIL_LERP
		normalTangent = lerp(
			normalTangent,
			detailNormalTangent,
			mask);
	#else				
		normalTangent = lerp(
			normalTangent,
			BlendNormals(normalTangent, detailNormalTangent),
			mask);
	#endif
#endif
	return normalTangent;
}
#endif

#endif // TESSUTILS_INCLUDED