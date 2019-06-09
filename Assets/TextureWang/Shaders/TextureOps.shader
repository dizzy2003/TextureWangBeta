// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/TextureOps" {
	Properties 
	{ 
	 _MainTex ("", any) = "" {} 
	 _GradientTex("", any) = "" {}
	 _Multiply ("_Multiply", Vector) = (1,1,1,1)
     _TexSizeRecip("_TexSizeRecip", Vector) = (.1,.1,.1,.1)
	}
	CGINCLUDE

#pragma skip_variants POINT POINT_COOKIE INSTANCING_ON LIGHTMAP_ON DIRECTIONAL LIGHTPROBE_SH SHADOWS_SCREEN LIGHTMAP_SHADOW_MIXING
#pragma multi_compile_fwdbase novertexlight noshadowmask nodynlightmap nodirlightmap nolightmap

	#include "UnityCG.cginc"
	struct v2f {
		float4 pos : SV_POSITION; 
		half2 uv : TEXCOORD0;
	};
	sampler2D _MainTex;
	SamplerState my_point_sampler;
	SamplerState my_linear_clamp_sampler;

	sampler2D _MainTexClamp = sampler_state {
		Texture = (_MainTex);
		MinFilter = Linear;
		MagFilter = Linear;
		AddressU = Clamp;
		AddressV = Clamp;
	};


	sampler2D _GradientTex;
	sampler2D _GradientTex2;
	sampler2D _GradientTex3;
	float4 _Multiply;
	float4 _Multiply2;
	float4 _Multiply3;
	float4 _Multiply4;
	float4 _TexSizeRecip;
	int   _MainIsGrey; 
	int   _TextureBIsGrey;
	int   _TextureOutIsGrey;
	int _Saturate;
	int _Abs;
	int _InvertInput; 
	int _ClampInputUV;
	int _InvertOutput;
	float4 _ScaleOutput;
	float4 _ScaleOutput2;
	int4 m_GeneralInts;
	
