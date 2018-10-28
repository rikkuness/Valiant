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
using System.Collections.Generic;

#if MIXCAST_STEAMVR
using Valve.VR;
#endif

namespace BlueprintReality.MixCast {
	public partial class TrackedDeviceManager {
#if MIXCAST_STEAMVR
        private TrackedDevicePose_t[] trackedObjects = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

        private string[] trackedObjectGuids = new string[OpenVR.k_unMaxTrackedDeviceCount];

        public bool GetDeviceTransformByGuid_SteamVR(string guid, out Vector3 position, out Quaternion rotation)
        {
            int index = -1;
            for (int i = 0; i < trackedObjectGuids.Length; i++)
                if (trackedObjectGuids[i] == guid)
                    index = i;

            return GetDeviceTransformByIndex_SteamVR(index, out position, out rotation);
        }
        public bool GetDeviceTransformByIndex_SteamVR(int index, out Vector3 position, out Quaternion rotation)
        {
            if( index < 0 || index >= trackedObjects.Length)
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }

            TrackedDevicePose_t pose = trackedObjects[index];
            if (pose.bDeviceIsConnected && pose.bPoseIsValid && pose.eTrackingResult == ETrackingResult.Running_OK)
            {
                Matrix4x4 poseMat = TrackingSpaceOrigin.ConvertMatrixOpenVRToUnity(pose.mDeviceToAbsoluteTracking);
                position = TrackingSpaceOrigin.ExtractPosition(poseMat);
                position.z *= -1;   //flip for OpenGL to DirectX conversion
                Quaternion quat = TrackingSpaceOrigin.ExtractRotation(poseMat);
                quat = Quaternion.Euler(new Vector3(-quat.eulerAngles.x, -quat.eulerAngles.y, quat.eulerAngles.z));
                rotation = quat;
                return true;
            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }
        }
        public bool GetDeviceTransformByRole_SteamVR(DeviceRole role, out Vector3 position, out Quaternion rotation)
        {
            switch(role)
            {
                case DeviceRole.Head:
                    return GetDeviceTransformByIndex_SteamVR((int)SteamVR_TrackedObject.EIndex.Hmd, out position, out rotation);
                case DeviceRole.LeftHand:
                    return GetDeviceTransformByIndex_SteamVR((int)Valve.VR.OpenVR.System.GetTrackedDeviceIndexForControllerRole(Valve.VR.ETrackedControllerRole.LeftHand), out position, out rotation);
                case DeviceRole.RightHand:
                    return GetDeviceTransformByIndex_SteamVR((int)Valve.VR.OpenVR.System.GetTrackedDeviceIndexForControllerRole(Valve.VR.ETrackedControllerRole.RightHand), out position, out rotation);
                default:
                    position = Vector3.zero;
                    rotation = Quaternion.identity;
                    return false;
            }
        }

        private string camera0Name = "Camera0";
        private string camera1Name = "Camera1";
        private string camera2Name = "Camera2";
        private string camera3Name = "Camera3";
        /// <summary>
        /// Gets the sensor position under SteamVR coordinate system.
        /// </summary>
        /// <input>Sensor index (0 for first one, 1 for second)</input>
        /// <outputs>Sensor Position and Orientation</outputs>
        public bool GetSteamSensorPosition(int index, out Vector3 position, out Quaternion orientation)
        {
            position = Vector3.zero;
            orientation = Quaternion.identity;

            UpdateTransforms();
            Update_SteamVR();

            string camName = null;
            switch (index)
            {
                case 0:
                    {
                        camName = camera0Name;
                        break;
                    }
                case 1:
                    {
                        camName = camera1Name;
                        break;
                    }
                case 2:
                    {
                        camName = camera2Name;
                        break;
                    }
                case 3:
                    {
                        camName = camera3Name;
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (!string.IsNullOrEmpty(trackedObjectGuids[i]) && trackedObjectGuids[i].Contains(camName))
                {
                    GetDeviceTransformByGuid_SteamVR(trackedObjectGuids[i], out position, out orientation);
                    return true;
                }
            }
            return false;
        }

        public bool IsSteamSensorAvailable(int index)
        {
            UpdateTransforms();
            Update_SteamVR();

            for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                if (!string.IsNullOrEmpty(trackedObjectGuids[i]) && trackedObjectGuids[i].Contains("Camera" + index.ToString()))
                {
                    return true;
                }
            }

            return false;
        }


        protected void UpdateTransforms()
        {
            if (VRInfo.IsDeviceOpenVR())
            {
                if (OpenVR.System != null && OpenVR.Compositor != null)
                {
                    OpenVR.System.GetDeviceToAbsoluteTrackingPose(OpenVR.Compositor.GetTrackingSpace(), 0, trackedObjects);
                    if (OnTransformsUpdated != null)
                        OnTransformsUpdated();
                }
            }
        }

        private void Start_SteamVR()
        {
            if( MixCastCameras.Instance != null )
                MixCastCameras.Instance.OnBeforeRender += UpdateTransforms;
        }
        private void OnDestroy_SteamVR()
        {
            if (MixCastCameras.Instance != null)
                MixCastCameras.Instance.OnBeforeRender -= UpdateTransforms;
        }

        private void Update_SteamVR()
        {
            for (int i = 0; i < trackedObjects.Length; i++)
            {
                TrackedDevicePose_t pose = trackedObjects[i];
                if (pose.bDeviceIsConnected)
                {
                    if (string.IsNullOrEmpty(trackedObjectGuids[i]))
                    {
                        string checkGuid = VRInfo.GetDeviceSerial((uint)i);
                        if (!string.IsNullOrEmpty(checkGuid))
                            trackedObjectGuids[i] = checkGuid;
                    }
                }
                else
                {
                    trackedObjectGuids[i] = null;
                }
            }
        }
#endif
    }
}
#endif
