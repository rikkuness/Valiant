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

namespace BlueprintReality.MixCast
{
    public class OutputMixCastToTimelapse : OutputMixCastToScreenshotAuto
    {
        private const float ENABLE_COOLDOWN = 1f;

        private float captureCooldown;

        protected override void OnEnable()
        {
            base.OnEnable();

            captureCooldown = ENABLE_COOLDOWN;
        }

        private void Update()
        {
            if (context.Data == null || !MixCast.TimelapseCameras.Contains(context.Data))
                return;

            if( captureCooldown > 0 )
            {
                if (captureCooldown > context.Data.recordingData.timelapseInterval && captureCooldown > ENABLE_COOLDOWN)
                    captureCooldown = context.Data.recordingData.timelapseInterval;

                captureCooldown -= Time.unscaledDeltaTime;
                if( captureCooldown <= 0 )
                {
                    Run();
                    captureCooldown = Mathf.Max(context.Data.recordingData.timelapseInterval, ENABLE_COOLDOWN);
                }
            }
        }

        protected override void HandleDataChanged()
        {
            if (context.Data != null || !MixCast.TimelapseCameras.Contains(context.Data))
                captureCooldown = context.Data.recordingData.timelapseInterval;

            base.HandleDataChanged();
        }
    }
}
#endif
