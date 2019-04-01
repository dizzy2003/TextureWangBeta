// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/CreateTexture" {
	Properties 
	{ 
	 _Multiply ("_Multiply", Vector) = (1,1,1,1)
     _TexSizeRecip("_TexSizeRecip", Vector) = (.1,.1,.1,.1)
	}
	CGINCLUDE
	#include "UnityCG.cginc"
	struct v2f {
		float4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
	};

	float4 _Multiply;
	float4 _Multiply2;
	float4 _TexSizeRecip;
	uniform sampler2D _PermTable1D, _Gradient2D;
	uniform float _Frequency, _Lacunarity, _Gain;
	uniform float _Jitter;
	uniform int   _Octaves;
	int _InvertOutput;
	float4 _ScaleOutput;
	float4 _ScaleOutput2;
	
	int _Saturate;

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
	v2f vertMult(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex); 
		o.uv = v.texcoord ; 
		return o;
	}
	float2 fade(float2 t)
	{
		return 1-t * t * t * (t * (t * 6 - 15) + 10);
	}
	float4 ProcessOutput(float4 _col)
	{
		if (_InvertOutput)
			_col.rgb = 1 - _col.rgb;
//		_col.rgb *= _ScaleOutput;
		_col = LevelsControl(_col, _ScaleOutput.x, _ScaleOutput2.x, _ScaleOutput.y, _ScaleOutput.z, _ScaleOutput.w);

		if (_Saturate)
			_col = saturate(_col);
		if (_ScaleOutput2.y > 0.0f)
			_col = step(_ScaleOutput2.y, _col);


		return _col;
	}


	float perm(float x)
	{
		return tex2D(_PermTable1D, float2(x, 0)).a;
	}

	float grad(float x, float2 p) 
	{
		float2 g = tex2D(_Gradient2D, float2(x, 0)).rg *2.0 - 1.0;
		return dot(g, p);
	}
	//https://gamedev.stackexchange.com/questions/23625/how-do-you-generate-tileable-perlin-noise
	float surflet(float2 xy,float2 grid, float2 repeatPeriod)
	{
		float2 distPos = abs(xy - grid);
		float2 dist = (xy - grid);
		float2 poly =  fade(distPos);
		float hashed = (perm( perm( floor(fmod(grid.x, repeatPeriod.x )) / 256.0) + floor(fmod(grid.y, repeatPeriod.y )) / 256.0));
		float gradient =  grad(hashed, dist);
		return poly.x*poly.y*gradient;
	}
	float inoise(float2 xy, float2 repeatPeriod)
	{
		float2 xyInt = floor(xy);
		return (surflet(xy, xyInt, repeatPeriod)
				+ surflet(xy, xyInt + float2(1, 0), repeatPeriod) 
				+ surflet(xy, xyInt + float2(0, 1), repeatPeriod) 
				+ surflet(xy, xyInt + float2(1, 1), repeatPeriod));
	}
//http://staffwww.itn.liu.se/~stegu/aqsis/aqsis-newnoise/noise1234.cpp
	
/*
	float inoise(float2 p,float repeatPeriod)
	{
		float2 P = fmod(floor(p), 256.0);	// FIND UNIT SQUARE THAT CONTAINS POINT
		p -= floor(p);                      // FIND RELATIVE X,Y OF POINT IN SQUARE.
		float2 f = fade(p);                 // COMPUTE FADE CURVES FOR EACH OF X,Y.

		P = P / 256.0;
		const float one = 1.0 / 256.0;

		// HASH COORDINATES OF THE 4 SQUARE CORNERS
		float A = perm(P.x) + P.y;
		float B = perm(P.x + one) + P.y;

		// AND ADD BLENDED RESULTS FROM 4 CORNERS OF SQUARE 
		return lerp(lerp(grad(perm(A), p),
			grad(perm(B), p + float2(-1, 0)), f.x),
			lerp(grad(perm(A + one), p + float2(0, -1)),
				grad(perm(B + one), p + float2(-1, -1)), f.x), f.y);

	}
*/

	 float3 mod289(float3 x) {
		 return x - floor(x * (1.0 / 289.0)) * 289.0;
	 }

	 float4 mod289(float4 x) {
		 return x - floor(x * (1.0 / 289.0)) * 289.0;
	 }

	 float4 permute(float4 x) {
		 return mod289(((x*34.0) + 1.0)*x);
	 }

	 float4 taylorInvSqrt(float4 r)
	 {
		 return 1.79284291400159 - 0.85373472095314 * r;
	 }
