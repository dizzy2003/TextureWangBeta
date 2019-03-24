Shader "Tessellation/Standard Fixed" {
        Properties {
            _Tess ("Tessellation", Range(1,128)) = 4
            _MainTex ("Base (RGB)", 2D) = "white" {}
            _MOS ("Metallic (R), Occlussion (G), Smoothness (B)", 2D) = "white" {}
            _DispTex ("Disp Texture", 2D) = "gray" {}
            _NormalMap ("Normalmap", 2D) = "bump" {}
            _Displacement ("Displacement", Range(0, 1.0)) = 0.3
//			_ParallaxStrength("_ParallaxStrength", Range(0, 1.0)) = 0.3
            _DispOffset ("Disp Offset", Range(0, 1)) = 0.5
            _Color ("Color", color) = (1,1,1,0)
            _Metallic ("Metallic", Range(0, 1)) = 0.5
            _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        }
        SubShader {
            Tags { "RenderType"="Opaque" }
            LOD 300
            
            CGPROGRAM
            #pragma surface surf Standard addshadow fullforwardshadows vertex:disp tessellate:tessFixed
            #pragma target 5.0
				sampler2D _DispTex;
			sampler2D _MOS;
			uniform float4 _DispTex_ST;
			float _Displacement;
			float _DispOffset;
			float _ParallaxStrength;
            struct appdata {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
				float3 tangentViewDir : TEXCOORD3;
            };

			struct Input {
				float2 uv_MainTex;
				float2 uv_MOS;
				float3 tangentViewDir;
			};

#define PARALLAX_BIAS 0
			//	#define PARALLAX_OFFSET_LIMITING
#define PARALLAX_RAYMARCHING_STEPS 50
#define PARALLAX_RAYMARCHING_INTERPOLATE
#define PARALLAX_RAYMARCHING_SEARCH_STEPS 3
#define PARALLAX_FUNCTION ParallaxRaymarching


			float GetParallaxHeight(float2 uv) {
				return tex2D(_DispTex, uv).g;
			}

			float2 ParallaxOffset(float2 uv, float2 viewDir) {
				float height = GetParallaxHeight(uv);
				height -= 0.5;
				height *= _ParallaxStrength;
				return viewDir * height;
			}

			float2 ParallaxRaymarching(float2 uv, float2 viewDir) {
#if !defined(PARALLAX_RAYMARCHING_STEPS)
#define PARALLAX_RAYMARCHING_STEPS 10
#endif
				float2 uvOffset = 0;
				float stepSize = 1.0 / PARALLAX_RAYMARCHING_STEPS;
				float2 uvDelta = viewDir * (stepSize * _ParallaxStrength);

				float stepHeight = 1;
				float surfaceHeight = GetParallaxHeight(uv);

				float2 prevUVOffset = uvOffset;
				float prevStepHeight = stepHeight;
				float prevSurfaceHeight = surfaceHeight;

				for (
					int i = 1;
					i < PARALLAX_RAYMARCHING_STEPS && stepHeight > surfaceHeight;
					i++
					) {
					prevUVOffset = uvOffset;
					prevStepHeight = stepHeight;
					prevSurfaceHeight = surfaceHeight;

					uvOffset -= uvDelta;
					stepHeight -= stepSize;
					surfaceHeight = GetParallaxHeight(uv + uvOffset);
				}

#if !defined(PARALLAX_RAYMARCHING_SEARCH_STEPS)
#define PARALLAX_RAYMARCHING_SEARCH_STEPS 0
#endif
#if PARALLAX_RAYMARCHING_SEARCH_STEPS > 0
				for (int i = 0; i < PARALLAX_RAYMARCHING_SEARCH_STEPS; i++) {
					uvDelta *= 0.5;
					stepSize *= 0.5;

					if (stepHeight < surfaceHeight) {
						uvOffset += uvDelta;
						stepHeight += stepSize;
					}
					else {
						uvOffset -= uvDelta;
						stepHeight -= stepSize;
					}
					surfaceHeight = GetParallaxHeight(uv + uvOffset);
				}
#elif defined(PARALLAX_RAYMARCHING_INTERPOLATE)
				float prevDifference = prevStepHeight - prevSurfaceHeight;
				float difference = surfaceHeight - stepHeight;
				float t = prevDifference / (prevDifference + difference);
				uvOffset = prevUVOffset - uvDelta * t;
#endif

				return uvOffset;
			}

			float2  ApplyParallax(inout Input i) 
			{

				i.tangentViewDir = normalize(i.tangentViewDir);
#if !defined(PARALLAX_OFFSET_LIMITING)
#if !defined(PARALLAX_BIAS)
#define PARALLAX_BIAS 0.42
#endif
				i.tangentViewDir.xy /= (i.tangentViewDir.z + PARALLAX_BIAS);
#endif

#if !defined(PARALLAX_FUNCTION)
#define PARALLAX_FUNCTION ParallaxOffset
#endif
				float2 uvOffset = PARALLAX_FUNCTION(i.uv_MainTex.xy, i.tangentViewDir.xy);
				//i.uv_MainTex.xy += uvOffset;
				//i.uv_MOS.xy += uvOffset;
//				i.uv_MainTex.zw += uvOffset;// *(_DetailTex_ST.xy / _MainTex_ST.xy);

				//i.uv_MainTex.xy + 0.5f;
				return uvOffset;
			}



            float _Tess;

            float4 tessFixed()
            {
                return _Tess;
            }


            
            void disp (inout appdata v)
            {
                float d = tex2Dlod(_DispTex, float4(v.texcoord.xy * _DispTex_ST.xy + _DispTex_ST.zw,0,0)).r * _Displacement;
                d = d * 0.5 - 0.5 +_DispOffset;
                v.vertex.xyz += v.normal * d;

/*
#if defined(PARALLAX_SUPPORT_SCALED_DYNAMIC_BATCHING)
				v.tangent.xyz = normalize(v.tangent.xyz);
				v.normal = normalize(v.normal);
#endif
				float3x3 objectToTangent = float3x3(
					v.tangent.xyz,
					cross(v.normal, v.tangent.xyz) * v.tangent.w,
					v.normal
					);
				v.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));
*/
            }



            sampler2D _MainTex;
            sampler2D _NormalMap;
            fixed4 _Color;
            float _Metallic;
            float _Glossiness;

            void surf (Input IN, inout SurfaceOutputStandard o) {


//				float2 uvOffset=ApplyParallax(IN);
				float2 uvOffset = 0;

                half4 c = tex2D (_MainTex, IN.uv_MainTex+ uvOffset) * _Color;
                half4 mos = tex2D (_MOS, IN.uv_MOS+ uvOffset);

                o.Albedo = c.rgb;
                o.Metallic = mos.r * _Metallic;
                o.Smoothness = mos.b *_Glossiness;
                o.Occlusion = mos.g;
                o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
            }
            ENDCG
        }
        FallBack "Standard"
    }