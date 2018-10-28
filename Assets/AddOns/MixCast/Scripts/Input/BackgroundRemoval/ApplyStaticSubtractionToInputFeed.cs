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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class ApplyStaticSubtractionToInputFeed : ApplyRemovalToInputFeed
    {
        private const string SHADER_KEYWORD = "BG_REMOVAL_STATIC_COLOR";

        //public const string CALIB_MIN_MAP_PROP = "_BgStaticColor_MidMap";
        //public const string CALIB_RANGE_MAP_PROP = "_BgStaticColor_RangeMap";
        //public const string FEATHER_PROP = "_BgStaticColor_HsvFeathering";
        public const string CALIB_CIELAB_COLOR_PROP = "_BgStaticColor_CIELab";
        public const string STATIC_COLOR_TOLERANCE = "_BgStaticColor_Tolerance";
        public const string STATIC_COLOR_RANGE = "_BgStaticColor_Range";

        //public Texture midValueTexOverride;
        //public Texture rangeValueTexOverride;
        public Texture cielabColorMapOverride;

        protected override void StartRender(InputFeed feed)
        {
            if (context.Data == null || feed.context.Data != context.Data)
                return;

            Material blitMaterial = feed.ProcessTextureMaterial;
            if( blitMaterial != null && IsPossible())
            {
                blitMaterial.EnableKeyword(SHADER_KEYWORD);

                //Texture midValueTex = (midValueTexOverride != null) ? midValueTexOverride : context.Data.staticSubtractionData.midValueTexture.Tex;
                //blitMaterial.SetTexture(CALIB_MIN_MAP_PROP, midValueTex);
                //Texture rangeValueTex = (rangeValueTexOverride != null) ? rangeValueTexOverride : context.Data.staticSubtractionData.rangeValueTexture.Tex;
                //blitMaterial.SetTexture(CALIB_RANGE_MAP_PROP, rangeValueTex);
                Texture cielabTex = (cielabColorMapOverride != null) ? cielabColorMapOverride : context.Data.staticSubtractionData.cielabTexture.Tex;
                blitMaterial.SetTexture(CALIB_CIELAB_COLOR_PROP, cielabTex);
                //blitMaterial.SetVector(FEATHER_PROP, context.Data.staticSubtractionData.keyHsvFeathering);

                blitMaterial.SetVector(STATIC_COLOR_TOLERANCE, context.Data.staticSubtractionData.keyTolerance);
                blitMaterial.SetFloat(STATIC_COLOR_RANGE, context.Data.staticSubtractionData.keyRange);
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
            return context.Data != null && context.Data.staticSubtractionData.active && context.Data.staticSubtractionData.calibrated;
        }
    }
}
#endif