/*
	float rand(float2 n)
	{
		return fract(sin(dot(n.xy, float2(12.9898, 78.233)))* 43758.5453);
	}
	*/
	uint wangHash(int _Seed)
	{
		_Seed = (_Seed ^ 61) ^ (_Seed >> 16);
		_Seed *= 9;
		_Seed = _Seed ^ (_Seed >> 4);
		_Seed *= 0x27d4eb2d;
		_Seed = _Seed ^ (_Seed >> 15);
		return _Seed;
	}

	float4 GetTextureMain4(sampler2D _tex, float2 uv)
	{
		if (_ClampInputUV > 0.1f)
		{
			if (uv.x<0 || uv.y<0 || uv.x>=1.0f || uv.y>=1.0f)
			{
				return float4(0,0,0,1);
			}
		}
//		if (_ClampInputUV > 0.1f)
//			uv = saturate(uv);
		/*

		if (uv.x > .95)// 1.0f - _TexSizeRecip.x)
			uv.x = .95;// 1.0f - _TexSizeRecip.x;
		if (uv.y > .95)//1.0f - _TexSizeRecip.y)
			uv.y = .95;//1.0f - _TexSizeRecip.y;
*/
		

		if (_MainIsGrey>0 )
		{
//			float r = _tex.Sample(my_point_sampler,uv).r;// tex2D(_tex, uv).r;
			float r;
//			if (_ClampInputUV > 0.1f)
//				r = tex2D(_MainTexClamp, uv).r;// tex2D(_tex, uv).r;
//			else
				r = tex2D(_tex, uv).r;
			if (_InvertInput)
				r = 1.0f - r;

			return float4(r, r, r, 1.0f);
		}
		else
		{
			float4 ret = tex2D(_tex, uv);

			if (_InvertInput)
				ret = 1.0f - ret;

			return ret;// tex2D(_tex, uv);
		}
	}
	float4 GetTextureMain4lod(sampler2D _tex, float4 uv)
	{
		if (_ClampInputUV > 0.1f)
		{
			if (uv.x<0 || uv.y<0 || uv.x >= 1.0f || uv.y >= 1.0f)
			{
				return float4(0, 0, 0, 1);
			}
		}

		if (_MainIsGrey>0)
		{
			float r;
//			if (_ClampInputUV > 0.1f)
//				r = tex2D(_MainTexClamp, uv).r;// tex2D(_tex, uv).r;
//			else
				r = tex2D(_tex, uv).r;

			if (_InvertInput)
				r = 1.0f - r;

			return float4(r, r, r, 1.0f);
		}
		else
		{
			float4 ret = tex2Dlod(_tex, uv); 

			if (_InvertInput)
				ret = 1.0f - ret;

			return tex2Dlod(_tex, uv);
		}
	}
	float3 GetNorm(float2 uv, sampler2D _tex,float _offsetRead)
	{
		float heightMapSizeX = _TexSizeRecip.x*_offsetRead;
		float heightMapSizeY = _TexSizeRecip.y*_offsetRead;

		float h0 = GetTextureMain4(_tex, uv).x;
		//		float n = tex2D(_MainTex,float2(uv.x,uv.y+1.0*heightMapSizeY)).x;
		float hdown = GetTextureMain4(_tex, float2(uv.x, uv.y - 1.0*heightMapSizeY)).x;
		float hright = GetTextureMain4(_tex, float2(uv.x + 1.0*heightMapSizeX, uv.y)).x;
		//		float w = tex2D(_MainTex,float2(uv.x-1.0*heightMapSizeX,uv.y)).x;                

		float3 right = float3(1, (hright - h0)* _TexSizeRecip.z, 0);
		float3 down = float3(0, (hdown - h0) * _TexSizeRecip.z, 1.0f);

		float3 norm = cross(normalize(down), normalize(right));
		norm = normalize(norm);
		norm.z *= -1.0f;

		
		return norm.xzy;
	}

	float4 GammaCorrection(float4 color, float gamma)
	{
		
		float4 v = 1.0 / gamma;
		return pow(color, v);
	}

	float4 LevelsControlInputRange(float4 color, float minInput, float maxInput)
	{
		
		color -= minInput;
		float range = maxInput - minInput;
		color /= range;

		return color;
	}

	float4 LevelsControlInput(float4 color, float minInput, float gamma, float maxInput)
	{
		return GammaCorrection(LevelsControlInputRange(color, minInput, maxInput), gamma);
	}

	float4 LevelsControlOutputRange(float4 color, float minOutput, float maxOutput)
	{
		
		float range = maxOutput - minOutput;
		color *= range;
		color += minOutput;
		return color;
	
	}
	float4 LevelsControl(float4 color, float minInput, float  gamma, float  maxInput, float  minOutput, float  maxOutput)
	{
		return LevelsControlOutputRange(LevelsControlInput(color, minInput, gamma, maxInput), minOutput, maxOutput);
	}
	float4 ProcessOutput(float4 _col)
	{
		if (_InvertOutput)
			_col.rgb = 1 - _col.rgb;
/*
		float range = _ScaleOutput.w - _ScaleOutput.z;
		range/= _ScaleOutput.y - _ScaleOutput.x;

		_col.rgb -= _ScaleOutput.x;
		_col.rgb *= range;
		_col.rgb += _ScaleOutput.z;
*/
		if (_Abs)
		_col = abs(_col);

		_col = LevelsControl(_col, _ScaleOutput.x, _ScaleOutput2.x, _ScaleOutput.y, _ScaleOutput.z, _ScaleOutput.w);
//		_col.rgb*=_ScaleOutput;

		if (_Saturate)
			_col= saturate(_col);
		if (_TextureOutIsGrey)
			_col = (_col.r + _col.g + _col.b) / 3.0;

		if (_ScaleOutput2.y > 0.0f)
			_col = step(_ScaleOutput2.y, _col);

		return _col;
	}
	float4 GetTextureB4(sampler2D _tex, float2 uv) 
	{
		if (_TextureBIsGrey>0)
		{
			float r = tex2D(_tex, uv).r;
			return float4(r, r, r, 1.0f);
		}
		else
			return tex2D(_tex, uv);
	}

	v2f vertMult(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex); 
		o.uv = v.texcoord ; 
		return o;
	}
	float4 fragMult(v2f i) : SV_Target
	{
		
		float4 color = GetTextureMain4(_MainTex, i.uv);
		color *= _Multiply;
		//color.b = 100.0f;
		return ProcessOutput(color);
	}
		float4 fragDot(v2f i) : SV_Target
	{

		float4 color = GetTextureMain4(_MainTex, i.uv);
		float4 res=dot(color.xyz, _Multiply.xyz);
		res.a = 1;
		return ProcessOutput(res);
	}

	float4 fragAdd(v2f i) : SV_Target
	{
		float4 color = GetTextureMain4(_MainTex, i.uv);
		color += _Multiply;
		//color.b = 100.0f;
		return ProcessOutput(color);

	} 
	float4 fragMapCylinder(v2f i) : SV_Target
	{
		float v = (i.uv.y-0.5)*2 ;
		//v = sqrt(_Multiply.x*_Multiply.x - v*v);
		v = asin(v*_Multiply.x) + _Multiply.y;
		v*=_Multiply.z;
		float2 uv = float2(i.uv.x,v+0.5);
		float4 color = GetTextureMain4(_MainTex, uv);
		float v2 = -(i.uv.y - 0.5) * 2;
		v2 = asin(v2*_Multiply.x) + _Multiply.y;
		v2 *= _Multiply.z;
		float2 uv2 = float2(i.uv.x, v2 + 0.5);
		color += GetTextureMain4(_MainTex, uv2)*0.3;

		//color.b = 100.0f;
		return ProcessOutput(color);

	}
		float4 fragSymetry(v2f i) : SV_Target
	{
		float PI = 3.14159265;
	float angle = _Multiply.x*PI/180;
	float4 plane = float4(cos(angle), 0, -sin(angle), 0);
	float4 pos = float4(i.uv.x - 0.5f, 0, i.uv.y - 0.5f, 1);

	float d = dot(pos, plane);
	if (d < 0)
	{
		//reflect
		float sum = dot(plane.xyz, plane.xyz);
		float ux = pos.x - 2 * plane.x*d / sum;
		float uz = pos.z - 2 * plane.z*d / sum;
		i.uv = float2(ux + 0.5, uz + 0.5);
	}
	//i.uv.y = 1 - i.uv;
	float4 color = GetTextureMain4(_MainTex, i.uv);

	//color.b = 100.0f;
	return ProcessOutput(color);

	}

	float4 fragMapSphere(v2f i) : SV_Target
	{
		float PI = 3.14159265;

		float3 delta;
		delta.x = i.uv.x - 0.5f;
		delta.y = i.uv.y - 0.5f;
		float radius = _Multiply.x;

		float val = radius*radius - delta.x*delta.x - delta.y * delta.y;
		if (val < 0)
			return float4(0,0,0,1);

		delta.z = sqrt(val);

		float u = 0.5 + _Multiply.y*atan2(delta.z, delta.x)/(2*PI);
		float v = 0.5 - _Multiply.y*asin(delta.y) /  PI;


/*
		float lat = i.uv.y*PI - PI*.5;
		float lon = i.uv.x*PI - PI;
		float u = cos(lat)*cos(lon);
		float v = sin(lat);
*/
		float4 color = GetTextureMain4(_MainTex, float2(u,v));

		//color.b = 100.0f;
		return ProcessOutput(color);

	}
		float4 fragSetCol(v2f i) : SV_Target
	{
		float4 color = _Multiply;
		color.a = 1.0f;
		return ProcessOutput(color);

	}
		float4 fragAnimCurveAxis(v2f i) : SV_Target
	{
		float4 color = (max(GetTextureMain4(_MainTex, float2(i.uv.x*_Multiply.x,0)) , GetTextureB4(_GradientTex, float2(i.uv.y*_Multiply.y,0))));
		color.a = 1;

		return ProcessOutput(color);

	}
		
	float4 fragAnimCurveDraw(v2f i) : SV_Target
	{
		float4 color = GetTextureMain4(_MainTex, float2(i.uv.x*_Multiply.x,0)) ;
		float v = i.uv.y;
		v -= _Multiply.z;
		if(_Multiply.w>0.5)
			v = abs(v);
		if (v < color.r*_Multiply.y)
			color.rgb = 1;// -v;
		else
			color.rgb = 0;
		color.a = 1;

		return ProcessOutput(color);

	}

	float4 fragDraw(v2f i) : SV_Target
	{
		float4 color = (max(GetTextureMain4(_MainTex, float2(i.uv.x*_Multiply.x,0)) , GetTextureB4(_GradientTex, float2(i.uv.y*_Multiply.y,0))));
		color.a = 1;

		return ProcessOutput(color);

	}

	float4 fragAdd2(v2f i) : SV_Target
	{
		float4 color = (GetTextureMain4(_MainTex, i.uv) +GetTextureB4(_GradientTex, i.uv));
		color.a = 1;

		return ProcessOutput(color);
	}

		float4 fragSub2(v2f i) : SV_Target
	{
		float4 color = (GetTextureMain4(_MainTex, i.uv) - GetTextureB4(_GradientTex, i.uv));
		color.a = 1;
		return ProcessOutput(color);

	}
		float4 fragBlend2(v2f i) : SV_Target
	{
		float4 color = (lerp(GetTextureMain4(_MainTex, i.uv) , GetTextureB4(_GradientTex, i.uv),_TexSizeRecip.z));
		return ProcessOutput(color);

	}
		float4 fragMult2(v2f i) : SV_Target
	{
		float4 color = (GetTextureMain4(_MainTex, i.uv) * GetTextureB4(_GradientTex, i.uv));
		color.a = 1;

		return ProcessOutput(color);

	}
		float4 fragDivide2(v2f i) : SV_Target
	{
		float4 color = (GetTextureMain4(_MainTex, i.uv) / GetTextureB4(_GradientTex, i.uv));
		return ProcessOutput(color);

	}
		float4 fragPow2(v2f i) : SV_Target
	{
		float4 color = (pow(GetTextureMain4(_MainTex, i.uv) , GetTextureB4(_GradientTex, i.uv)));
		color.a = 1;

		return ProcessOutput(color);

	}
		float4 fragMin(v2f i) : SV_Target
	{
		float4 color = min(GetTextureMain4(_MainTex, i.uv) , GetTextureB4(_GradientTex, i.uv));
		color.a = 1;

		return ProcessOutput(color);

	}
		float4 fragMax(v2f i) : SV_Target
	{
		float4 color = (max(GetTextureMain4(_MainTex, i.uv) , GetTextureB4(_GradientTex, i.uv)));
		color.a = 1;

		return ProcessOutput(color);

	}
		float4 fragMin1(v2f i) : SV_Target
	{
		float4 ret = min(GetTextureMain4(_MainTex, i.uv) , _Multiply.x);
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}
		float4 fragSrcStepify(v2f i) : SV_Target
	{
		float4 ret = fmod(GetTextureMain4(_MainTex, i.uv) , _Multiply.x);
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}
		float4 fragMax1(v2f i) : SV_Target
	{ 
		float4 ret = max(GetTextureMain4(_MainTex, i.uv) , _Multiply.x);
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}
		float4 fragStep1(v2f i) : SV_Target
	{
		float4 src = GetTextureMain4(_MainTex, i.uv);
		if (_TextureOutIsGrey)
			src = (src.r + src.g + src.b) / 3.0;

		float4 ret = step(_Multiply.x, src);
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}
		float4 fragInvert(v2f i) : SV_Target
	{
		float4 ret = 1.0f-(GetTextureMain4(_MainTex, i.uv));
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}

	float4 fragSobel(v2f i) : SV_Target
	{
		float2 delta = _TexSizeRecip.xy*_Multiply.x;

		float4 hr = float4(0, 0, 0, 0);
		float4 vt = float4(0, 0, 0, 0); 

		hr += tex2D(_MainTex, (i.uv + float2(-1.0, -1.0) * delta)) *  1.0;
//		hr += tex2D(_MainTex, (i.uv + float2(0.0, -1.0) * delta)) *  0.0;
		hr += tex2D(_MainTex, (i.uv + float2(1.0, -1.0) * delta)) * -1.0;
		hr += tex2D(_MainTex, (i.uv + float2(-1.0,  0.0) * delta)) *  2.0;
//		hr += tex2D(_MainTex, (i.uv + float2(0.0,  0.0) * delta)) *  0.0;
		hr += tex2D(_MainTex, (i.uv + float2(1.0,  0.0) * delta)) * -2.0;
		hr += tex2D(_MainTex, (i.uv + float2(-1.0,  1.0) * delta)) *  1.0;
//		hr += tex2D(_MainTex, (i.uv + float2(0.0,  1.0) * delta)) *  0.0;
		hr += tex2D(_MainTex, (i.uv + float2(1.0,  1.0) * delta)) * -1.0;

		vt += tex2D(_MainTex, (i.uv + float2(-1.0, -1.0) * delta)) *  1.0;
		vt += tex2D(_MainTex, (i.uv + float2(0.0, -1.0) * delta)) *  2.0;
		vt += tex2D(_MainTex, (i.uv + float2(1.0, -1.0) * delta)) *  1.0;
//		vt += tex2D(_MainTex, (i.uv + float2(-1.0,  0.0) * delta)) *  0.0;
//		vt += tex2D(_MainTex, (i.uv + float2(0.0,  0.0) * delta)) *  0.0;
		//vt += tex2D(_MainTex, (i.uv + float2(1.0,  0.0) * delta)) *  0.0;
		vt += tex2D(_MainTex, (i.uv + float2(-1.0,  1.0) * delta)) * -1.0;
		vt += tex2D(_MainTex, (i.uv + float2(0.0,  1.0) * delta)) * -2.0;
		vt += tex2D(_MainTex, (i.uv + float2(1.0,  1.0) * delta)) * -1.0;

		float4 color= sqrt(hr * hr + vt * vt);
		return ProcessOutput(color);
	}
		float4 fragSharpen(v2f i) : SV_Target
	{
		float2 delta = _TexSizeRecip.xy*_Multiply.x;

		float4 vt = float4(0, 0, 0, 0);
		vt += tex2D(_MainTex, (i.uv + float2(0.0, -1.0) * delta)) *  -1.0;
		vt += tex2D(_MainTex, (i.uv + float2(0.0,  0.0) * delta)) * 5.0;
		vt += tex2D(_MainTex, (i.uv + float2(0.0,  1.0) * delta)) * -1.0;

		vt += tex2D(_MainTex, (i.uv + float2(-1.0, 0.0) * delta)) *  -1.0;
		vt += tex2D(_MainTex, (i.uv + float2(1.0,  0.0) * delta)) *  -1.0;

		
		return ProcessOutput(vt);
	}
		float4 fragEmboss(v2f i) : SV_Target
	{
		float2 delta = _TexSizeRecip.xy*_Multiply.x;

		float4 vt = float4(0, 0, 0, 0);
		vt += tex2D(_MainTex, (i.uv + float2(-1.0, -1.0) * delta)) *  -2.0;
		vt += tex2D(_MainTex, (i.uv + float2(0.0, -1.0) * delta)) *  -1.0;
		vt += tex2D(_MainTex, (i.uv + float2(1.0,  -1.0) * delta)) *  0.0;
		vt += tex2D(_MainTex, (i.uv + float2(-1.0, 0.0) * delta)) * -1.0;
		vt += tex2D(_MainTex, (i.uv + float2(0.0,  0.0) * delta)) * 1.0;
		vt += tex2D(_MainTex, (i.uv + float2(1.0,  0.0) * delta)) * 1.0;

		vt += tex2D(_MainTex, (i.uv + float2(-1.0, 1.0) * delta)) *  0.0;
		vt += tex2D(_MainTex, (i.uv + float2(0.0, 1.0) * delta)) *  1.0;
		vt += tex2D(_MainTex, (i.uv + float2(1.0,  1.0) * delta)) *  2.0;


		return ProcessOutput(vt);
	}
	float4 fragPosterize(v2f i) : SV_Target
	{
		float4 src = GetTextureMain4(_MainTex, i.uv);
		src = src*_Multiply.x;
		src = (int)src;
		src = src / _Multiply.x;


		return ProcessOutput(src);
	}

	float3 getSmoothNorm(float2 uv,float size,float scale)
	{
		float4 sum = 0;
		float sumG = 0.0f; 
		float dx;
		float dy;
		float maxLen = sqrt(size*scale*size*scale + size*size*scale*scale);
		[loop]
		for (dx = -size; dx <= size; dx += 1.0f)
		{
			[loop]
			for (dy = -size; dy <= size; dy += 1.0f)
			{
				float2 offset = float2(dx, dy);
				float2 offsetTex = offset*_TexSizeRecip.xy*scale;//=float2(dx/1024.0,dy/1024.0);
				float3 col = GetNorm(float2(uv.x + offsetTex.x, uv.y + offsetTex.y),_GradientTex,1 );

				float dist = length(offset);//sqrt(dot(offset,offset));
				float gauss = (1 + cos(3.14159*dist / maxLen)) / 2.0*3.14159;
				sumG += gauss;
				sum.rgb += col.rgb*gauss;
			}
		}
		sum.rgb /= sumG;
		return sum;
		
	}

	float4 fragSmooth(v2f i) : SV_Target
	{

		float col = GetTextureMain4(_MainTex, i.uv).r;
		float size = _Multiply.x;
		if (size < 1)
			size = 1;

		float dx;
		float dy;


		float recipDist = 1.0 / size;//_Dist;

		float4 colFinalr = float4(0,0,0,0);
		float4 colStart = GetTextureMain4(_MainTex, i.uv);
		//				float scaleTex=_InvTexSize;

		float maxLen = sqrt(size*size + size*size);
		float4 sum = 0;
		float sumG = 0.0f;
		[loop]
		for (dx = -size; dx<=size; dx += 1.0f)
		{
			[loop]
			for (dy = -size; dy<=size; dy += 1.0f)
			{
				float2 offset = float2(dx,dy);
				float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
				float4 col = GetTextureMain4lod(_MainTex, float4(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y,0,0));
				float dist = length(offset);//sqrt(dot(offset,offset));
				float gauss = (1 + cos(3.14159*dist / maxLen)) / 2.0*3.14159;
				sumG += gauss;
				sum.rgb += col.rgb*gauss;  
			}
		}
		sum.rgb /= sumG;
		sum.a = 1;
		return ProcessOutput(sum);

	}
	float4 fragSmoothDir(v2f i) : SV_Target
	{
		float size = floor(_Multiply.x);//50.0f;
		
		float dx;
		float dy;

		float4 colStart = 0;
//		float2 offsetDir = -getSmoothNorm(i.uv, 3.0f, 0.5f);// _Multiply.y);// GetNorm(i.uv, _GradientTex, 10.0).xy;
		float2 offsetDir = - GetNorm(i.uv, _GradientTex, 1.0).xy;
//		offsetDir = float2(1, 0);
		float sumG=0;
		float3 sum=0;
		if(size<1)
			colStart = GetTextureMain4lod(_MainTex, float4(i.uv.x, i.uv.y, 0, 0));
		else
		{
//			offsetDir = normalize(offsetDir);
			float step = size / _Multiply.z;
			int count = 0;
			
			for (dx = 1; dx < size; dx += step)
			//dx = size;
			{

				float2 offset = offsetDir*dx;// float2(dx, dy);
				float2 offsetTex = offset*_TexSizeRecip.xy*_Multiply.y;//=float2(dx/1024.0,dy/1024.0);
				float4 col = GetTextureMain4lod(_MainTex, float4(i.uv.x + offsetTex.x, i.uv.y + offsetTex.y, 0, 0));
				float gauss = (1 + cos(3.14159*(size-dx) / size)) / 2.0*3.14159;
				sumG += gauss;
				sum.rgb += col.rgb*gauss;

				colStart += col;
				//if (length(col.rgb)>0)
				count += 1;

			}
			//if (size > 0)
//				colStart *= 1 / (size*2);
			sum /= sumG;
			colStart.rgb = sum.rgb;

		}
		colStart.a = 1;
		
		return ProcessOutput(colStart);

	}
	float4 fragSmoothedMask(v2f i) : SV_Target
	{

		float col = GetTextureMain4(_MainTex, i.uv).r;
		float size = _Multiply.x;
		float dx;
		float dy;


		float recipDist = 1.0 / size;//_Dist;

		float4 colFinalr = float4(0,0,0,0);
		float4 colStart = GetTextureMain4(_MainTex, i.uv);
		float4 mask = GetTextureB4(_GradientTex, i.uv);
		size *= mask;
		if (size < 1)
			size = 1;



		float maxLen = sqrt(size*size + size*size);
		float4 sum = 0;
		float sumG = 0.0f;
		[loop]
		for (dx = -size; dx <= size; dx += 1.0f)
		{
			[loop]
			for (dy = -size; dy <= size; dy += 1.0f)
			{
				float2 offset = float2(dx,dy);
				float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
				float4 col = GetTextureMain4lod(_MainTex, float4(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y,0,0));
				float dist = length(offset);//sqrt(dot(offset,offset));
				float gauss = (1 + cos(3.14159*dist / maxLen)) / 2.0*3.14159;
				sumG += gauss;
				sum.rgb += col.rgb*gauss;
			}
		}
		sum.rgb  /= sumG;
		sum.a = 1;
		return ProcessOutput(sum);

	}
	float4 fragAO(v2f i) : SV_Target
	{

		
		float size = _Multiply.x;
		float dx;
		float dy;


		float recipDist = 1.0 / size;//_Dist;

		//				float scaleTex=_InvTexSize;

		float maxLen = sqrt(size*size + size*size);
		float sum = 0;
		float AO = 0.0f;
		float4 src= GetTextureMain4lod(_MainTex, float4(i.uv.x, i.uv.y, 0, 0)).r;
		float depth = src.r;
		[loop]
		for (dx = -size; dx <= size; dx += 1.0f)
		{
			[loop]
			for (dy = -size; dy <= size; dy += 1.0f)
			{
				float2 offset = float2(dx,dy);
				float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
				float depthSample = GetTextureMain4(_MainTex, float2(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y)).r;
				
				if (depthSample > depth) 
				{
					AO += 1.0;
				}

				sum += 1.0f;
			}
		}
		
		float4 col;
		col.rgb =  1-AO / sum;
		col.a = 1;
		return ProcessOutput(col);

	}
		float4 fragSrcBlend(v2f i) : SV_Target
	{
		float4 src = GetTextureMain4(_MainTex, i.uv);
		float4 src2 = GetTextureB4(_GradientTex, i.uv);

		float alpha = saturate(1- _Multiply.x*length(src.rgb));
		float4 ret = src + src2*( alpha);
		

		ret.w = 1.0f;
		return ProcessOutput(ret);

	}

		float4 fragPower(v2f i) : SV_Target
	{
		float4 ret= pow(GetTextureMain4(_MainTex, i.uv) , _Multiply);
		ret.w = 1.0f;
		return ProcessOutput(ret);
	}

	float4 fragLevel1(v2f i) : SV_Target
	{
		float4 a = GetTextureMain4(_MainTex, i.uv);
		a *= _Multiply.x;
		a += _Multiply.y;
		a.w = 1.0f;
//		a.r = 1.0f;
		return ProcessOutput(a);
	}
	float4 fragTransform(v2f i) : SV_Target
	{
		float2 uv;
		uv.x = (i.uv.x-0.5)*_Multiply.x*_Multiply2.z - (i.uv.y - 0.5)*_Multiply.y*_Multiply2.z+0.5+ _Multiply2.x;
		uv.y = (i.uv.x - 0.5)*_Multiply.y*_Multiply2.w + (i.uv.y - 0.5)*_Multiply.x*_Multiply2.w +0.5 + _Multiply2.y;

		float4 a = GetTextureMain4(_MainTex, uv);
		return ProcessOutput(a);
	}
		float4 fragSkew(v2f i) : SV_Target
	{
		float2 uv;
		uv.x = i.uv.x+(i.uv.y - 0.5)*_Multiply2.z + _Multiply2.x;
		uv.y = i.uv.y + (i.uv.x - 0.5)*_Multiply2.w + _Multiply2.y;
		

		float4 a = GetTextureMain4(_MainTex, uv);
		return ProcessOutput(a);
	}
	float4 fragSquish(v2f i) : SV_Target
	{
		float2 uv;
		float squishy = lerp(_Multiply2.z, _Multiply2.w, (i.uv.y + _Multiply2.y));
		float squishx = lerp(_Multiply.x, _Multiply.y, (i.uv.x + _Multiply2.x));
		uv.x = 0.5f+(i.uv.x-0.5f) / squishy ;
		uv.y = 0.5f + (i.uv.y - 0.5f) /squishx;// *(1 + (i.uv.x - 0.5)*_Multiply2.w) ;


		float4 a = GetTextureMain4(_MainTex, uv);
		return ProcessOutput(a);
	}
	float4 fragSplatter(v2f i) : SV_Target
	{
		float2 uv;
		float4 sum;
		int seed = _Multiply.x;
		float scale;
		float angle;
		float dx;
		float dy;
		float deltaX;
		float deltaY;
		int count = _Multiply.z;
		[loop]
		for (int index = 0; index < count; index++)
		{
			seed = wangHash(seed);
			angle = (seed*1.0 / 4294967295.0f)*30.14159;
			seed = wangHash(seed);
			float srange = _Multiply2.w - _Multiply.y;

			scale= saturate((seed*1.0 / 4294967295.0f)+0.5f)*srange+ _Multiply.y;

			seed = wangHash(seed);
			deltaY = ((seed*1.0 / 4294967295.0f) - 0.5f)*10.0f;
			seed = wangHash(seed);
			deltaX = ((seed*1.0 / 4294967295.0f) - 0.5f)*10.0f;
			seed = wangHash(seed);
			float brange = _Multiply2.y - _Multiply2.x;
			float bright = ((seed*1.0 / 4294967295.0f) + 0.5f)*brange + _Multiply2.x;
			dx = cos(angle)*scale;
			dy = sin(angle)*scale;


			uv.x = (i.uv.x - 0.5)*dx - (i.uv.y - 0.5)*dy + 0.5 + deltaX;
			uv.y = (i.uv.x - 0.5)*dy + (i.uv.y - 0.5)*dx + 0.5 + deltaY;
			if(_Multiply2.z==0)
				sum = max(sum, GetTextureMain4(_MainTex, uv)*bright);
			else
				sum += GetTextureMain4(_MainTex, uv);
		}
		return ProcessOutput(sum);
	}


	float4 GridCellCol(float2 localUV, float2 gridCell,float gridSize,float texProb)
	{

		int srcSeed = _Multiply.x;
		float seed = wangHash(8751 + (floor(1 + gridCell.x) * 11 + floor(1 + gridCell.y)*gridSize)*srcSeed);

		float rand = (((float)seed) / 1000);
		rand -= floor(rand);
		float4 sum;

		if (rand >= _Multiply2.w+(1-texProb))
		{
			float randomAngleScale = _Multiply3.x;
			float randomZoomScaleRange = _Multiply3.z-_Multiply3.y;
			float randomZoomScaleMin = _Multiply3.y;
			float randomBrightnessMax = _Multiply3.w;

			seed = wangHash(seed);
			float scale = randomZoomScaleMin + ((seed / 4294967295.0f) )*randomZoomScaleRange;
			seed = wangHash(seed);
			float deltaY = ((seed*2.0 / 4294967295.0f) - 0.5f)*_Multiply2.x;
			seed = wangHash(seed);
			float deltaX = ((seed*2.0 / 4294967295.0f) - 0.5f)*_Multiply2.y;
			seed = wangHash(seed);
			float brightness = 1 - ((seed*1.0 / 4294967295.0f))*randomBrightnessMax;
			seed = wangHash(seed);
			float angle = ((seed*2.0 / 4294967295.0f) - 0.5f)*randomAngleScale / 90 * 3.14159;
			
			float dx = cos(angle)*scale;
			float dy = sin(angle)*scale;

			float uvx = (localUV.x - 0.5)*dx - (localUV.y - 0.5)*dy + 0.5 +deltaX;
			float uvy = (localUV.x - 0.5)*dy + (localUV.y - 0.5)*dx + 0.5 +deltaY;

			sum = GetTextureMain4(_MainTex, float2(uvx, uvy))*brightness;
			sum.a = 1;
			

		}
		else
		{
			sum = 0;
			sum.a = 1;
		}
		return sum;

	}


		float4 fragSplatterGrid(v2f i) : SV_Target
		{

			float4 sum=0;

			float gridSize = floor(_Multiply.z);
			float extra = 3;
			[loop]
			for (float dx = -extra; dx <= extra; dx += 1)
			{
				[loop]
				for (float dy = -extra; dy <= extra; dy += 1)
				//float dy = 0;
				{
					float2 localUV = i.uv*gridSize;
					float2 floorLocalUV = floor(localUV);// i.uv*gridSize;


					float2 gridUV = floor(localUV) +float2(dx, dy);
					gridUV = fmod(gridUV+ gridSize, gridSize);
					localUV -= float2(dx, dy);
					localUV -= floorLocalUV;
					{
						//float4 a = GridCellCol(localUV - float2(dx, dy), gridUV + float2(dx, dy), gridSize);
						float4 a = GridCellCol(localUV , gridUV , gridSize, 1.0f);
						sum = max(a, sum);
					}
				}
			}
			sum.a = 1;
			return ProcessOutput(sum);
		}

		float4 fragSplatterGridProb(v2f i) : SV_Target
		{

			float4 sum = 0;

			float gridSize = floor(_Multiply.z);
			float extra = 3;
			[loop]
			for (float dx = -extra; dx <= extra; dx += 1)
			{
				[loop]
				for (float dy = -extra; dy <= extra; dy += 1)
					//float dy = 0;
				{
					float2 localUV = i.uv*gridSize;
					float2 floorLocalUV = floor(localUV);// i.uv*gridSize;


					float2 gridUV = floor(localUV) + float2(dx, dy);
					gridUV = fmod(gridUV + gridSize, gridSize);
					localUV -= float2(dx, dy);
					localUV -= floorLocalUV;
					float texProb = GetTextureB4(_GradientTex, gridUV /gridSize );
					{
						//float4 a = GridCellCol(localUV - float2(dx, dy), gridUV + float2(dx, dy), gridSize);
						float4 a = GridCellCol(localUV , gridUV , gridSize, texProb);
						sum = max(a, sum);
						//sum += texProb*.3;

					}
				}
			}
			
			sum.a = 1;
			return ProcessOutput(sum);
		}

		float4 fragDirWarp(v2f i) : SV_Target
		{
			float2 uv;
			float dist = tex2D(_GradientTex, i.uv).r-0.5f;

//			uv.x = _Multiply.x*dist;
	//		uv.y = _Multiply.y*dist;
			uv.x = cos(dist*(_Multiply.x))*_Multiply.z*0.1f;
			uv.y = sin(dist*(_Multiply.x))*_Multiply.z*0.1f;

			float4 a = GetTextureMain4(_MainTex, i.uv + uv);

			
			return   ProcessOutput(a);
		}
		float4 fragDirWarpDist(v2f i) : SV_Target
		{
			float2 uv;
			float dist = tex2D(_GradientTex, i.uv).r;

			uv.x = cos((_Multiply.x))*dist*_Multiply.z*0.01f;
			uv.y = sin((_Multiply.x))*dist*_Multiply.z*0.01f;

			float4 a = GetTextureMain4(_MainTex, i.uv + uv);

			//a.r = 1;
			return   ProcessOutput(a);
		}
		float4 fragGradient(v2f i) : SV_Target
	{
		float index = tex2D(_MainTex, i.uv).r;
		float x = index;// +_TexSizeRecip.z;// *_TexSizeRecip.x;
		x = saturate(x);
		float4 color = GetTextureB4(_GradientTex, float2(x,0));
		//float4 color = float4(0,1,0,0);// tex2D(_GradientTex, i.uv);
		color.a = 1.0f;
		return ProcessOutput(color);
	}
	float4 fragClipMin(v2f i) : SV_Target
	{
		float4 tex = GetTextureMain4(_MainTex, i.uv);
		if (tex.r < _Multiply.r)
			tex.r = 0.0f;
		if (tex.g < _Multiply.g)
			tex.g = 0.0f;
		if (tex.b < _Multiply.b)
			tex.b = 0.0f;
		return ProcessOutput(tex);
	}

	float4 fragDistortAbs(v2f i) : SV_Target
	{
		float2 offset = float2(tex2D(_GradientTex, i.uv).r , tex2D(_GradientTex2, i.uv).r);// tex2D(_GradientTex, i.uv).r );
		float4 color = GetTextureMain4(_MainTex, offset);// offset.xy );
		return ProcessOutput(color);

	}
		
	float4 fragUVOffset(v2f i) : SV_Target
	{
		
		float u = GetTextureB4(_GradientTex, i.uv).r;
		float v = GetTextureB4(_GradientTex2, i.uv).r;

		float4 src = GetTextureMain4(_MainTex, i.uv+float2(u,v)*_Multiply.r);



		return ProcessOutput(src);

	}
	float4 fragCos(v2f i) : SV_Target
	{
		float4 src = GetTextureMain4(_MainTex, i.uv );
		src = cos(src*_Multiply.x)*_Multiply.y;

		return ProcessOutput(src);

	}

	float4 fragProbabilityBlend(v2f i) : SV_Target
	{
		float4 color0 = GetTextureMain4(_MainTex, i.uv);
		float4 color1 = GetTextureB4(_GradientTex, i.uv);
		float probability = tex2D(_GradientTex2, i.uv).r;

		float rand = wangHash(i.uv.x*1093+ i.uv.y * 999983+ m_GeneralInts.w)*2.0 / 4294967295.0f;
		float4 color = color0;
		if (probability > rand)
			color = color1;


		return ProcessOutput(color);

	}

		float4 fragMaskedBlend(v2f i) : SV_Target
	{
		float4 color0 = GetTextureMain4(_MainTex, i.uv);
		float4 color1 = GetTextureB4(_GradientTex, i.uv);
		float probability = tex2D(_GradientTex2, i.uv).r;

		float rand = wangHash(i.uv.x * 1093 + i.uv.y * 999983)*2.0 / 4294967295.0f;
		float4 color = color0;
		
		color = lerp(color0,color1, probability);

		return ProcessOutput(color);

	}

		float4 fragRandomEdge(v2f i) : SV_Target
	{
		float4 color = GetTextureMain4(_MainTex, i.uv);
		if (length(color.rgb) < _Multiply2.x)
		{
			float size = _Multiply.x;
			float dx;
			float dy;

			float4 colFinalr = float4(0, 0, 0, 0);
			float4 colStart = GetTextureMain4(_MainTex, i.uv);
			//				float scaleTex=_InvTexSize;

			float maxLen = sqrt(size*size + size*size);
			float4 maxcol = 0;
			[loop]
			for (dx = -size; dx <= size; dx += 1.0f)
			{
				[loop]
				for (dy = -size; dy <= size; dy += 1.0f)
				{
					float2 offset = float2(dx, dy);
					float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
					float4 col = GetTextureMain4lod(_MainTex, float4(i.uv.x + offsetTex.x, i.uv.y + offsetTex.y, 0, 0));
					maxcol = max(maxcol, col);
				}
			}


			float rand = wangHash(i.uv.x * 1093 + i.uv.y * 999983)*2.0 / 4294967295.0f;
			
			if (length(maxcol.rgb)>_Multiply.y && _Multiply.z > rand)
				color = 1;
		}

		return ProcessOutput(color);

	}

	float4 fragDistort(v2f i) : SV_Target
	{
		float4 color;
//	float count;
	float blur =  min(20, _TexSizeRecip.w);
	 blur = max(blur, 0.1);
	float maxLen = sqrt(blur * blur + blur * blur);
		//for (float dx = -blur; dx <= blur; dx += 1.0f)
		//for (float dy = -blur; dy <= blur; dy += 1.0f)
		{
			float dx = 0;
			float dy = 0;
			float2 delta = 0;// float2(dx*_TexSizeRecip.x, dy*_TexSizeRecip.y) / 3;
			float2 offset = -GetNorm(i.uv, _GradientTex,1.0).xy;// float2(tex2D(_GradientTex, i.uv + delta).r - 0.5, tex2D(_GradientTex, i.uv.yx + delta).r - 0.5);
			//float2 offset =  float2(tex2D(_GradientTex, i.uv + delta).r - 0.5, tex2D(_GradientTex, i.uv.yx + delta).r - 0.5);
   //		offset.g = -offset.g;
		   offset.rg *= _TexSizeRecip.xy;
		   offset.rg *= _Multiply.xy;
		   delta = float2(dx*_TexSizeRecip.x, dy*_TexSizeRecip.y) / 3;
		   float dist = length(delta);//sqrt(dot(offset,offset));
		   float gauss =  (1 + cos(3.14159*dist / maxLen)) / 2.0*3.14159;
		   
		   color =  GetTextureMain4(_MainTex, i.uv + offset.rg);// *gauss;
		   //count += gauss;
		}

		return ProcessOutput(color);
	}

	float4 fragEdgeDist(v2f i) : SV_Target
	{
		float size = floor(_Multiply.x);//50.0f;
		if (size < 1)
			size = 1;
		if (size > 100)
			size = 100;

		
		float dx;
		float dy;
		float startSize = (size + 10)*(size + 10);
		float distFinalr = startSize;

		float recipDist = 1.0 / size;//_Dist;

		float4 colFinalr = float4(0,0,0,0);
		float4 colStart = GetTextureMain4(_MainTex, i.uv);// tex2D(_MainTex, i.uv);
//		if (_Invert>0)
//			colStart = 1 - colStart;
		float2 bestOffset;
		for (dx = -size; dx<=size; dx += 1)
		{
			
			for (dy = -size; dy<=size; dy += 1)
			{
				float2 offset = float2(dx,dy);
				float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
				float4 col = GetTextureMain4(_MainTex, i.uv + offsetTex+ _TexSizeRecip.xy*0.0);// float4(i.uv.x, i.uv.y + offsetTex.y, 0, 0));
//				if (_Invert>0)
//					col = 1 - col;
				if (col.r>_Multiply.y)
				{
					float dist = dot(offset, offset);// length(offset);//sqrt(dot(offset,offset));
												//if(col.r>0.9)
					if (dist<distFinalr) 
					{
						bestOffset = offset;
						colStart = col;
						distFinalr = dist; 
					}
				}
			}
		}
		//local search around the found pixel
		if (distFinalr < startSize)
		{
/*
			distFinalr = startSize+100;
			for (dx = -1; dx <= 1; dx += 0.2)
			{

				for (dy = -1; dy <= 1; dy += 0.2)
				{
					float2 offset2 = float2(dx, dy)+bestOffset;

					float2 offsetTex = offset2*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
					float4 col = GetTextureMain4(_MainTex, i.uv + offsetTex + _TexSizeRecip.xy*0.0);// float4(i.uv.x, i.uv.y + offsetTex.y, 0, 0));
																									//				if (_Invert>0)
																									//					col = 1 - col;
					if (col.r>_Multiply.y)
					{
						float dist = dot(offset2, offset2);// length(offset);//sqrt(dot(offset,offset));
														 //if(col.r>0.9)
						if (dist<distFinalr)
						{
							colStart = col;
							distFinalr = dist;
						}
					}
				}
			}
*/		


			if(m_GeneralInts.x==0)
				colStart = 1.0f - saturate(sqrt(distFinalr) / size);//distFinalr/size;//colFinalr*distFinalr;//(1-saturate(  sqrt(distUse)*(colFinal.a) ));
			else
			{
				//leave colstart the colour of the pixel we found
			}

		}
		else
			colStart = 0.0;

//		if (colStart.r<0.2)
//			colStart = 0;
//		if (_Invert>0)
//			colStart = 1 - colStart;
//		colStart = 0.5;
		return ProcessOutput(colStart);

	}
		float4 fragEdgeDistDir(v2f i) : SV_Target
	{
		float size = floor(_Multiply.x);//50.0f;
	float size2 = _Multiply.x;//50.0f;
	float dx;
	float dy;

	float4 colFinalr = float4(0,0,0,0);
	float4 colStart = tex2D(_MainTex, i.uv);
	//		if (_Invert>0)
	//			colStart = 1 - colStart;
	float2 delta = 0;// float2(dx*_TexSizeRecip.x, dy*_TexSizeRecip.y) / 3;
	float2 offsetDir = float2(tex2D(_GradientTex, i.uv + delta).r - 0.5, tex2D(_GradientTex2, i.uv.yx + delta).r - 0.5);
	int len = length(offsetDir);
	offsetDir = normalize(offsetDir);
//	size = size*len;

	float distFinalr = size + 1000;

	float recipDist = 1.0 / size;//_Dist; 

	for (dx = 0; dx<size; dx += 1)
	{

	//	for (dy = -size2; dy<size2; dy += 1.0f)
		{
			float2 offset = offsetDir*dx;// float2(dx, dy);
			float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
			float4 col = tex2Dlod(_MainTex, float4(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y,0,0));
			//				if (_Invert>0)
			//					col = 1 - col;
			if (col.r>_Multiply.y)
			{
				float dist = length(offset);//sqrt(dot(offset,offset));
											//if(col.r>0.9)
				if (dist<distFinalr)
				{
					colStart = col;
					distFinalr = dist;
				}
			}
		}
	}
	if (distFinalr < size + 100)
	{
//		if (m_GeneralInts.x == 0)
			colStart = 1-saturate(distFinalr / size);//distFinalr/size;//colFinalr*distFinalr;//(1-saturate(  sqrt(distUse)*(colFinal.a) ));

	}
	else
		colStart = 0.0;

	//		if (colStart.r<0.2)
	//			colStart = 0;
	//		if (_Invert>0)
	//			colStart = 1 - colStart;
	
	colStart.a = 1;
	return ProcessOutput(colStart);

	}
	float4 fragEdgeDistFixedDir(v2f i) : SV_Target
	{
		float size = _Multiply.x;//50.0f;

		float dx;
		float dy;

		float4 colFinalr = float4(0,0,0,0);
		float4 colStart = tex2D(_MainTex, i.uv);

		float2 delta = 0;
		float angle = _Multiply.z*3.14159265/180.0f;
		float2 offsetDir = float2(cos(angle), sin(angle));
	
		offsetDir = normalize(offsetDir);
		float distFinalr = size + 1000;

		float recipDist = 1.0 / size;//_Dist; 

		for (dx = 0; dx<size; dx += 0.5)
		{
			float2 offset = offsetDir*dx;
			float2 offsetTex = offset*_TexSizeRecip.xy;
			float4 col = tex2Dlod(_MainTex, float4(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y,0,0));

			if (col.r>_Multiply.y)
			{
				float dist = length(offset);
											
				if (dist<distFinalr)
				{
					colStart = col;
					distFinalr = dist;
				}
			}
		}
		if (distFinalr < size + 100)
		{
			
			colStart = 1.0f - saturate(distFinalr / size);

		}
		else
			colStart = 0.0;


		return ProcessOutput(colStart);

	}
	float4 fragBlackEdge(v2f i) : SV_Target
	{
		float size = _Multiply.r;//50.0f;
		float dx;
		float dy;
		float distFinalr = size + 1000;

		float recipDist = 1.0 / size;//_Dist;

		float4 colFinalr = float4(0,0,0,0);
		float4 colStart = tex2D(_MainTex, i.uv);
		//		if (_Invert>0)
		//			colStart = 1 - colStart;

		for (dx = -size; dx<=size; dx += 1.0f)
		{
			for (dy = -size; dy<=size; dy += 1.0f)
			{
				float2 offset = float2(dx,dy);
				float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
				float4 col = tex2Dlod(_MainTex, float4(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y,0,0));
				//				if (_Invert>0)
				//					col = 1 - col;
				if ( length(colStart-col)>_Multiply.y)
				{
					float dist = length(offset);//sqrt(dot(offset,offset));
												//if(col.r>0.9)
					if (dist<distFinalr)
					{
//						colStart = col;
						distFinalr = dist;
					}
				}
			}
		}
		if (distFinalr < size + 100)
		{
			colStart = 0;// 1.0f - saturate(distFinalr / size);//distFinalr/size;//colFinalr*distFinalr;//(1-saturate(  sqrt(distUse)*(colFinal.a) ));
		}

		//		if (colStart.r<0.2)
		//			colStart = 0;
		//		if (_Invert>0)
		//			colStart = 1 - colStart;

		return ProcessOutput(colStart);

	}

	float4 fragQuadify(v2f i) : SV_Target
	{
		if (i.uv.x < 0.5)
		{
			if (i.uv.y >= 0.5)
				return GetTextureMain4(_MainTex, i.uv * 2);
			else
				return GetTextureB4(_GradientTex2, (i.uv - float2(0, 0.5)) * 2);
		}
		else
		{
			if (i.uv.y >= 0.5)
				return GetTextureMain4(_GradientTex, (i.uv - float2(0.5, 0)) * 2);
			else
				return GetTextureB4(_GradientTex3, (i.uv - float2(0.5, 0.5)) * 2);
		}
		
		return 0;
	}

	float4 fragCopyR(v2f i) : SV_Target
	{
		float4 col =  tex2D(_MainTex, i.uv).r;
		col *= _Multiply.x;
		//col.g = 0.5f; 
		col.a = 1;
		//col.b = 0;
		return col;// ProcessOutput(col);
	}

	float4 fragCopy(v2f i) : SV_Target
	{
		float4 col = tex2D(_MainTex, i.uv);
		//col.g = 0.5f; 
		col.a = 1;
		//col.b = 0;
		return col;
	}
	float4 fragCopyColAndAlpha(v2f i) : SV_Target
	{
		float4 col = GetTextureMain4(_MainTex,i.uv);
		float alpha=GetTextureB4(_GradientTex,i.uv).r;
		
		col.a = alpha;
		//col.b = 0;
		return col;
	}
	float4 fragCopyRGBAChannels(v2f i) : SV_Target
	{
		float r = GetTextureMain4(_MainTex,i.uv).r;
		float g = GetTextureB4(_GradientTex, i.uv).r;
		float b = GetTextureB4(_GradientTex2, i.uv).r;
		float alpha = GetTextureB4(_GradientTex3,i.uv).r;
		float4 col;
		col.r = r;
		col.g = g;
		col.b = b;
		col.a = alpha;
		
		return col;
	}
	float4 fragCopyColRGBA(v2f i) : SV_Target
	{
		float4 col = GetTextureMain4(_MainTex,i.uv);
		return col = ProcessOutput(col);
		
	}
	float4 fragCopyRAndAlpha(v2f i) : SV_Target
	{
		float4 col = GetTextureMain4(_MainTex,i.uv).r;
		float alpha = GetTextureB4(_GradientTex,i.uv).r;
		col.g = 0;
		col.b = 0;

		col.a = alpha;
		//col.b = 0;
		return col;
	}
