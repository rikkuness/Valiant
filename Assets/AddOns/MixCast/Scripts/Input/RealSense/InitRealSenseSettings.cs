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
using Intel.RealSense;

namespace BlueprintReality.MixCast.RealSense
{
    public class InitRealSenseSettings : MonoBehaviour
    {
        [SerializeField] CameraConfigContext context;

        public RealSenseInputFeed feed { private get; set; }

        void OnEnable()
        {
            if (context == null)
            {
                context = GetComponentInParent<CameraConfigContext>();
            }

            StartCoroutine(DelayedInit());
        }

        IEnumerator DelayedInit()
        {
            yield return new WaitForEndOfFrame();

            if (context == null)
            {
                yield break;
            }

            RealSenseUtility.SetOption(feed.colorSensor, Option.Exposure, context.Data.deviceData.exposure);
            RealSenseUtility.SetOption(feed.colorSensor, Option.Gain, context.Data.deviceData.gain);
            RealSenseUtility.SetOption(feed.colorSensor, Option.WhiteBalance, context.Data.deviceData.whiteBalance);
            RealSenseUtility.SetOption(feed.depthSensor, Option.Exposure, context.Data.deviceData.infraredExposure);
        }
    }
}
#endif
