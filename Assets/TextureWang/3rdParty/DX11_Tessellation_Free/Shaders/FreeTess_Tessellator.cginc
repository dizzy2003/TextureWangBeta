#ifndef FT_TESSELLATOR_INCLUDED
#define FT_TESSELLATOR_INCLUDED

#include "UnityShaderVariables.cginc"

// ---- utility functions

float FTCalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess)
{
	float3 wpos = mul(unity_ObjectToWorld, vertex).xyz;
	float dist = distance (wpos, _WorldSpaceCameraPos);
	float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
	return f;
}

float4 FTCalcTriEdgeTessFactors (float3 triVertexFactors)
{
	float4 tess;
	tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
	tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
	tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
	tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
	return tess;
}

float FTCalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen)
{
	float dist = distance (0.5 * (wpos0+wpos1), _WorldSpaceCameraPos);
	float len = distance(wpos0, wpos1);
	float f = max(len * _ScreenParams.y / (edgeLen * dist), 1.0);
	return f;
}

float FTDistanceFromPlane (float3 pos, float4 plane)
{
	float d = dot (float4(pos,1.0f), plane);
	return d;
}

bool FTWorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps)
{    
	float4 planeTest;
	
	planeTest.x = (( FTDistanceFromPlane(wpos0, unity_CameraWorldClipPlanes[0]) > -cullEps) ? 1.0f : 0.0f ) +
				  (( FTDistanceFromPlane(wpos1, unity_CameraWorldClipPlanes[0]) > -cullEps) ? 1.0f : 0.0f ) +
				  (( FTDistanceFromPlane(wpos2, unity_CameraWorldClipPlanes[0]) > -cullEps) ? 1.0f : 0.0f );
	planeTest.y = (( FTDistanceFromPlane(wpos0, unity_CameraWorldClipPlanes[1]) > -cullEps) ? 1.0f : 0.0f ) +
				  (( FTDistanceFromPlane(wpos1, unity_CameraWorldClipPlanes[1]) > -cullEps) ? 1.0f : 0.0f ) +
				  (( FTDistanceFromPlane(wpos2, unity_CameraWorldClipPlanes[1]) > -cullEps) ? 1.0f : 0.0f );
	planeTest.z = (( FTDistanceFromPlane(wpos0, unity_CameraWorldClipPlanes[2]) > -cullEps) ? 1.0f : 0.0f ) +
				  (( FTDistanceFromPlane(wpos1, unity_CameraWorldClipPlanes[2]) > -cullEps) ? 1.0f : 0.0f ) +
				  (( FTDistanceFromPlane(wpos2, unity_CameraWorldClipPlanes[2]) > -cullEps) ? 1.0f : 0.0f );
	planeTest.w = (( FTDistanceFromPlane(wpos0, unity_CameraWorldClipPlanes[3]) > -cullEps) ? 1.0f : 0.0f ) +
				  (( FTDistanceFromPlane(wpos1, unity_CameraWorldClipPlanes[3]) > -cullEps) ? 1.0f : 0.0f ) +
				  (( FTDistanceFromPlane(wpos2, unity_CameraWorldClipPlanes[3]) > -cullEps) ? 1.0f : 0.0f );
		
	return !all (planeTest);
}

// ---- tessellation functions

float4 FTSphereProjectionTess (float4 v0, float4 v1, float4 v2, float maxDisplacement, float egdePixelSize)
{
#if 1 // TESSELLATOR_ADAPTIVE

	float3 pos0 = mul(unity_ObjectToWorld, v0).xyz;
	float3 pos1 = mul(unity_ObjectToWorld, v1).xyz;
	float3 pos2 = mul(unity_ObjectToWorld, v2).xyz;

#if 1 // TESSELLATOR_CLIP_PASS

	if (FTWorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement)) {
		return 0.0f;
	}

#endif

