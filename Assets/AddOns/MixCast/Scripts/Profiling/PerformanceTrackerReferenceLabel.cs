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
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintReality.MixCast.Profiling
{
    /// <summary>
    /// Label for Peformance Stats when direct reference to StatsTracker component can be make in Editor inspector.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class PerformanceTrackerReferenceLabel : MonoBehaviour
    {
        public StatsTracker[] trackers;
        public string format = "{0}: {1} ({2}/{3}/{4})";
        public float updateInterval = 0.1f;

        private float nextUpdateTime;
        private UnityEngine.UI.Text label;
        private StringBuilder sb = new StringBuilder();

        void Awake()
        {
            label = GetComponent<UnityEngine.UI.Text>();
            nextUpdateTime = Time.unscaledTime + nextUpdateTime;
        }

        void Update()
        {
            if (Time.unscaledTime >= nextUpdateTime)
            {
                nextUpdateTime += updateInterval;

                UpdateLabel();
            }
        }

        private void UpdateLabel()
        {
            sb.Length = 0;

            foreach (var tracker in trackers)
            {
                if (tracker.IsReady)
                {
                    sb.AppendFormat(format, tracker.Key, tracker.Current, tracker.Lowest, tracker.Highest, tracker.Avg, tracker.Target);
                }
                else
                {
                    sb.AppendFormat(format, tracker.Key, "-", "-", "-", "-", tracker.Target);
                }

                sb.AppendLine();
            }

            label.text = sb.ToString();
        }
    }
}
#endif
