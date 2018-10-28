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

namespace BlueprintReality.MixCast
{
	public class SetCameraConfigFromDisplayingCamera : CameraComponent
    {
        [Tooltip("When MixCastDesktop is configured for multiple displaying cameras, this points to which slot to access")]
        public DisplaySlotContext slotContext;

        protected override void OnEnable()
        {
            if (slotContext == null)
                slotContext = GetComponentInParent<DisplaySlotContext>();
            base.OnEnable();
            Update();
        }

        private void Update()
        {
            int displaySlotIndex = 0;
            if (slotContext != null)
                displaySlotIndex = slotContext.Index;

            if (displaySlotIndex < MixCast.Desktop.DisplaySlots && displaySlotIndex < MixCast.Desktop.displayingCameras.Count)
                context.Data = MixCast.Desktop.displayingCameras[displaySlotIndex];
            else
                context.Data = null;
        }
    }
}
#endif
