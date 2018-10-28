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
using UnityEngine;
using System.Diagnostics;

namespace BlueprintReality.MixCast.Profiling
{
    /// <summary>
    /// Test class to check if FPSStatsTracker calculates FPS data correctly.
    /// </summary>
    [RequireComponent(typeof(FPSStatsTracker))]
    public class FPSTrackerTester : MonoBehaviour
    {
        public int targetFPS = 60;
        public int measuredFPS = 0;
        public int avgFPS = 0;
        public int internalFPS = 0;

        private int internalFPSCounter;
        private float nextTickTime;
        private FPSStatsTracker tracker;
        private Stopwatch stopwatch;

        void Awake()
        {
            tracker = GetComponent<FPSStatsTracker>();

            stopwatch = new Stopwatch();
        }

        void Start()
        {
            stopwatch.Start();
            
            nextTickTime = Time.unscaledTime + 1f / targetFPS;

            tracker.StartTracking();
        }
        
        private void Tick()
        {
            ++internalFPSCounter;

            tracker.Tick();
        }

        void Update()
        {
            measuredFPS = tracker.Current;
            avgFPS = tracker.Avg;
        }

        void LateUpdate()
        {
            if (stopwatch.ElapsedMilliseconds >= 1000)
            {
                internalFPS = internalFPSCounter;
                internalFPSCounter = 0;

                stopwatch.Reset();
                stopwatch.Start();
            }

            if (Time.unscaledTime >= nextTickTime)
            {
                Tick();

                nextTickTime += 1f / targetFPS;
            }
        }
    }
}
#endif
