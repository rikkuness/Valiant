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
using BlueprintReality.MixCast;

namespace BlueprintReality.MixCast.Profiling
{
    public class MixCastRenderFPSTracker : FPSStatsTracker
    {
        public const string KeyName = "MixCast Render";

        public override string Key { get { return KeyName; } }

        public override void StartTracking()
        {
            base.StartTracking();

            if (MixCastCameras.Instance != null)
            {
                MixCastCameras.Instance.OnBeforeRender += Tick;
            }
        }

        public override void StopTracking()
        {
            base.StopTracking();

            if (MixCastCameras.Instance != null)
            {
                MixCastCameras.Instance.OnBeforeRender -= Tick;
            }
        }

        void Update()
        {
            Target = MixCast.Settings.global.targetFramerate;
        }
        
        void OnDestroy()
        {
            if (MixCastCameras.Instance != null)
            {
                MixCastCameras.Instance.OnBeforeRender -= Tick;
            }
        }
    }
}
#endif
