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
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCastDesktop
    {
        private int displaySlots = 1;
        public int DisplaySlots
        {
            get
            {
                return displaySlots;
            }
            set
            {
                displaySlots = Mathf.Max(0, value);
                AutoAddCameras(displaySlots);
                AutoRemoveCameras(displaySlots);
            }
        }
        public List<MixCastData.CameraCalibrationData> displayingCameras = new List<MixCastData.CameraCalibrationData>();

        public bool ShowingUI
        {
            get; set;
        }
        public bool ShowingOutput
        {
            get; set;
        }

        public MixCastDesktop()
        {
            MixCast.MixCastEnabled += MixCastEnabled;
            MixCast.MixCastDisabled += MixCastDisabled;
        }
        ~MixCastDesktop()
        {
            MixCast.MixCastEnabled -= MixCastEnabled;
            MixCast.MixCastDisabled -= MixCastDisabled;
        }

        private void MixCastEnabled()
        {
            ShowingOutput = true;
            ShowingUI = true;
            AutoAddCameras(DisplaySlots);
        }

        private void MixCastDisabled()
        {
            AutoRemoveCameras(0);
        }

        public bool CameraInUse(MixCastData.CameraCalibrationData cam)
        {
            return ShowingOutput && displayingCameras.Contains(cam);
        }

        void AutoAddCameras(int targetCount)
        {
            var cameras = MixCast.Settings.cameras;
            if (cameras.Count <= 0)
            {
                return;
            }

            while (displayingCameras.Count < targetCount)
            {
                var nextIndex = 0;

                if (displayingCameras.Count > 0)
                {
                    var lastAddedCamera = displayingCameras[displayingCameras.Count - 1];
                    var cameraIndex = cameras.IndexOf(lastAddedCamera);
                    nextIndex = (cameraIndex + 1) % cameras.Count;
                }

                var camera = cameras[nextIndex];
                displayingCameras.Add(camera);
            }
        }
        void AutoRemoveCameras(int targetCount)
        {
            while (displayingCameras.Count > targetCount)
                displayingCameras.RemoveAt(displayingCameras.Count - 1);
        }


        public void DestroyedCamera(MixCastData.CameraCalibrationData camera)
        {
            for( int i = 0; i < displayingCameras.Count; i++ )
            {
                if (displayingCameras[i] != camera)
                    continue;

                int index = MixCast.Settings.cameras.IndexOf(camera);
                if (index > 0)
                    displayingCameras[i] = MixCast.Settings.cameras[index - 1];
                else if (index < MixCast.Settings.cameras.Count - 1)
                    displayingCameras[i] = MixCast.Settings.cameras[index + 1];
                else
                    displayingCameras[i] = null;
            }
        }

        public void CycleCameraForward(int displaySlot = 0)
        {
            if (MixCast.Settings.cameras.Count == 0 || displaySlot >= DisplaySlots)
                return;

            List<MixCastData.CameraCalibrationData> cameraList = MixCast.Settings.cameras;

            int curIndex = cameraList.IndexOf(displayingCameras[displaySlot]);
            if (curIndex == -1)
            {
                displayingCameras[displaySlot] = cameraList[0];
                return;
            }

            int camIndex = cameraList.IndexOf(displayingCameras[displaySlot]);

            camIndex++;
            if (camIndex >= cameraList.Count)
                camIndex = 0;

            displayingCameras[displaySlot] = cameraList[camIndex];
        }
        public void CycleCameraBackward(int displaySlot = 0)
        {
            if (MixCast.Settings.cameras.Count == 0 || displaySlot >= DisplaySlots)
                return;

            List<MixCastData.CameraCalibrationData> cameraList = MixCast.Settings.cameras;

            int curIndex = cameraList.IndexOf(displayingCameras[displaySlot]);
            if (curIndex == -1)
            {
                displayingCameras[displaySlot] = cameraList[0];
                return;
            }

            int camIndex = cameraList.IndexOf(displayingCameras[displaySlot]);

            camIndex--;
            if (camIndex < 0)
                camIndex = cameraList.Count - 1;

            displayingCameras[displaySlot] = cameraList[camIndex];
        }
    }
}
#endif