//https://github.com/ashima/webgl-noise/blob/master/src/noise3D.glsl
	 float snoise3D(float3 v)
	 {
		 const float2  C = float2(1.0 / 6.0, 1.0 / 3.0);
		 const float4  D = float4(0.0, 0.5, 1.0, 2.0);

		 // First corner
		 float3 i = floor(v + dot(v, C.yyy));
		 float3 x0 = v - i + dot(i, C.xxx);

		 // Other corners
		 float3 g = step(x0.yzx, x0.xyz);
		 float3 l = 1.0 - g;
		 float3 i1 = min(g.xyz, l.zxy);
		 float3 i2 = max(g.xyz, l.zxy);

		 //   x0 = x0 - 0.0 + 0.0 * C.xxx;
		 //   x1 = x0 - i1  + 1.0 * C.xxx;
		 //   x2 = x0 - i2  + 2.0 * C.xxx;
		 //   x3 = x0 - 1.0 + 3.0 * C.xxx;
		 float3 x1 = x0 - i1 + C.xxx;
		 float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
		 float3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

									// Permutations
		 i = mod289(i);
		 float4 p = permute(permute(permute(
			 i.z + float4(0.0, i1.z, i2.z, 1.0))
			 + i.y + float4(0.0, i1.y, i2.y, 1.0))
			 + i.x + float4(0.0, i1.x, i2.x, 1.0));

		 // Gradients: 7x7 points over a square, mapped onto an octahedron.
		 // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
		 float n_ = 0.142857142857; // 1.0/7.0
		 float3  ns = n_ * D.wyz - D.xzx;

		 float4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

		 float4 x_ = floor(j * ns.z);
		 float4 y_ = floor(j - 7.0 * x_);    // mod(j,N)

		 float4 x = x_ *ns.x + ns.yyyy;
		 float4 y = y_ *ns.x + ns.yyyy;
		 float4 h = 1.0 - abs(x) - abs(y);

		 float4 b0 = float4(x.xy, y.xy);
		 float4 b1 = float4(x.zw, y.zw);

		 //float4 s0 = float4(lessThan(b0,0.0))*2.0 - 1.0;
		 //float4 s1 = float4(lessThan(b1,0.0))*2.0 - 1.0;
		 float4 s0 = floor(b0)*2.0 + 1.0;
		 float4 s1 = floor(b1)*2.0 + 1.0;
		 float4 sh = -step(h, float4(0,0,0,0));

		 float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
		 float4 a1 = b1.xzyw + s1.xzyw*sh.zzww;

		 float3 p0 = float3(a0.xy, h.x);
		 float3 p1 = float3(a0.zw, h.y);
		 float3 p2 = float3(a1.xy, h.z);
		 float3 p3 = float3(a1.zw, h.w);

		 //Normalise gradients
		 float4 norm = taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
		 p0 *= norm.x;
		 p1 *= norm.y;
		 p2 *= norm.z;
		 p3 *= norm.w;

		 // Mix final noise value
		 float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
		 m = m * m;
		 return 42.0 * dot(m*m, float4(dot(p0, x0), dot(p1, x1),
			 dot(p2, x2), dot(p3, x3)));
	 }
	// fractal sum, range -1.0 - 1.0
	float fBm(float2 p, int octaves, float2 repeatPeriod)
	{
		float freq = 1.0f, amp = 1.0;
		float sum = 0;
//		float3 p3 = float3(p.x, p.y, 0.5f);
		for (int i = 0; i < octaves; i++)
		{
			sum += (inoise(p * freq, repeatPeriod)) * amp;
			freq *= _Lacunarity;
			amp *= _Gain;
			repeatPeriod *= 2.0f;
		}
//		sum = tex2D(_Gradient2D, float2(p.x, 0)).r;
		return saturate(sum *0.5f + 0.5f);
	}
	float fBm3D(float3 p, int octaves)
	{
		float freq = 1.0f, amp = 1.0;
		float sum = 0;
		//		float3 p3 = float3(p.x, p.y, 0.5f);
		for (int i = 0; i < octaves; i++)
		{
			sum += (snoise3D(p * freq)) * amp;
			freq *= _Lacunarity;
			amp *= _Gain;

		}
		//		sum = tex2D(_Gradient2D, float2(p.x, 0)).r;
		return saturate(sum *0.5f + 0.5f);
	}
	// fractal abs sum, range 0.0 - 1.0
	float turbulence(float2 p, int octaves)
	{
		float sum = 0;
		float freq = _Frequency, amp = 1.0;
		for (int i = 0; i < octaves; i++)
		{
			sum += abs(inoise(p*freq,256.0))*amp;
			freq *= _Lacunarity;
			amp *= _Gain;
		}
		return sum;
	}

	// Ridged multifractal, range 0.0 - 1.0
	// See "Texturing & Modeling, A Procedural Approach", Chapter 12
	float ridge(float h, float offset)
	{
		h = abs(h);
		h = offset - h;
		h = h * h;
		return h;
	}

	float ridgedmf(float2 p, int octaves, float offset)
	{
		float sum = 0;
		float freq = _Frequency, amp = 0.5;
		float prev = 1.0;
		for (int i = 0; i < octaves; i++)
		{
			float n = ridge(inoise(p*freq,256.0), offset);
			sum += n*amp*prev;
			prev = n;
			freq *= _Lacunarity;
			amp *= _Gain;
		}
		return sum;
	}

	float4 fragNoiseBm(v2f i) : COLOR
	{
		
		float n = saturate(fBm(i.uv*_Multiply2.xy+ _Multiply2.zw, _Octaves,_Multiply2.xy));


		return ProcessOutput(float4(n, n, n, 1));
	}

	float4 fragNoiseBm3d(v2f i) : COLOR
	{
		float2 uv = i.uv*_Multiply2.xy + _Multiply2.zw;
		float3 uvw = float3(uv.x, uv.y, _Multiply.z);
		float n =  saturate(fBm3D(uvw, _Octaves));


	return ProcessOutput(float4(n, n, n, 1));
	}
		float4 fragNoiseTurbulence(v2f i) : COLOR
	{
		//float n = turbulence(i.uv, 4); //was uv.xz
		float n = saturate(turbulence(i.uv*_Multiply2.xy + _Multiply2.zw, _Octaves));

	return ProcessOutput(float4(n, n, n, 1));
	}
