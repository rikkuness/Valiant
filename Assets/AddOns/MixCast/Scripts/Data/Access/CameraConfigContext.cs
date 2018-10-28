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
    public class CameraConfigContext : MonoBehaviour
    {
        private MixCastData.CameraCalibrationData data;

        public MixCastData.CameraCalibrationData Data
        {
            get
            {
                return data;
            }
            set
            {
                if (data == value)
                    return;

                data = value;
                
                if (DataChanged != null)
                    DataChanged();
            }
        }
        public event System.Action DataChanged;
    }
}
#endif
