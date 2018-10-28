/**********************************************************************************
* Blueprint Reality Inc. CONFIDENTIAL
* 2018 Blueprint Reality Inc.
* All Rights Reserved.
*
* NOTICE:  All information contained herein is, and remains, the property of
* Blueprint Reality Inc. and its suppliers, if any.  The intellectual and
* technical concepts contained herein are proprietary to Blueprint Reality Inc.
* and its suppliers and may be covered by Patents, pending patents, and are
* protected by trade secret or copyright law.
*
* Dissemination of this information or reproduction of this material is strictly
* forbidden unless prior written permission is obtained from Blueprint Reality Inc.
***********************************************************************************/

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UnityEngine;
using UnityEngine.Rendering;

namespace BlueprintReality.MixCast
{
    public class ApplyChromakeyToInputFeed : ApplyRemovalToInputFeed
    {
        private const string SHADER_KEYWORD = "BG_REMOVAL_CHROMA";

        public const string CHROMA_MID_PROP = "_BgChroma_HsvMid";
        public const string CHROMA_RANGE_PROP = "_BgChroma_HsvRange";
        public const string CHROMA_FEATHER_PROP = "_BgChroma_HsvFeathering";
        public const string CHROMA_DESAT_BAND_WIDTH_PARAM = "_BgChroma_KeyDesaturateBandWidth";
        public const string CHROMA_DESAT_FALLOFF_WIDTH_PARAM = "_BgChroma_KeyDesaturateFalloffWidth";

        protected override void StartRender(InputFeed feed)
        {
            if (context.Data == null || feed.context.Data != context.Data)
                return;

            Material blitMaterial = feed.ProcessTextureMaterial;
            if (blitMaterial != null && IsPossible())
            {
                blitMaterial.EnableKeyword(SHADER_KEYWORD);

                blitMaterial.SetVector(CHROMA_MID_PROP, context.Data.chromakeying.keyHsvMid);
                blitMaterial.SetVector(CHROMA_RANGE_PROP, context.Data.chromakeying.keyHsvRange);
                blitMaterial.SetVector(CHROMA_FEATHER_PROP, context.Data.chromakeying.keyHsvFeathering);
                blitMaterial.SetFloat(CHROMA_DESAT_BAND_WIDTH_PARAM, context.Data.chromakeying.keyDesaturationBandWidth);
                blitMaterial.SetFloat(CHROMA_DESAT_FALLOFF_WIDTH_PARAM, context.Data.chromakeying.keyDesaturationFalloffWidth);
            }
        }
        protected override void StopRender(InputFeed feed)
        {
            if (context.Data == null || feed.context.Data != context.Data)
                return;

            Material blitMaterial = feed.ProcessTextureMaterial;
            if (blitMaterial != null && IsPossible())
            {
                blitMaterial.DisableKeyword(SHADER_KEYWORD);
            }
        }

        protected override bool IsPossible()
        {
            return context.Data != null && context.Data.chromakeying.active && context.Data.chromakeying.calibrated;
        }
    }
}
#endif