/* //from unityCG.inc
inline fixed3 UnpackNormalDXT5nm (fixed4 packednormal)
{
fixed3 normal;
normal.xy = packednormal.wy * 2 - 1;
normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
return normal;
}

// Unpack normal as DXT5nm (1, y, 1, x) or BC5 (x, y, 0, 1)
// Note neutral texture like "bump" is (0, 0, 1, 1) to work with both plain RGB normal and DXT5nm/BC5
fixed3 UnpackNormalmapRGorAG(fixed4 packednormal)
{
// This do the trick
packednormal.x *= packednormal.w;

fixed3 normal;
normal.xy = packednormal.xy * 2 - 1;
normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
return normal;
}
inline fixed3 UnpackNormal(fixed4 packednormal)
{
#if defined(UNITY_NO_DXT5nm)
return packednormal.xyz * 2 - 1;
#else
return UnpackNormalmapRGorAG(packednormal);
#endif
}
*/
	//unity will unpack this 	
	float4 fragCopyNormal(v2f i) : SV_Target
	{
		float4 col = tex2D(_MainTex,i.uv);
		float4 norm=0;
		norm.y = col.y;
		norm.a = col.x;
		norm = col;
		norm.a = 1;
		
		return norm;
	}
	StructuredBuffer<uint4> _Histogram;

	float4 fragHistogram(v2f i) : SV_Target
	{
		float remapI = i.uv.x * 511.0;
		uint index = floor(remapI);
		float delta = frac(remapI);
		int _Channel = 0;
		float v1 = _Histogram[index][_Channel];
		float v2 = _Histogram[min(index + 1, 511)][_Channel];
		float h = v1;// *(1.0 - delta) + v2 * delta;
		uint y = (uint)round(i.uv.y * _Multiply.y);
		float4 color =  float4(0.1, 0.1, 0.1, 1.0);
		

		float fill = step(y, h);
		color = fill;// lerp(color, float4(1, 1, 1, 1), fill);
		color.a = 1;
		return color;


	}

		

		float4 fragNormal(v2f i) : SV_Target
	{

		float heightMapSizeX = _TexSizeRecip.x;
		float heightMapSizeY = _TexSizeRecip.y;

		float h0 = GetTextureMain4(_MainTex,i.uv).x;
		//		float n = tex2D(_MainTex,float2(i.uv.x,i.uv.y+1.0*heightMapSizeY)).x;
				float hdown = GetTextureMain4(_MainTex,float2(i.uv.x,i.uv.y - 1.0*heightMapSizeY)).x;
				float hright = GetTextureMain4(_MainTex,float2(i.uv.x + 1.0*heightMapSizeX,i.uv.y)).x;
				//		float w = tex2D(_MainTex,float2(i.uv.x-1.0*heightMapSizeX,i.uv.y)).x;                

						float3 right = float3(1, (hright - h0)* _TexSizeRecip.z, 0);
						float3 down = float3(0, (hdown - h0) * _TexSizeRecip.z, 1.0f);

						float3 norm = cross(normalize(down),normalize(right) );
						norm = normalize(norm);
						norm.z *= -1.0f;

						norm = norm*0.5 + 0.5f; 

						float4 color;
						color.xzy = norm;

						color.a = 1.0f;

						return ProcessOutput(color);
						
	}
		// Pseudo random number generator with 2D coordinates
		float UVRandom(float u, float v)
	{
		float f = dot(float2(12.9898, 78.233), float2(u, v));
		return frac(43758.5453 * sin(f));
	}
