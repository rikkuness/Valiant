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
    //This component exposes controller functions for modifying the actively displaying MixCast camera
	public class DisplayingCameraControls : MonoBehaviour
    {
        public DisplaySlotContext slotContext;
        public int SlotIndex { get
            {
                if (slotContext != null)
                    return slotContext.Index;
                else
                    return 0;
            }
        }

        private void OnEnable()
        {
            if (slotContext == null)
                slotContext = GetComponentInParent<DisplaySlotContext>();
        }

        public void SetToConfig(CameraConfigContext context)
        {
            MixCast.Desktop.displayingCameras[SlotIndex] = context.Data;
        }
        public void ClearDisplayed()
        {
            MixCast.Desktop.DisplaySlots = 0;
        }

        public void CycleCameraForward()
        {
            MixCast.Desktop.CycleCameraForward(SlotIndex);
        }
        public void CycleCameraBackward()
        {
            MixCast.Desktop.CycleCameraBackward(SlotIndex);
        }

        public void SetToFirstAvailable()
        {
            if (MixCast.Settings.cameras.Count == 0)
            {
                return;
            }

            MixCast.Desktop.displayingCameras[SlotIndex] = MixCast.Settings.cameras[0];
        }

        public void SetToLastAvailable()
        {
            if (MixCast.Settings.cameras.Count == 0)
            {
                return;
            }

            MixCast.Desktop.displayingCameras[SlotIndex] = MixCast.Settings.cameras[MixCast.Settings.cameras.Count - 1];
        }
    }
}
#endif
