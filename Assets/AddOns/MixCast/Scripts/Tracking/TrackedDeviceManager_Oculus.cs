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
#if MIXCAST_STEAMVR
using Valve.VR;
#endif

namespace BlueprintReality.MixCast
{
	public partial class TrackedDeviceManager
    {
#if MIXCAST_OCULUS
        public bool GetDeviceTransformByGuid_Oculus(string guid, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }
        public bool GetDeviceTransformByIndex_Oculus(int index, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }
        public bool GetDeviceTransformByRole_Oculus(DeviceRole role, out Vector3 position, out Quaternion rotation)
        {
            OVRPlugin.Node node;
            switch(role)
            {
                case DeviceRole.Head:
                    node = OVRPlugin.Node.Head;
                    break;
                case DeviceRole.LeftHand:
                    node = OVRPlugin.Node.HandLeft;
                    break;
                case DeviceRole.RightHand:
                    node = OVRPlugin.Node.HandRight;
                    break;
                default:
                    position = Vector3.zero;
                    rotation = Quaternion.identity;
                    return false;
            }
            if (OVRPlugin.GetNodePositionTracked(node) && OVRPlugin.GetNodeOrientationTracked(node))
            {
                OVRPlugin.Posef pose = OVRPlugin.GetNodePose(node, OVRPlugin.Step.Render);
                position = new Vector3(pose.Position.x, pose.Position.y, pose.Position.z);
                rotation = new Quaternion(pose.Orientation.x, pose.Orientation.y, pose.Orientation.z, pose.Orientation.w);
                return true;
            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }
        }

        /// <summary>
        /// Gets the sensor position under Oculus coordinate system.
        /// </summary>
        /// <input>Sensor index (0 for first one, 1 for second)</input>
        /// <outputs>Sensor Position and Orientation</outputs>
        public bool GetOculusSensorPosition(int index, out Vector3 position, out Quaternion orientation)
        {
            position = Vector3.zero;
            orientation = Quaternion.identity;

            OVRPose p;
            switch (index)
            {
                case 0:
                    p = OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerZero, OVRPlugin.Step.Render).ToOVRPose();
                    break;
                case 1:
                    p = OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerOne, OVRPlugin.Step.Render).ToOVRPose();
                    break;
                case 2:
                    p = OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerTwo, OVRPlugin.Step.Render).ToOVRPose();
                    break;
                case 3:
                    p = OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerThree, OVRPlugin.Step.Render).ToOVRPose();
                    break;
                default:
                    return false;
            }

            position = p.position;
            orientation = p.orientation * Quaternion.Euler(0, 180, 0);
            return true;
        }


        public bool IsOculusSensorAvailable(int index)
        {
            switch (index)
            {
                case 0:
                    return (OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerZero, OVRPlugin.Step.Render).ToOVRPose()) != OVRPose.identity;
                case 1:
                    return (OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerOne, OVRPlugin.Step.Render).ToOVRPose()) != OVRPose.identity;
                case 2:
                    return (OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerTwo, OVRPlugin.Step.Render).ToOVRPose()) != OVRPose.identity;
                case 3:
                    return (OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerThree, OVRPlugin.Step.Render).ToOVRPose()) != OVRPose.identity;
                default:
                    return false;
            }
        }

        public void UpdateTransforms_Oculus()
        {
            if (OnTransformsUpdated != null)
                OnTransformsUpdated();
        }
#endif
    }
}
#endif
