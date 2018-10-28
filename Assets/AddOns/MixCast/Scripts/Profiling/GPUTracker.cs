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
using UnityEngine;

namespace BlueprintReality.MixCast.Profiling
{
    public class GPUTracker : StatsTracker
    {
        public const string KeyName = "GPU";

        public override string Key { get { return KeyName; } }

        public override void StartTracking()
        {
            base.StartTracking();
        }

        public override void StopTracking()
        {
            base.StopTracking();
        }

        public void LateUpdate()
        {
            if (IsTracking)
            {
                int ms = GetGPUMs();
                
                SetCurrent(ms);
            }
        }

        private int GetGPUMs()
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
            {
                return GetGPUMsSteamVR();
            }
#elif MIXCAST_OCULUS
            if (VRInfo.IsDeviceOculus())
            {
                return GetGPUMsOculus();
            }
#endif
            
            return 0;
        }

#if MIXCAST_OCULUS
        private int GetGPUMsOculus()
        {
            var stats = OVRPlugin.GetAppPerfStats();

            if (stats.FrameStatsCount > 0)
            {
                // To get Total GPU, maybe we have to sum AppGpuElapsedTime with CompositorGpuElapsedTime.
                return Mathf.RoundToInt(stats.FrameStats[stats.FrameStatsCount - 1].AppGpuElapsedTime);
            }

            return 0;
        }
#endif

#if MIXCAST_STEAMVR
        private int GetGPUMsSteamVR()
        {
            var compositor = Valve.VR.OpenVR.Compositor;
            if (compositor != null)
            {
                var timing = new Valve.VR.Compositor_FrameTiming();
                timing.m_nSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.Compositor_FrameTiming));

                compositor.GetFrameTiming(ref timing, 0);

                return Mathf.RoundToInt(timing.m_flTotalRenderGpuMs);
            }

            return 0;
        }
#endif
    }
}
#endif
