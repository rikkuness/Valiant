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
#if MIXCAST_STEAMVR
using Valve.VR;
#endif

namespace BlueprintReality.MixCast
{
    public class SetActiveFromCameraTracked : CameraComponent
    {
        public List<GameObject> tracked = new List<GameObject>();
        public List<GameObject> untracked = new List<GameObject>();

        private bool lastState;

        protected override void OnEnable()
        {
            base.OnEnable();
     
            SetState(CalculateNewState());
        }
        void Update()
        {
            bool newState = CalculateNewState();
            if (newState != lastState)
                SetState(newState);
        }

        bool CalculateNewState()
        {
            if (context.Data == null || context.Data.wasTracked == false)
                return false;

#if MIXCAST_STEAMVR
            if (VRInfo.IsDeviceOpenVR() && IsTracked_Steam())
                return true;
#endif
#if MIXCAST_OCULUS
            if (VRInfo.IsDeviceOculus() && IsTracked_Oculus())
                return true;
#endif

            return false;
        }
        void SetState(bool newState)
        {
            tracked.ForEach(g => g.SetActive(newState));
            untracked.ForEach(g => g.SetActive(!newState));
            lastState = newState;
        }


#if MIXCAST_STEAMVR
        private SteamVR_TrackedObject.EIndex trackedByIndexSteam = SteamVR_TrackedObject.EIndex.None;

        bool IsTracked_Steam()
        {
            if (string.IsNullOrEmpty(context.Data.trackedByDevice))
                return false;

            try {
                if (trackedByIndexSteam.ToString() != context.Data.trackedByDevice)
                    trackedByIndexSteam = (SteamVR_TrackedObject.EIndex)System.Enum.Parse(typeof(SteamVR_TrackedObject.EIndex), context.Data.trackedByDevice);

                if (trackedByIndexSteam == SteamVR_TrackedObject.EIndex.None || (int)trackedByIndexSteam >= Valve.VR.OpenVR.k_unMaxTrackedDeviceCount)
                    return false;

                if (!SteamVR.instance.hmd.IsTrackedDeviceConnected((uint)trackedByIndexSteam))
                    return false;

                return true;
            }
            catch(System.Exception)
            {
                return false;
            }
        }
#endif

#if MIXCAST_OCULUS
        bool IsTracked_Oculus()
        {
            if (string.IsNullOrEmpty(context.Data.trackedByDevice))
                return false;

            try {
                OVRInput.Controller controller = OVRInput.Controller.None;
                if (context.Data.trackedByDevice == "Device1")
                    controller = OVRInput.Controller.LTouch;
                else if (context.Data.trackedByDevice == "Device2")
                    controller = OVRInput.Controller.RTouch;

                return OVRInput.IsControllerConnected(controller) && OVRInput.GetControllerPositionTracked(controller) && OVRInput.GetControllerOrientationTracked(controller);
            }
            catch (System.Exception)
            {
                return false;
            }
        }
#endif
    }
}
#endif
