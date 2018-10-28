Shader "Hidden/MixCast/Depth Mask" {
	Properties{
		_MainTex("Base(RGB)", 2D) = "white"{}
	}

		SubShader{
			Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "Opaque" }

		//ColorMask RGB
		Lighting Off
		//AlphaTest Off
		Blend One Zero
		// No Ztest
		ZTest Always Cull Off ZWrite Off

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#pragma target 3.0

#pragma multi_compile __ PXL_FLIP_Y
#pragma multi_compile __ DEPTH_ZED DEPTH_REALSENSE

#include "UnityCG.cginc"
#include "./../Shaders/Includes/MixCastDepth.cginc"

		sampler2D _MainTex;
		sampler2D _BgStaticDepth_CalibMap;
#ifdef DEPTH_ZED
		sampler2D _BgStaticDepth_ConfidenceMap;
#endif
		float _BgStaticDepth_MinOffset;
		float _BgStaticDepth_MaxDepth;

		float CalculateDepthConf(float2 inputUvs, float inputDist)
		{
#if DEPTH_ZED
			return 1 - 0.01 * tex2D(_BgStaticDepth_ConfidenceMap, inputUvs).r;
#elif DEPTH_REALSENSE
			return step(0.0001, inputDist * 0.01);
#else
			return 0;
#endif
		}

		void MaskDepth(float2 inputUvs, float inputDist, out float4 outColor)
		{
			// Remove pixels further than the maximum depth
			float alphaResult = 0;

			float2 depthUvs = inputUvs;
			float3 storedDepthData = tex2D(_BgStaticDepth_CalibMap, depthUvs).rgb;
			float storedDepthSample = storedDepthData.r;
			float storedDistSample = storedDepthSample * 10;
			float storedConf = storedDepthData.g;

			storedDistSample = lerp(10, storedDistSample, storedConf);	//send to back if no good stored data

			float deltaDistance = inputDist - storedDistSample;

			float staticDepthScore = step(_BgStaticDepth_MinOffset, -deltaDistance);	//negative distance means pixel closer than background
			float staticDepthConf = CalculateDepthConf(inputUvs, inputDist);

			alphaResult = staticDepthScore * staticDepthConf;

			outColor.r = lerp(alphaResult, 0.0, step(_BgStaticDepth_MaxDepth, inputDist));
			outColor.g = staticDepthConf;
			outColor.b = 0;
			outColor.a = 1;
		}


		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 pos : SV_POSITION;
			float3 worldNormal : TEXCOORD2;
		};


		v2f vert(appdata_full v)
		{
			v2f o;

			o.uv = v.texcoord;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldNormal = -UNITY_MATRIX_IT_MV[2].xyz;
			return o;
		}

		struct frag_output {
			float4 col:COLOR;
		};

		frag_output frag(v2f i) {
			frag_output o;

			float2 uvs = i.uv;
#ifdef PXL_FLIP_Y
			uvs.y = 1 - uvs.y;
#endif

			float playerDist = GetPixelDistance(uvs, _MainTex);
			MaskDepth(uvs, playerDist, o.col);

			return o;
		}


		ENDCG
	}
	}
}