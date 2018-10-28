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
    public class MixCastActivator : MonoBehaviour
    {
        const float REFRESH_RATE = 0.5f;    //2x per sec

        private void OnEnable()
        {
            InvokeRepeating("RefreshStatus", 0.0001f, REFRESH_RATE);
        }
        private void OnDisable()
        {
            CancelInvoke("RefreshStatus");
        }

        void RefreshStatus()
        {
            bool isDataConfigured = MixCast.Settings.cameras.Count > 0;
            bool isServiceRunning = MixCastRegistry.IsServiceRunning();

            bool shouldMixCastBeActive = isDataConfigured && isServiceRunning;

            if (shouldMixCastBeActive && !MixCast.Active)
                MixCast.SetActive(true);
            else if (!shouldMixCastBeActive && MixCast.Active)
                MixCast.SetActive(false);
        }
    }
}
#endif