// This code is about 1% slower. Let's just leave it here.
//	float3 Edge0 = (pos0 - pos1);
//	float3 Edge1 = (pos1 - pos2);
//	float3 Edge2 = (pos2 - pos0);
//
//	float3 midPoint0 = 0.5 * (pos0 + pos1) - _WorldSpaceCameraPos;
//	float3 midPoint1 = 0.5 * (pos1 + pos2) - _WorldSpaceCameraPos;
//	float3 midPoint2 = 0.5 * (pos2 + pos0) - _WorldSpaceCameraPos;
//
//	// Use spherical projection instead of planar
//	float4 EdgeTessFactors = float4(
//		sqrt (dot(Edge1, Edge1) / dot(midPoint1, midPoint1)),
//		sqrt (dot(Edge2, Edge2) / dot(midPoint2, midPoint2)),
//		sqrt (dot(Edge0, Edge0) / dot(midPoint0, midPoint0)),
//		1.0 );

	float4 EdgeTessFactors = float4(
		distance(pos2, pos1) / distance((pos2 + pos1) * 0.5, _WorldSpaceCameraPos),
		distance(pos2, pos0) / distance((pos2 + pos0) * 0.5, _WorldSpaceCameraPos),
		distance(pos0, pos1) / distance((pos0 + pos1) * 0.5, _WorldSpaceCameraPos),
		1.0 );

	EdgeTessFactors.w = (EdgeTessFactors.x + EdgeTessFactors.y + EdgeTessFactors.z) * 0.333f;
	EdgeTessFactors = 0.5 * unity_CameraProjection[1][1] * _ScreenParams.y / egdePixelSize * EdgeTessFactors;

	//Tess factors can be clamped here
	//EdgeTessFactors = clamp(EdgeTessFactors, 1, 16);

	return EdgeTessFactors;	

#else
	return 1.0f;
#endif
}

float4 FTSphereProjectionTessNoClip (float4 v0, float4 v1, float4 v2, float egdePixelSize)
{

	float3 pos0 = mul(unity_ObjectToWorld, v0).xyz;
	float3 pos1 = mul(unity_ObjectToWorld, v1).xyz;
	float3 pos2 = mul(unity_ObjectToWorld, v2).xyz;


	float3 Edge0 = (pos0 - pos1);
	float3 Edge1 = (pos1 - pos2);
	float3 Edge2 = (pos2 - pos0);

	float3 midPoint0 = 0.5 * (pos0 + pos1) - _WorldSpaceCameraPos;
	float3 midPoint1 = 0.5 * (pos1 + pos2) - _WorldSpaceCameraPos;
	float3 midPoint2 = 0.5 * (pos2 + pos0) - _WorldSpaceCameraPos;

	float4 EdgeTessFactors = float4(
		sqrt (dot(Edge1, Edge1) / dot(midPoint1, midPoint1)),
		sqrt (dot(Edge2, Edge2) / dot(midPoint2, midPoint2)),
		sqrt (dot(Edge0, Edge0) / dot(midPoint0, midPoint0)),
		1.0 );

	EdgeTessFactors.w = (EdgeTessFactors.x + EdgeTessFactors.y + EdgeTessFactors.z) * 0.333f;
	EdgeTessFactors = 0.5 * unity_CameraProjection[1][1] * _ScreenParams.y / egdePixelSize * EdgeTessFactors;

	return EdgeTessFactors;	
}

float4 FTDistanceBasedTess (float4 v0, float4 v1, float4 v2, float minDist, float maxDist, float tess)
{
	float3 f;

	f.x = FTCalcDistanceTessFactor (v0, minDist, maxDist, tess);
	f.y = FTCalcDistanceTessFactor (v1, minDist, maxDist, tess);
	f.z = FTCalcDistanceTessFactor (v2, minDist, maxDist, tess);

	return FTCalcTriEdgeTessFactors (f); // * unity_CameraProjection[1][1];
}

float4 FTEdgeLengthBasedTess (float4 v0, float4 v1, float4 v2, float edgeLength)
{
	float3 pos0 = mul(unity_ObjectToWorld, v0).xyz;
	float3 pos1 = mul(unity_ObjectToWorld, v1).xyz;
	float3 pos2 = mul(unity_ObjectToWorld, v2).xyz;
	float4 tess;

	edgeLength *= 2;
	tess.x = FTCalcEdgeTessFactor (pos1, pos2, edgeLength);
	tess.y = FTCalcEdgeTessFactor (pos2, pos0, edgeLength);
	tess.z = FTCalcEdgeTessFactor (pos0, pos1, edgeLength);
	tess.w = (tess.x + tess.y + tess.z) / 3.0f;
	return tess;
}

float4 FTEdgeLengthBasedTessCull (float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement)
{
	float3 pos0 = mul(unity_ObjectToWorld, v0).xyz;
	float3 pos1 = mul(unity_ObjectToWorld, v1).xyz;
	float3 pos2 = mul(unity_ObjectToWorld, v2).xyz;
	float4 tess;

	if (FTWorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement)) {
		tess = 0.0f;
	} else {
	 	edgeLength *= 2;
		tess.x = FTCalcEdgeTessFactor (pos1, pos2, 8);
		tess.y = FTCalcEdgeTessFactor (pos2, pos0, 8);
		tess.z = FTCalcEdgeTessFactor (pos0, pos1, 8);
		tess.w = (tess.x + tess.y + tess.z) / 3.0f;
	}
	return tess;
}

#endif // FT_TESSELLATOR_INCLUDED