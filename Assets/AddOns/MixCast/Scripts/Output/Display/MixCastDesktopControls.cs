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
    public class MixCastDesktopControls : MonoBehaviour
    {
        public void SetToSingle()
        {
            MixCast.Desktop.DisplaySlots = 1;
        }
        public void SetToQuadrant()
        {
            MixCast.Desktop.DisplaySlots = 4;
        }
        public void SetSplit(bool split)
        {
            if (split)
                SetToQuadrant();
            else
                SetToSingle();
        }

        public void SetUiVisible(bool visible)
        {
            MixCast.Desktop.ShowingUI = visible;
        }
        public void ToggleUiVisible()
        {
            MixCast.Desktop.ShowingUI = !MixCast.Desktop.ShowingUI;
        }
        public void SetOutputVisible(bool visible)
        {
            MixCast.Desktop.ShowingOutput = visible;
        }
        public void ToggleOutputVisible()
        {
            MixCast.Desktop.ShowingOutput = !MixCast.Desktop.ShowingOutput;
        }

        public void CycleCameraForward(int displayIndex)
        {
            MixCast.Desktop.CycleCameraForward(displayIndex);
        }
        public void CycleCameraBackward(int displayIndex)
        {
            MixCast.Desktop.CycleCameraBackward(displayIndex);
        }
        public void CycleAllCamerasForward()
        {
            for (int i = 0; i < MixCast.Desktop.DisplaySlots; i++)
                MixCast.Desktop.CycleCameraForward(i);
        }
        public void CycleAllCamerasBackward()
        {
            for (int i = 0; i < MixCast.Desktop.DisplaySlots; i++)
                MixCast.Desktop.CycleCameraBackward(i);
        }
    }
}
#endif
