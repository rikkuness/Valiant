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

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif


namespace BlueprintReality.MixCast.Profiling
{
    public class GameFPSTracker : FPSStatsTracker
    {
        public const string KeyName = "Game";

        public override string Key { get { return KeyName; } }

        void Start()
        {
#if UNITY_2017_2_OR_NEWER
            Target = Mathf.RoundToInt(XRDevice.refreshRate);
#else
            Target = Mathf.RoundToInt(VRDevice.refreshRate);
#endif
        }

        void Update()
        {
            Tick();
        }
    }
}
#endif
