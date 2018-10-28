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
using UnityEngine;

namespace BlueprintReality.MixCast
{
	public partial class TrackedDeviceManager : MonoBehaviour
    {
        public bool loggedTrackingError { get; protected set; }

        // These enums need to be the same names as the OpenVR device role enums, although they don't need to be in the same order.
        // This is because we use them to translate back and forth between device roles in SteamVR in Mixcast Studio and device roles for the Oculus SDK
        public enum DeviceRole
        {
            Head, LeftHand, RightHand, Invalid
        }

        private static TrackedDeviceManager instance;
        public static TrackedDeviceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("TrackedDeviceManager");
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    DontDestroyOnLoad(obj);
                    instance = obj.AddComponent<TrackedDeviceManager>();
                }
                return instance;
            }
        }

        public static event Action OnTransformsUpdated;

        public bool GetDeviceTransform(string guid, string role, out Vector3 position, out Quaternion rotation)
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
                return GetDeviceTransformByGuid_SteamVR(guid, out position, out rotation);
#endif
#if MIXCAST_OCULUS
            if (VRInfo.IsDeviceOculus())
            {
                DeviceRole vrRole;

                try
                {
                     vrRole = (DeviceRole)Enum.Parse(typeof(DeviceRole), role);
                }
                catch
                {
                    // Invalid role name
                    if (!loggedTrackingError)
                    {
                        // Avoid thousands of logged errors
                        Debug.LogError("Trying to get position for invalid controller role: " + role);
                        loggedTrackingError = true;
                    }
                    position = Vector3.zero;
                    rotation = Quaternion.identity;
                    return false;
                }

                return GetDeviceTransformByRole_Oculus(vrRole, out position, out rotation);
            }
#endif

            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        public bool GetDeviceTransformByGuid(string guid, out Vector3 position, out Quaternion rotation)
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
                return GetDeviceTransformByGuid_SteamVR(guid, out position, out rotation);
#endif
#if MIXCAST_OCULUS
            if (VRInfo.IsDeviceOculus())
                return GetDeviceTransformByGuid_Oculus(guid, out position, out rotation);
#endif

            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }
        public bool GetDeviceTransformByIndex(int index, out Vector3 position, out Quaternion rotation)
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
                return GetDeviceTransformByIndex_SteamVR(index, out position, out rotation);
#endif
#if MIXCAST_OCULUS
            if (VRInfo.IsDeviceOculus())
                return GetDeviceTransformByIndex_Oculus(index, out position, out rotation);
#endif

            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }
        public bool GetDeviceTransformByRole(DeviceRole role, out Vector3 position, out Quaternion rotation)
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
                return GetDeviceTransformByRole_SteamVR(role, out position, out rotation);
#endif
#if MIXCAST_OCULUS
            if (VRInfo.IsDeviceOculus())
                return GetDeviceTransformByRole_Oculus(role, out position, out rotation);
#endif

            if (Application.isEditor) {
                position = Camera.main.transform.localPosition;
                rotation = Camera.main.transform.localRotation;
                return true;
            }

            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        const string UNKNOWN_MESSAGE = "Unknown VR Device";
        public bool GetSensorPosition(int index, out Vector3 position, out Quaternion rotation)
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
                return GetSteamSensorPosition(index, out position, out rotation);
#endif
#if MIXCAST_OCULUS
            if (VRInfo.IsDeviceOculus())
                return GetOculusSensorPosition(index, out position, out rotation);
#endif
            Debug.LogError(UNKNOWN_MESSAGE);
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        public bool IsSensorAvailable(int requestIndex)
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
                return IsSteamSensorAvailable(requestIndex);
#endif
#if MIXCAST_OCULUS
            if (VRInfo.IsDeviceOculus())
                return IsOculusSensorAvailable(requestIndex);
#endif
            Debug.LogError(UNKNOWN_MESSAGE);
            return false;
        }

        private void Start()
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
                Start_SteamVR();
#endif
        }
        private void OnDestroy()
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
                OnDestroy_SteamVR();
#endif
        }
        private void Update()
        {
#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR())
                Update_SteamVR();
#endif
        }
    }
}
#endif