/*
	float4 fragNoiseRidged(v2f i) : COLOR
	{
		float n = ridgedmf(i.uv, 4, 1.0);

		return float4(n,n,n,1);
	}
*/
		float4 fragNoiseRidged(v2f i) : COLOR
	{
		//float n = ridgedmf(i.uv, 4, 1.0);
		float n = saturate(ridgedmf(i.uv*_Multiply2.xy + _Multiply2.zw, _Octaves,1.0f));

	return ProcessOutput(float4(n, n, n, 1));
	}
		float4 fragSetCol(v2f i) : SV_Target
	{
		float4 color = _Multiply;
		color.a = 1.0f;
		return saturate(color);
	}


	

	//1/7
#define K 0.142857142857
	//3/7
#define Ko 0.428571428571

	float3 mod(float3 x, float y) { return x - y * floor(x / y); }
	float2 mod(float2 x, float y) { return x - y * floor(x / y); }

	// Permutation polynomial: (34x^2 + x) mod 289
	float3 Permutation(float3 x)
	{
		return mod((34.0 * x + 1.0) * x, 289.0);
	}

	float2 inoisejitter(float2 P, float jitter)
	{
		float2 Pi = mod(floor(P), 289.0);
		float2 Pf = frac(P);
		float3 oi = float3(-1.0, 0.0, 1.0);
		float3 of = float3(-0.5, 0.5, 1.5);
		float3 px = Permutation(Pi.x + oi);

		float3 p, ox, oy, dx, dy;
		float2 F = 1e6;

		for (int i = 0; i < 3; i++)
		{
			p = Permutation(px[i] + Pi.y + oi); // pi1, pi2, pi3
			ox = frac(p*K) - Ko;
			oy = mod(floor(p*K), 7.0)*K - Ko;
			dx = Pf.x - of[i] + jitter*ox;
			dy = Pf.y - of + jitter*oy;

			float3 d = dx * dx + dy * dy; // di1, di2 and di3, squared

										  //find the lowest and secoond lowest distances
			for (int n = 0; n < 3; n++)
			{
				if (d[n] < F[0])
				{
					F[1] = F[0];
					F[0] = d[n];
				}
				else if (d[n] < F[1])
				{
					F[1] = d[n];
				}
			}
		}

		return F;
	}

	float fBm_F0(float2 p, int octaves)
	{
		float freq = _Frequency, amp = 0.5;
		float sum = 0;
		for (int i = 0; i < octaves; i++)
		{
			float2 F = inoisejitter(p * freq, _Jitter) * amp;

			sum += 0.1 + sqrt(F[0]);

			freq *= _Lacunarity;
			amp *= _Gain;
		}
		return sum;
	}

	float fBm_F1_F0(float2 p, int octaves)
	{
		float freq = _Frequency, amp = 0.5;
		float sum = 0;
		for (int j = 0; j < octaves; j++)
		{
			freq *= _Lacunarity;
			amp *= _Gain;
		}
		int i = octaves;
		
		{
			float2 F = inoisejitter(p * freq, _Jitter) * amp;
/*
			if (abs(sqrt(F[1]) - sqrt(F[0])) < 0.005f)
				return 1;
			else
				return 0;
*/
			return sqrt(F[0]);
			sum += 0.1 + sqrt(F[1]) - sqrt(F[0]);

			freq *= _Lacunarity;
			amp *= _Gain;
		}
		return sum;
	}

	float4 fragNoiseVeroni(v2f i) : COLOR
	{
		float n = fBm_F1_F0(i.uv, _Octaves);

	return ProcessOutput(float4(n, n, n, 1));
	}
	float4 fragPattern(v2f i) : COLOR
	{

		float c =  cos(i.uv.x*_Multiply.x+_Multiply2.x);
		float s = cos(i.uv.y*_Multiply.y + _Multiply2.y);
		float n = (c + s);
		if (_Gain > 0.5)
			n = abs(n);
		n *= 0.5f;
		return ProcessOutput(float4(n, n, n, 1));
		
	}
		float4 fragRipples(v2f i) : COLOR
	{

		float c = (i.uv.x - _Multiply2.x)*_Multiply.x;
		float s = (i.uv.y- _Multiply2.y)*_Multiply.y;
		float n = sqrt(c*c+s*s);
		n = cos(n*_Multiply.z+ _Multiply.w);
		if (_Gain > 0.5)
			n = abs(n);
		
		return ProcessOutput(float4(n, n, n, 1));

	}
		float getGridRandom(float2 grid)
	{
		int index = grid.x * 7 + grid.y*_Multiply.x * 19;
		index *= 17919;
		index *= (int)grid.x ^ (((int)grid.y)<<4);
		index = (index >> 20) & 0x3f;

		return   ((float)index / (float)(0x3f));
	}

	float4 fragGrid(v2f i) : COLOR 
	{
		//calc mid point of the brick
		
		float2 uv = i.uv;

		//get an offset for every other row
		float h2 = floor(i.uv.y *_Multiply.y) - floor(i.uv.y *_Multiply.y*0.5f)*2.0f;
		uv.x += h2*_Multiply2.x / _Multiply.x;

		float2 mid = floor(uv*_Multiply.xy) + 0.5f;
		

		//turn the uv into brick position
		float2 p = uv*_Multiply.xy;

		//p.x += 0.5f*h2;

//		float h = 1.0f-length(p - mid);
		//delta to nearest mid point of current brick
		float2 delta = ((p - mid)); //-0.5 to 0.5

		float yaw = (getGridRandom(mid*(1.0f+_Multiply2.w)*23.0f)-0.5f)*_Multiply2.y;
		float cy = cos(yaw);
		float sy = sin(yaw);
		
		float dx = delta.x*cy - delta.y*sy;
		float dy = delta.x*sy + delta.y*cy;
		delta.x = dx;
		delta.y = dy;


		delta.y *= _Multiply.x / _Multiply.y;

		delta = abs(delta);
//		delta= delta*delta;
		
		
		//black mortar
		float h = min(0.6f-delta.x , 0.6f*_Multiply.x / _Multiply.y -delta.y );
		
		//h = 1-length(delta);
		//fill the center of the brick with solid white
		h = min(h*_Multiply.z*10,1-step(h, _Multiply.w));
		
		
		h *= (1.0f- _Multiply2.z) + getGridRandom(mid*(1.0f + _Multiply2.w)*11.0f)*_Multiply2.z;
		//h = _Multiply2.z;

/*
		float h2 = floor(i.uv.y *_Multiply.y) -floor(i.uv.y *_Multiply.y*0.5f)*2.0f;
	
		float w = ((i.uv.x+h2*1/_Multiply.x*0.5) % (1 / _Multiply.x))*_Multiply.x;
		w = step(_Multiply.z, w);
		float h = (i.uv.y % (1 / _Multiply.y))*_Multiply.y;
	
		h = step(_Multiply.z, w*h);
		//		float w = step(_Multiply.x, (i.uv.x%_Multiply.z) * 10) - step(1 - _Multiply.x,(i.uv.x%_Multiply.z) * 10);
		//		float h = step(_Multiply.y , (i.uv.y%_Multiply.z) * 10) - step(1 - _Multiply.y, (i.uv.y%_Multiply.z) * 10);
*/
		float n = saturate(h);// lerp(0, 1, w*h);

		return ProcessOutput(float4(n,n,n,1));
	}

	float4 fragGrid2(v2f i) : COLOR
	{
		float h2 = floor(i.uv.y *_Multiply.y) - floor(i.uv.y *_Multiply.y*0.5f)*2.0f;

	float w = ((i.uv.x + h2 * 1 / _Multiply.x*0.5) % (1 / _Multiply.x))*_Multiply.x;
	w = step(_Multiply.z, w);
	float h = (i.uv.y % (1 / _Multiply.y))*_Multiply.y;

	h = step(_Multiply.z, w*h);
	//		float w = step(_Multiply.x, (i.uv.x%_Multiply.z) * 10) - step(1 - _Multiply.x,(i.uv.x%_Multiply.z) * 10);
	//		float h = step(_Multiply.y , (i.uv.y%_Multiply.z) * 10) - step(1 - _Multiply.y, (i.uv.y%_Multiply.z) * 10);
	float n = saturate(h);// lerp(0, 1, w*h);

	return ProcessOutput(float4(n, n, n, 1));
	}
	float4 fragWeave(v2f i) : COLOR
	{
		float shadowScale = _Multiply.z*_Multiply.w;
	//create vertical striped going 0..1..0 
		float vertStripesFrac = 1.0f-abs(frac(i.uv.x*_Multiply.x)-0.5f)*2;
		//vertStripesFrac = cos((1-vertStripesFrac)*3.14159);

	//only the inner high part is a 1
		float vertStripes = step(_Multiply.z, vertStripesFrac);
    //create a shadow edge beside the main strip
		float vertStripes2 = saturate((vertStripesFrac - shadowScale)*(1.0f/ shadowScale));

		float hStripesFrac = 1.0f - abs(frac(i.uv.y*_Multiply.y)-0.5f) * 2;
		float hStripes = step(_Multiply.z, hStripesFrac);
		float hStripes2 = saturate((hStripesFrac - shadowScale)*(1.0f / shadowScale));
		
		//odd squares use the main vertical strip with the horizontal strip added but faded by and shadow value
		float n = vertStripes + (1.0f-vertStripes2)*hStripes;
		//float n = max(vertStripes, hStripes);

		//calc checker pattern
		float total = floor(i.uv.x*_Multiply.x*1.0f) + floor(i.uv.y*_Multiply.y*1.0f);
		bool isEven = fmod(total, 2.0) == 0.0;
		//return (isEven) ? col1 : col2;
		//n= (isEven) ? vertStripes + (1.0f - vertStripes2)*hStripes : hStripes + (1.0f - hStripes2)*vertStripes;
		if(isEven)
			n=hStripes + (1.0f - hStripes2)*vertStripes;

		return ProcessOutput(float4(n, n, n, 1));
		//return (isEven) ? float4(n, 0, 0, 1) : float4(0, n, 0, 1);
	}
	float4 fragCircle(v2f i) : SV_Target
	{
		float4 col = 0;
		float dx = (i.uv.x - 0.5 - _Multiply.y)/_Multiply.x;
		float dy = (i.uv.y - 0.5 - _Multiply.z)/_Multiply.w;
		float len = length(dx*dx + dy*dy);

		if (len < 1 && len>1 - _Multiply2.x)//_Multiply.x)
		{
			if(_Multiply2.y != 0)
				col = saturate((len));// / _Multiply.x));
			else
				col = saturate(1 - (len));// / _Multiply.x));
		}

		return ProcessOutput(col);
	}
	float4 fragDistance(v2f i) : SV_Target
	{
		float4 col = 0;
		float dx = (i.uv.x - 0.5 - _Multiply.x);
		float dy = (i.uv.y - 0.5 - _Multiply.y);
		float len = length(dx*dx + dy*dy)*_Multiply.z;

		col = len;

		return ProcessOutput(col);
	}
		float4 fragSegment(v2f i) : SV_Target
	{
		float4 col = 0;
		float dx = (i.uv.x - 0.5 - _Multiply.z); // _Multiply.x;
		float dy = (i.uv.y - 0.5 - _Multiply.w); // _Multiply.w;
		//float len = length(dx*dx + dy*dy);
		float a=atan2(dy, dx)+ _Multiply2.x;

		if(a>_Multiply.x && a<  _Multiply.y)
		//if (len < 1 && len>1 - _Multiply2.x)//_Multiply.x)
		{
			if (_Multiply2.y != 0)
				col = saturate(1);// / _Multiply.x));
			else
				col = saturate((a- _Multiply.x)/(_Multiply.y- _Multiply.x));// / _Multiply.x));
		}

		return ProcessOutput(col);
	}
	float4 fragRectangle(v2f i) : SV_Target
	{
		float4 col = 0;
		float dx = ((i.uv.x - 0.5 - _Multiply.y) / _Multiply.x)*2;
		float dy = ((i.uv.y - 0.5 - _Multiply.z) / _Multiply.w)*2;
		float len = max(abs(dx) , abs(dy));// length(dx*dx + dy*dy);

		{
			if (len < 1 && len>1-_Multiply2.x)//_Multiply.x)
			{
				if (_Multiply2.y != 0)
					col = saturate((len));// / _Multiply.x));
				else
					col = saturate(1 - (len));// / _Multiply.x));
			}
		}

		return ProcessOutput(col);
	}
		float4 fragHexagon(v2f i) : SV_Target
	{
		float4 col = 0;
		float v = _Multiply.x*0.5;
		float h = v*1.732 ;
		float q2x = abs((i.uv.x - 0.5 - _Multiply.y));
		float q2y = abs((i.uv.y - 0.5 - _Multiply.z))*_Multiply.x/ _Multiply.w;

		if (q2x > h || q2y > v * 2)
		{
			col = 0;
		}
		else
		{
			if (2 * v * h - v * q2x - h * q2y >= 0) 
			{
/*
				if (q2x < q2y*1.732) //1.732 is root 3
				{
					float newx = abs(q2x*0.5 - q2y*0.866);
					//float2 edgeDir = float2(1.732 / 2, 0.5); //cos 30 sin 30
					
					//float2 dir = float2(q2x, q2y);
					//float d = dot(dir, edgeDir);

					col = newx;// 1 - (h - newx) / h;
				}
				else
*/
				col = 1;// -(h - q2x) / h;
			}
		}
		return ProcessOutput(col);
	}
		uint wangHash(int _Seed)
	{
		_Seed = (_Seed ^ 61) ^ (_Seed >> 16);
		_Seed *= 9;
		_Seed = _Seed ^ (_Seed >> 4);
		_Seed *= 0x27d4eb2d;
		_Seed = _Seed ^ (_Seed >> 15);
		return _Seed;
	}

		float4 fragNoise(v2f i) : SV_Target
	{

		float rand = wangHash(i.uv.x * 1093 + i.uv.y * 999983 + _Multiply.x)*2.0 / 4294967295.0f;
		float4 color = rand;
		return ProcessOutput(color);

	}
		ENDCG
	SubShader {
		Pass
		{//0
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragSetCol
			ENDCG
		}
		Pass
		{//1
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragNoiseBm
			ENDCG
		}
			Pass
		{//2
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragNoiseTurbulence
			ENDCG
		}
			Pass
		{//3
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragNoiseRidged
			ENDCG
		}
		Pass
		{//4
			ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragNoiseVeroni
				ENDCG
		}
		Pass
		{//5
			ZTest Always Cull Off ZWrite Off 

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragPattern
				ENDCG
		}
			Pass
		{//6
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragGrid
			ENDCG
		}
			Pass
		{//7
			ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragWeave
				ENDCG
		}
			Pass
		{//8 circle
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragCircle
			ENDCG
		}
			Pass
		{//9 ripples
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragRipples
			ENDCG
		}
			Pass
		{//10 Rectangle
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragRectangle
			ENDCG
		}
		Pass
	{//11 Hexagon
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragHexagon
		ENDCG
	}
			Pass
		{//12 whiteNoise/wang
			ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragNoise
				ENDCG
		}
		Pass
		{//13 Dist
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDistance
			ENDCG
		}
			Pass
		{//14 Segment
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSegment
			ENDCG
		}
			Pass
		{//15
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragNoiseBm3d
			ENDCG
		}
	}

	Fallback off
}
