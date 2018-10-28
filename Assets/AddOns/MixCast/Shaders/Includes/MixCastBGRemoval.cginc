/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/

#include "MixCastDepth.cginc"
#include "MixCastColor.cginc"

#ifdef BG_REMOVAL_CHROMA
float3 _BgChroma_HsvMid;
float3 _BgChroma_HsvRange;
float3 _BgChroma_HsvFeathering;
float _BgChroma_KeyDesaturateBandWidth;
float _BgChroma_KeyDesaturateFalloffWidth;
#endif
#ifdef BG_REMOVAL_STATIC_COLOR
sampler2D _BgStaticColor_CIELab;
float4 _BgStaticColor_Tolerance;
float _BgStaticColor_Range;
#endif
#ifdef BG_REMOVAL_STATIC_DEPTH
sampler2D _BgStaticDepthMask;
float _BgStaticDepth_OutlineSize;
float _BgStaticDepth_OutlineFade;
#endif


#ifdef BG_REMOVAL_CHROMA
//Chroma
float CalculateChromaAlpha(float3 inputHSV, float3 keyHSV, float3 channelLimits, float3 channelFeathers, float3 channelFactors)
{
	float3 dists = float3(abs(inputHSV.x - keyHSV.x), abs(inputHSV.y - keyHSV.y), abs(inputHSV.z - keyHSV.z));
	if (dists.x > 0.5)
		dists.x = 1 - dists.x;

	float hueFactor = 1 - smoothstep(channelLimits.x, channelLimits.x + channelFeathers.x, dists.x);
	float saturationFactor = 1 - smoothstep(channelLimits.y, channelLimits.y + channelFeathers.y, dists.y);
	float valueFactor = 1 - smoothstep(channelLimits.z, channelLimits.z + channelFeathers.z, dists.z);

	float alpha = 1 - lerp(1, hueFactor, channelFactors.x) * lerp(1, saturationFactor, channelFactors.y) * lerp(1, valueFactor, channelFactors.z);
	return alpha;
}
#endif

#ifdef BG_REMOVAL_STATIC_COLOR
float CalculateLABAlpha(float3 inputLAB, float3 keyLAB, float distance)
{
	float newDis = LABDistance(inputLAB, keyLAB);

	float3 labDelta = inputLAB - keyLAB;

	float alphaDis = saturate((newDis - distance - _BgStaticColor_Tolerance.w) / (_BgStaticColor_Tolerance.w*_BgStaticColor_Range));

	float alphaL = saturate((abs(labDelta.x) - _BgStaticColor_Tolerance.x) / (_BgStaticColor_Tolerance.x*_BgStaticColor_Range));
	float alphaA = saturate((abs(labDelta.y) - _BgStaticColor_Tolerance.y) / (_BgStaticColor_Tolerance.y*_BgStaticColor_Range));
	float alphaB = saturate((abs(labDelta.z) - _BgStaticColor_Tolerance.z) / (_BgStaticColor_Tolerance.z*_BgStaticColor_Range));

	return alphaDis * alphaL * alphaA * alphaB;

	/*if (newDis <= distance + _BgStaticColor_Tolerance.w)
		return 0;
	if (abs(labDelta.x) <= _BgStaticColor_Tolerance.x)
		return 0;
	if (abs(labDelta.y) <= _BgStaticColor_Tolerance.y)
		return 0;
	if (abs(labDelta.z) <= _BgStaticColor_Tolerance.z)
		return 0;
	return 1;*/
}
#endif

#if BG_REMOVAL_STATIC_DEPTH
float CalculateDepthConf(float2 inputUvs, float inputDist)
{
#ifdef DEPTH_REALSENSE
	return step(0.0001, inputDist * 0.01);
#else
	return 0;
#endif
}
#endif

#ifdef BG_REMOVAL_CHROMA
float3 ApplyChromaColorModification(float3 inputHSV, float3 keyHSV, float3 channelLimits)
{
	float hueDist = abs(inputHSV.x - keyHSV.x);
	if (hueDist > 0.5)
		hueDist = 1 - hueDist;
	hueDist = max(0.01, hueDist - channelLimits.x);
	hueDist *= 2;
	hueDist = hueDist / (1.0 - channelLimits.x * 2);
	float desat = smoothstep(_BgChroma_KeyDesaturateBandWidth, _BgChroma_KeyDesaturateBandWidth + _BgChroma_KeyDesaturateFalloffWidth, hueDist);
	inputHSV.y *= desat;
	return inputHSV;
}
#endif

void ApplyBackgroundRemoval(float2 inputUvs, float inputDist, inout float4 inputColor)
{
	float alphaResult = 1;
	float cummulativeConf = 0;

	//Incorporate background removal process results

#ifdef BG_REMOVAL_STATIC_DEPTH
	float2 depthMaskResult = tex2D(_BgStaticDepthMask, inputUvs).rg;

	float halfFade = 0.5 * _BgStaticDepth_OutlineFade;

	float minAlpha = halfFade;
	float maxAlpha = 1 - halfFade;

	float midAlpha = lerp(minAlpha, maxAlpha, 1 - _BgStaticDepth_OutlineSize);
	minAlpha = midAlpha - halfFade;
	maxAlpha = midAlpha + halfFade;

	float depthMaskAlpha = smoothstep(minAlpha, maxAlpha, depthMaskResult.r);

	float depthMaskConfidence = min(step( 0.48, abs(0.5 - depthMaskAlpha) ), depthMaskResult.g);

	float staticDepthEffect = saturate(1 - cummulativeConf);
	alphaResult = lerp(alphaResult, depthMaskAlpha, staticDepthEffect);
	cummulativeConf += depthMaskConfidence;

//	inputColor.rgb = cummulativeConf;		//Uncomment these 2 lines to visualize confidence values
//	alphaResult = 1;
#endif

#ifdef BG_REMOVAL_CHROMA
	//Calculate individual method results
	float3 inputHSV = RGB_to_HSV(inputColor.rgb);
	float chromaAlpha = CalculateChromaAlpha(inputHSV, _BgChroma_HsvMid, _BgChroma_HsvRange, _BgChroma_HsvFeathering, float3(1, 1, 1));
	float chromaConf = 1 - chromaAlpha;

	//Determine how much it affects the total result
	float chromaEffect = saturate(1 - cummulativeConf);
	alphaResult = lerp(alphaResult, chromaAlpha, chromaEffect);
	cummulativeConf += chromaConf;
#endif

#ifdef BG_REMOVAL_STATIC_COLOR
	float3 inputLAB = RGB_to_LAB(inputColor.rgb);

	float3 keyCIELAB = tex2D(_BgStaticColor_CIELab, inputUvs).rgb;
	float keyDistance = tex2D(_BgStaticColor_CIELab, inputUvs).a;

	float staticColorAlpha = CalculateLABAlpha(inputLAB, keyCIELAB, keyDistance);
	float staticColorConf = 0;

	float staticColorEffect = saturate(1 - cummulativeConf);
	alphaResult = lerp(alphaResult, staticColorAlpha, staticColorEffect);
	cummulativeConf += staticColorConf;
#endif


	//Apply color modification
#ifdef BG_REMOVAL_CHROMA
	inputHSV = ApplyChromaColorModification(inputHSV, _BgChroma_HsvMid, _BgChroma_HsvRange);
	inputColor.rgb = HSV_to_RGB(inputHSV);
#endif

	inputColor.a = alphaResult;
}