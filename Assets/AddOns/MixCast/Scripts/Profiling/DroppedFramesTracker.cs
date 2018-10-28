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

namespace BlueprintReality.MixCast.Profiling
{
    public class DroppedFramesTracker : StatsTracker
    {
        public const string KeyName = "Dropped Frames";

        public override string Key { get { return KeyName; } }
        public int FrameWindow = 3;
        private int frameWindowCounter;
        private int droppedFramesThisWindow;
        
        void LateUpdate()
        {
            if (!IsTracking)
            {
                return;
            }

            int dropped = GetDroppedFrames();
            
            droppedFramesThisWindow += dropped;

            ++frameWindowCounter;

            if (frameWindowCounter >= FrameWindow)
            {
                frameWindowCounter = 0;

                SetCurrent(droppedFramesThisWindow);

                droppedFramesThisWindow = 0;
            }
        }

        private int GetDroppedFrames()
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
            {
                return GetDroppedFramesSteamVR();
            }
#elif MIXCAST_OCULUS
            if (VRInfo.IsDeviceOculus())
            {
                return GetDroppedFramesOculus();
            }
#endif

            return 0;
        }

#if MIXCAST_OCULUS
        private int GetDroppedFramesOculus()
        {
            var stats = OVRPlugin.GetAppPerfStats();

            if (stats.FrameStatsCount > 0)
            {
                return stats.FrameStats[stats.FrameStatsCount - 1].AppDroppedFrameCount;
            }

            return 0;
        }
#endif

#if MIXCAST_STEAMVR
        private int GetDroppedFramesSteamVR()
        {
            var compositor = Valve.VR.OpenVR.Compositor;
            if (compositor != null)
            {
                var timing = new Valve.VR.Compositor_FrameTiming();
                timing.m_nSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.Compositor_FrameTiming));

                compositor.GetFrameTiming(ref timing, 0);

                return (int)timing.m_nNumDroppedFrames;
            }

            return 0;
        }
#endif
    }
}
#endif
