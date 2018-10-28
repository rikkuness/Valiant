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
using BlueprintReality.MixCast.RealSense;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class ApplyStaticDepthToInputFeed : ApplyRemovalToInputFeed
    {
        private const string SHADER_KEYWORD = "BG_REMOVAL_STATIC_DEPTH";

        public const string CALIB_MAP_PROP = "_BgStaticDepth_CalibMap";
        public const string CONFIDENCE_MAP_PROP = "_BgStaticDepth_ConfidenceMap";
        public const string MIN_CONFIDENCE_PROP = "_BgStaticDepth_MinConf";
        public const string MIN_OFFSET_PROP = "_BgStaticDepth_MinOffset";

        public const string DEPTH_OUTLINE_SIZE_PROP = "_BgStaticDepth_OutlineSize";
        public const string DEPTH_OUTLINE_FADE_PROP = "_BgStaticDepth_OutlineFade";

        public const float MAX_FADE_RANGE = 0.8f;

        [Range(0,1)]
        public float minConf = 0.5f;

        public float minOffset = 0.01f;

        public Texture depthValueTexOverride;

        protected override void OnEnable()
        {
            base.OnEnable();

            InputFeed.OnBeforeProcessTexture += StartProcess;
            InputFeed.OnAfterProcessTexture += StopProcess;
        }
        protected override void OnDisable()
        {
            InputFeed.OnBeforeProcessTexture -= StartProcess;
            InputFeed.OnAfterProcessTexture -= StopProcess;
            base.OnDisable();
        }

        protected void StartProcess(InputFeed feed)
        {
            if (context.Data == null || feed.context.Data != context.Data)
                return;

            RealSenseInputFeed realSenseFeed = feed as RealSenseInputFeed;
            if (realSenseFeed == null)
            {
                return;
            }

            Material blitMaterial = realSenseFeed.DepthMaskMaterial;

            if ( blitMaterial != null && IsPossible())
            {
                Texture depthValueTex = depthValueTexOverride != null ? depthValueTexOverride : context.Data.staticDepthData.depthValueTexture.Tex;
                blitMaterial.SetTexture(CALIB_MAP_PROP, depthValueTex);

                blitMaterial.SetFloat(MIN_CONFIDENCE_PROP, minConf);
                blitMaterial.SetFloat(MIN_OFFSET_PROP, minOffset);
            }
        }

        protected void StopProcess(InputFeed feed)
        {
        }

        protected override void StartRender(InputFeed feed)
        {
            if (context.Data == null || feed.context.Data != context.Data)
                return;

            Material blitMaterial = feed.ProcessTextureMaterial;

            if (blitMaterial != null && IsPossible())
            {
                blitMaterial.EnableKeyword(SHADER_KEYWORD);
                blitMaterial.SetFloat(DEPTH_OUTLINE_SIZE_PROP, context.Data.staticDepthData.outlineSize);
                blitMaterial.SetFloat(DEPTH_OUTLINE_FADE_PROP, MAX_FADE_RANGE * Mathf.Max(0.01f, context.Data.staticDepthData.outlineFade));
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
            return context.Data != null && context.Data.staticDepthData.active && context.Data.staticDepthData.calibrated;
        }
    }
}
#endif
