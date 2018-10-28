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
using UnityEngine;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif

namespace BlueprintReality.MixCast
{
    public class SensorMissingIndicator : MonoBehaviour
    {
        private void Update()
        {
#if UNITY_2017_2_OR_NEWER
            if (XRDevice.isPresent)
#else
            if (VRDevice.isPresent)
#endif
            {
                if (VRInfo.IsVRModelOculus())
                {
                    bool sensor1 = TrackedDeviceManager.Instance.IsSensorAvailable(0);
                    bool sensor2 = TrackedDeviceManager.Instance.IsSensorAvailable(1);
                    if (!sensor1 || !sensor2)
                    {
                        var popUpWindow = GetComponent<GameObjects.OpenPopupWindow>();

                        if (!popUpWindow)
                        {
                            return;
                        }

                        popUpWindow.Open();
                    }
                }
                Destroy(this.gameObject);
            }
        }
    }
}
#endif
