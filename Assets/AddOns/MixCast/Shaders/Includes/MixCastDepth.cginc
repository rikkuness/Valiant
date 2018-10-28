/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/

float _CamNear;
float _CamFar;

fixed _PlayerDist;

#ifdef DEPTH_REALSENSE
sampler2D _RsDepthMap;
float _RsDepthMultiplier;
#endif

float GetPixelDistance(float2 uvs)
{
#ifdef DEPTH_REALSENSE
	return tex2D(_RsDepthMap, uvs).x * _RsDepthMultiplier;
#else
	return _PlayerDist;
#endif
}
float GetPixelDistance(float2 uvs, sampler2D map)
{
#ifdef DEPTH_REALSENSE
	return tex2D(map, uvs).x * _RsDepthMultiplier;
#else
	return _PlayerDist;
#endif
}