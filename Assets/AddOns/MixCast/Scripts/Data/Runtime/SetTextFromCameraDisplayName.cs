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
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
	public class SetTextFromCameraDisplayName : CameraComponent
    {
        public UnityEngine.UI.Text text;
        public bool applyEveryFrame = false;

        private string lastCustomName;
        private string lastCamDevice;

        protected override void OnEnable()
        {
            base.OnEnable();
            ApplyName(context.Data);
        }
        protected override void HandleDataChanged()
        {
            base.HandleDataChanged();
            ApplyName(context.Data);
        }
        private void Update()
        {
            if (context.Data == null)
                return;

            if (applyEveryFrame || lastCustomName != context.Data.displayName || lastCamDevice != context.Data.deviceName)
                ApplyName(context.Data);
        }

        void ApplyName(MixCastData.CameraCalibrationData data)
        {
            if (data != null)
            {
                text.text = MixCastDataUtility.CalculateCameraName(data);

                lastCustomName = data.displayName;
                lastCamDevice = data.deviceName;
            }
            else
            {
                text.text = "";

                lastCustomName = null;
                lastCamDevice = null;
            }
        }
    }
}
#endif
