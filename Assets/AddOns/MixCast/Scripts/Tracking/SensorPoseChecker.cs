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
    /// <summary>
    /// MixCast Studio is using SteamVR (with OpenVR) as the main VR SDK.
    /// But Oculus SDK have different point of origin in the real world than SteamVR.
    /// They can be setup independently.
    ///
    /// When user of MixCast SDK uses Oculus as its main VR SDK for their project, the virtual camera
    /// is wronly positioned.
    /// 
    /// This script reads the sensor pose and saves it into MixCast settings.
    /// This can help us to do calibration work.
    /// </summary>
    public class SensorPoseChecker : MonoBehaviour
    {
        public bool checkInFixedUpdate = false;

        private Vector3 cachedPosition;
        //private Quaternion cachedRotation;

        void Start()
        {
            cachedPosition = transform.localPosition;
            //cachedRotation = transform.localRotation;
        }

        void Update()
        {
            if (!checkInFixedUpdate)
                SensorCheck();
        }

        void FixedUpdate()
        {
            if (checkInFixedUpdate)
                SensorCheck();
        }

        const string SPACE_CHANGE_WARNING = "MixCast: Tracking Space change detected!";

        void SensorCheck()
        {
#if UNITY_2017_2_OR_NEWER
            if (XRDevice.isPresent && VRInfo.IsVRModelOculus())
#else
            if (VRDevice.isPresent && VRInfo.IsVRModelOculus())
#endif
            {
                Vector3 newPos;
                Quaternion newRot;

                var oldSensorPose = MixCast.Settings.sensorPose;

                var index = MixCast.Settings.sensorIndex;
                if (TrackedDeviceManager.Instance.GetSensorPosition(index, out newPos, out newRot))
                {
                    if (newPos != Vector3.zero || newRot != Quaternion.identity)
                    {
                        if (WasSensorDifferent(oldSensorPose, newPos, newRot))
                        {
                            SetTransformFromOldPoseToNewPose(oldSensorPose, newPos, newRot);

                            if (VRInfo.IsDeviceOpenVR())
                            {
                                Debug.Log(SPACE_CHANGE_WARNING);
                                MixCast.Settings.sensorPose = new MixCastData.SensorPose() { position = newPos, rotation = newRot };
                            }
                        }
                    }
                }
            }
        }

        private bool WasSensorDifferent(MixCastData.SensorPose oldSensorPose, Vector3 newPos, Quaternion newRot)
        {
            return (oldSensorPose.position != newPos) || (oldSensorPose.rotation != newRot);
        }

        private void SetTransformFromOldPoseToNewPose(MixCastData.SensorPose oldPose, Vector3 newPos, Quaternion newRot)
        {
            Vector3 pos;
            Quaternion rot;
            TrackingSpaceOrigin.GetOriginOffsetData(newPos, newRot, oldPose.position, oldPose.rotation, out pos, out rot);

            transform.localPosition = cachedPosition + pos;
            //Remove orientation change for the camera. Will remove for next release.
            //transform.localRotation = cachedRotation * rot;
        }
    }
}
#endif
