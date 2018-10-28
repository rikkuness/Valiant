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
using System;
using System.Text;
using System.Collections.Generic;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif
#if MIXCAST_STEAMVR
using Valve.VR;
#endif

namespace BlueprintReality.MixCast
{
    public class VRInfo : MonoBehaviour
    {
        private static VRInfo mInstance = null;
        private static VRInfo instance
        {
            get
            {
                if(mInstance == null)
                {
                    var obj = new GameObject("VRInfo");
                    mInstance = obj.AddComponent<VRInfo>();
                }
                return mInstance;
            }
        }

        private const string OPENVR_DEVICE_NAME = "OpenVR";
        private const string OCULUS_DEVICE_NAME = "Oculus";
        private const string VIVE_MODEL_NAME = "vive";
        private const string OCULUS_MODEL_NAME = "rift";

        public static string loadedDeviceNameOverride = "";

        public enum VRSystemType { Oculus, Lighthouse, Holographic, Unknown }

        private string loadedDeviceName = null;
        private string deviceModel = null;
        private string vrSystem = null;
        private VRSystemType vrSystemType = VRSystemType.Unknown;

        private void Awake()
        {
            Initialize();
        }

        private bool vrEnableState = false;
        private void Update()
        {
#if UNITY_2017_2_OR_NEWER
            var newState = XRSettings.enabled;
#else
            var newState = VRSettings.enabled;
#endif
            if (newState != vrEnableState)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
#if UNITY_2017_2_OR_NEWER
            vrEnableState = XRSettings.enabled;
            loadedDeviceName = XRSettings.loadedDeviceName;
            deviceModel = XRDevice.model.ToLower();
#else
            vrEnableState = VRSettings.enabled;
            loadedDeviceName = VRSettings.loadedDeviceName;
            deviceModel = VRDevice.model.ToLower();
#endif

#if MIXCAST_STEAMVR
            if ((!string.IsNullOrEmpty(loadedDeviceNameOverride) && loadedDeviceNameOverride == OPENVR_DEVICE_NAME) || loadedDeviceName == OPENVR_DEVICE_NAME)
            {
                if (SteamVR.enabled)
                {
                    vrSystem = SteamVR.instance.hmd_TrackingSystemName;

                    if (!string.IsNullOrEmpty(vrSystem))
                    {
                        for (int i = 0; i < (int)VRSystemType.Unknown; i++)
                        {
                            VRSystemType sysType = (VRSystemType)i;
                            if (sysType.ToString().ToLower() == vrSystem.ToLower())
                            {
                                vrSystemType = sysType;
                                break;
                            }
                        }
                    }
                    else
                        vrSystemType = VRSystemType.Unknown;
                }
                else
                {
                    Debug.LogError("OpenVR init failed");
                }
            }
            else
            {
                Debug.LogError("Try to get OpenVR system when OpenVR is disabled");
            }
#endif
        }
#if MIXCAST_STEAMVR
        private Dictionary<uint, uint> serialCapacity = new Dictionary<uint, uint>();
        private Dictionary<uint, StringBuilder> serialBuilder = new Dictionary<uint, StringBuilder>();
#endif
        public static string GetDeviceSerial(uint deviceIndex)
        {
#if MIXCAST_STEAMVR
            if (!IsDeviceOpenVR())
            {
                return deviceIndex.ToString();
            }

            var error = Valve.VR.ETrackedPropertyError.TrackedProp_Success;
            var capactiy = Valve.VR.OpenVR.System.GetStringTrackedDeviceProperty(deviceIndex, Valve.VR.ETrackedDeviceProperty.Prop_SerialNumber_String, null, 0, ref error);
            StringBuilder result = null;
            if (capactiy > 1)
            {
                if (instance.serialCapacity.ContainsKey(deviceIndex))
                {
                    if (instance.serialCapacity[deviceIndex] == capactiy)
                    {
                        result = instance.serialBuilder[deviceIndex];
                    }
                    else
                    {
                        instance.serialCapacity.Remove(deviceIndex);
                        instance.serialBuilder.Remove(deviceIndex);

                        result = new StringBuilder((int)capactiy);

                        instance.serialCapacity.Add(deviceIndex, capactiy);
                        instance.serialBuilder.Add(deviceIndex, result);
                    }
                }
                else
                {
                    result = new StringBuilder((int)capactiy);

                    instance.serialCapacity.Add(deviceIndex, capactiy);
                    instance.serialBuilder.Add(deviceIndex, result);
                }

                Valve.VR.OpenVR.System.GetStringTrackedDeviceProperty(deviceIndex, Valve.VR.ETrackedDeviceProperty.Prop_SerialNumber_String, result, capactiy, ref error);
                return result.ToString();
            }

            return deviceIndex.ToString();
#else
            // TODO: Find a way how to get Device Serial Number from Oculus SDK
            return deviceIndex.ToString();
#endif
        }

        public static bool IsDeviceOculus()
        {
            if (!string.IsNullOrEmpty(loadedDeviceNameOverride))
                return loadedDeviceNameOverride == OCULUS_DEVICE_NAME;
            return instance.loadedDeviceName == OCULUS_DEVICE_NAME;
        }

        public static bool IsDeviceOpenVR()
        {
            if (!string.IsNullOrEmpty(loadedDeviceNameOverride))
                return loadedDeviceNameOverride == OPENVR_DEVICE_NAME;
            return instance.loadedDeviceName == OPENVR_DEVICE_NAME;
        }

        public static bool IsVRModelVive()
        {
            return instance.deviceModel.Contains(VIVE_MODEL_NAME);
        }

        public static bool IsVRModelOculus()
        {
            return instance.deviceModel.Contains(OCULUS_MODEL_NAME);
        }

        public static string GetVRSystem()
        {
            return instance.vrSystem;
        }

        public static bool IsVRSystemVive()
        {
            return instance.vrSystemType == VRSystemType.Lighthouse;
        }
        public static bool IsVRSystemOculus()
        {
            return instance.vrSystemType == VRSystemType.Oculus;
        }
        public static bool IsVRSystemWindowsMR()
        {
            return instance.vrSystemType == VRSystemType.Holographic;
        }

        public static VRSystemType GetVRSystemType()
        {
            return instance.vrSystemType;
        }

    }
}
#endif