/*
	// Sample point picker
	float3 PickSamplePoint(float2 uv, float index)
	{
		// Uniformaly distributed points on a unit sphere http://goo.gl/X2F1Ho
#if defined(FIX_SAMPLING_PATTERN)
		float gn = GradientNoise(uv * _Downsample);
		// FIXME: This was added to avoid a NVIDIA driver issue.
		//                                   vvvvvvvvvvvv
		float u = frac(UVRandom(0.0, index + uv.x * 1e-10) + gn) * 2.0 - 1.0;
		float theta = (UVRandom(1.0, index + uv.x * 1e-10) + gn) * UNITY_PI_2;
#else
		float u = UVRandom(uv.x + _Time.x, uv.y + index) * 2.0 - 1.0;
		float theta = UVRandom(-uv.x - _Time.x, uv.y + index) * UNITY_PI_2;
#endif
		float3 v = float3(CosSin(theta) * sqrt(1.0 - u * u), u);
		// Make them distributed between [0, _Radius]
		float l = sqrt((index + 1.0) / _SampleCount) * _Radius;
		return v * l;
	}

*/
		ENDCG
		SubShader {
		Pass{ //0
			 ZTest Always Cull Off ZWrite Off

			 CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMult
			 ENDCG
		}
			Pass{//1
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragGradient
			ENDCG
		}
			Pass{//2
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDistort
			ENDCG
		}

			Pass{//3
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragNormal
			ENDCG
		}
			Pass{//4
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragAdd
			ENDCG
		}
			Pass{//5
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragAdd2
			ENDCG
		}
			Pass{//6
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMult2
			ENDCG
		}
			Pass{//7
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragPow2
			ENDCG
		}
			Pass{//8
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMin
			ENDCG
		}
			Pass{//9
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragMax
			ENDCG
		}
			Pass{//10
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragClipMin
			ENDCG
		}
			Pass{//11
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragBlend2
			ENDCG
		}
			Pass{//12
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSub2
			ENDCG
		}
			Pass{//13
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragLevel1
				ENDCG
		}
			Pass{//14
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragTransform
				ENDCG
		}
			Pass{//15
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDirWarp
				ENDCG
		}
			Pass{//16
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragPower
				ENDCG
		}
			Pass{//17
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMin1
				ENDCG
		}
			Pass{//18
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMax1
				ENDCG
		}
			Pass{//19
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragCopyR 
					ENDCG
		}
			Pass{//20
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragCopy
					ENDCG
		}
			Pass{//21
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragStep1
			ENDCG
		}
			Pass{//22
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragInvert
					ENDCG
		}
			Pass{//23
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragSrcBlend
					ENDCG
		}
			Pass{//24
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragSrcStepify
					ENDCG
		}
			Pass{//25
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragEdgeDist
					ENDCG
		}
	Pass{//26
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSmooth
			ENDCG
		}
	Pass{//27
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragBlackEdge
			ENDCG
		}
	Pass{//28
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragCopyColAndAlpha
			ENDCG
		}
	Pass{//29
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragEdgeDistDir
				ENDCG
			}
	Pass{//30
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSplatter
			ENDCG
		}//
				Pass{//31
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSplatterGrid
				ENDCG
			}//
				Pass{//32
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSobel
				ENDCG
			}//
				Pass{//33
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragAnimCurveAxis
				ENDCG
			}//
				Pass{//34
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDistortAbs
				ENDCG
			}//
				Pass{//35
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMapCylinder
				ENDCG
			}//
				Pass{//36
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragCopyRAndAlpha
				ENDCG
			}
		Pass{//37
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragHistogram
				ENDCG
			}
		Pass{//38
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragProbabilityBlend
		ENDCG
				}
	Pass
	{//39
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragRandomEdge
		ENDCG
	}
	Pass
	{//40
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragCopyColRGBA
		ENDCG
	}
		Pass
	{//41
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragCopyNormal
		ENDCG
	}
		Pass
	{//42
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMaskedBlend
		ENDCG
	}
	Pass
	{//43
		ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSharpen
			ENDCG
	}
		Pass
	{//44
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragEmboss
		ENDCG
	}
		Pass
	{//45
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDivide2
		ENDCG
	}
		Pass
	{//46
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDirWarpDist
		ENDCG
	}	
		Pass
	{//47
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragPosterize
		ENDCG
	}
			Pass
		{//48
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDot
			ENDCG
		}
			Pass
		{//49
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragUVOffset
			ENDCG
		}

		Pass
		{//50
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragCos
			ENDCG
		}

		Pass
		{//51
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragAnimCurveDraw
			ENDCG
		}
			Pass{//52
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragEdgeDistFixedDir
			ENDCG
		}
		Pass{//53
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragAO
			ENDCG
		}
		Pass{//54
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSplatterGridProb 
		ENDCG
	}
			Pass{//55
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSmoothedMask
			ENDCG
		}
			Pass{//56
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSmoothDir
			ENDCG
		}
			Pass{//57
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragCopyRGBAChannels
			ENDCG
		}
			Pass{//58
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragQuadify
			ENDCG
		}
			Pass{//59
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMapSphere
			ENDCG
		}
			Pass{//60
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSymetry
			ENDCG
		}
			Pass{//61
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSkew
			ENDCG
		}
			Pass{//62
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSquish
			ENDCG
		}
	}
	

	Fallback off
}
