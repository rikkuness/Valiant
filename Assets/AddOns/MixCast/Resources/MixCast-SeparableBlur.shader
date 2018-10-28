// Separable bilateral blur using depth and RGB data

Shader "Hidden/MixCast/Separable Blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ImageTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ BLUR_HORIZONTAL BLUR_VERTICAL
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _ImageTex;
			float2 _MainTex_TexelSize;

			float4 noise(float2 p) {
				return float4(frac(sin(dot(p, float2(_Time.x * 34.21, _Time.y * 12.83))) * 8421.123), 0, 0, 0);
			}

			v2f vert (appdata_full v)
			{
				v2f o;
				o.uv = v.texcoord;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
#ifdef BLUR_HORIZONTAL
				float2 delta = float2(_MainTex_TexelSize.x * 2.0, 0);
#else
				float2 delta = float2(0, _MainTex_TexelSize.y * 2.0);
#endif

				// Depth samples
				float4 d1 = tex2D(_MainTex, i.uv - delta * 3.2307692308);
				float4 d2 = tex2D(_MainTex, i.uv - delta * 2);
				float4 d3 = tex2D(_MainTex, i.uv - delta);
				float4 d4 = tex2D(_MainTex, i.uv);
				float4 d5 = tex2D(_MainTex, i.uv + delta);
				float4 d6 = tex2D(_MainTex, i.uv + delta * 2);
				float4 d7 = tex2D(_MainTex, i.uv + delta * 3.2307692308);

				// Colour samples
				float3 c1 = tex2D(_ImageTex, i.uv - delta * 3.2307692308).rgb;
				float3 c2 = tex2D(_ImageTex, i.uv - delta * 2).rgb;
				float3 c3 = tex2D(_ImageTex, i.uv - delta).rgb;
				float3 c4 = tex2D(_ImageTex, i.uv).rgb;
				float3 c5 = tex2D(_ImageTex, i.uv + delta).rgb;
				float3 c6 = tex2D(_ImageTex, i.uv + delta * 2).rgb;
				float3 c7 = tex2D(_ImageTex, i.uv + delta * 3.2307692308).rgb;

				// Gaussian equation coefficient for the range filter
				const float gaussCoeff = 0.7978845608;

				// Range filter depends on the distance between samples in colour space
				float g1 = length(c4 - c1);
				float g2 = length(c4 - c2);
				float g3 = length(c4 - c3);
				float g4 = gaussCoeff;
				float g5 = length(c4 - c5);
				float g6 = length(c4 - c6);
				float g7 = length(c4 - c7);

				// Compute gaussian function values for range filter
				g1 = gaussCoeff * exp(-0.5 * (g1 / 0.5) * (g1 / 0.5));
				g2 = gaussCoeff * exp(-0.5 * (g2 / 0.5) * (g2 / 0.5));
				g3 = gaussCoeff * exp(-0.5 * (g3 / 0.5) * (g3 / 0.5));
				g5 = gaussCoeff * exp(-0.5 * (g5 / 0.5) * (g5 / 0.5));
				g6 = gaussCoeff * exp(-0.5 * (g6 / 0.5) * (g6 / 0.5));
				g7 = gaussCoeff * exp(-0.5 * (g7 / 0.5) * (g7 / 0.5));

				// Multiply coefficients to compose the spatial and range filter convolutions
				float coeff1 = 0.11453744493 * g1;
				float coeff2 = 0.19823788546 * g2;
				float coeff3 = 0.31718061674 * g3;
				float coeff4 = 0.37004405286 * g4;
				float coeff5 = 0.31718061674 * g5;
				float coeff6 = 0.19823788546 * g6;
				float coeff7 = 0.11453744493 * g7;

				// Normalize by the sum of all coefficients so the filter is energy-preserving
				float totalCoeff = coeff1 + coeff2 + coeff3 + coeff4 + coeff5 + coeff6 + coeff7;

				float4 col = (d1 * coeff1 + d2 * coeff2 + d3 * coeff3 + d4 * coeff4 + d5 * coeff5 + d6 * coeff6 + d7 * coeff7) / totalCoeff;

				return col;
			}
			ENDCG
		}
	}
}
