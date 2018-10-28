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
using System;
using System.Linq;

namespace BlueprintReality.MixCast
{
    public static class MixCastDataUtility
    {
        public static void UpdateForBackwardCompatibility(MixCastData data, string dataStr)
        {
            AddMissingCameraInfo(data);
        }

        /// <summary>
        /// Fills in camera device fields that were added in new versions of MixCast.
        /// </summary>
        static void AddMissingCameraInfo(MixCastData data)
        {
            foreach (var camera in data.cameras)
            {
                var device = FeedDeviceManager.FindDeviceFromName(camera.deviceName);

                if (device == null)
                {
                    continue;
                }

                AddDeviceInfo(camera, device);
            }
        }

        /// <summary>
        /// Adds device info to the camera data.
        /// </summary>
        static void AddDeviceInfo(MixCastData.CameraCalibrationData camera, FeedDeviceManager.DeviceInfo device)
        {
            if (string.IsNullOrEmpty(camera.deviceAltName))
            {
                camera.deviceAltName = device.altname;
            }

            if (camera.deviceFramerate == 0)
            {
                camera.deviceFramerate = device.outputs
                    .Select(output => Convert.ToInt32(output.framerate))
                    .OrderBy(framerate => framerate)
                    .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(camera.devicePixelFormat))
            {
                camera.devicePixelFormat = device.outputs
                    .Select(output => output.pixelFormat)
                    .FirstOrDefault();
            }
        }


        public static string CalculateCameraName(MixCastData.CameraCalibrationData camera)
        {
            if (!string.IsNullOrEmpty(camera.displayName))
                return camera.displayName;
            if (!string.IsNullOrEmpty(camera.deviceName))
                return camera.deviceName;

            int cameraIndex = 0;
            for (int i = 0; i < MixCast.Settings.cameras.Count; i++)
            {
                if (MixCast.Settings.cameras[i] == camera)
                    break;
                if (!string.IsNullOrEmpty(MixCast.Settings.cameras[i].displayName))
                    continue;
                if (!string.IsNullOrEmpty(MixCast.Settings.cameras[i].deviceName))
                    continue;
                cameraIndex++;
            }
            return string.Format(Text.Localization.Get("Str_VRCamNameDefault"), "(" + (cameraIndex + 1).ToString() + ")");
        }
    }
}
#endif
