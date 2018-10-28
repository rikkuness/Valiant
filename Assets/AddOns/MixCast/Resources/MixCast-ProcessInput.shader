/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
Shader "Hidden/MixCast/Process Input" {

	Properties{
		_MainTex("Base(RGB)", 2D) = "white"{}

		_KeyingFactor("Keying Factor", Range(0,1)) = 1
	}

		SubShader{
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "Opaque" }

		Lighting Off
		Blend One Zero

		Pass{
		CGPROGRAM

#pragma target 3.0
#pragma exclude_renderers d3d9

#pragma multi_compile PXL_FMT_RGB PXL_FMT_BGR
#pragma multi_compile __ PXL_FLIP_X
#pragma multi_compile __ PXL_FLIP_Y
#pragma multi_compile __ DEPTH_REALSENSE
#pragma multi_compile __ UNITY_COLORSPACE_GAMMA

#pragma multi_compile __ BG_REMOVAL_CHROMA
#pragma multi_compile __ BG_REMOVAL_STATIC_COLOR
#pragma multi_compile __ BG_REMOVAL_STATIC_DEPTH

#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
#include "./../Shaders/Includes/BPR_ShaderHelpers.cginc"
#include "./../Shaders/Includes/MixCastBGRemoval.cginc"

	sampler2D _MainTex;


	float4x4 _CamProjection;
	float4x4 _WorldToCam;
	float4x4 _CamToWorld;

	float _KeyingFactor;

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
#ifdef PXL_FLIP_X
		uvs.x = 1 - uvs.x;
#endif
#ifdef PXL_FLIP_Y
		uvs.y = 1 - uvs.y;
#endif

		float playerDist = GetPixelDistance(uvs);
		float playerDepth = (playerDist - _CamNear) / (_CamFar - _CamNear);    //Calculate frustum-relative distance

		float3 worldPos = CalculateWorldPosition(float3(i.uv.x, i.uv.y, playerDepth), _CamNear, _CamFar, _CamProjection, _CamToWorld);

		o.col.rgb = tex2D(_MainTex, uvs).rgb;
#ifdef PXL_FMT_BGR
		o.col.rgb = o.col.bgr;
#endif
		o.col.a = 1;

		ApplyBackgroundRemoval(uvs, playerDist, o.col);

		o.col.a = lerp(1, o.col.a, _KeyingFactor);
		clip(o.col.a - 0.05);

		return o;
	}


	ENDCG
	}
	}
}