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
    public class SetActiveFromCameraInUse : CameraComponent
    {
        public bool displaying = false;
        public bool recording = false;
        public bool streaming = false;
        public bool timelapsing = false;

        public bool activelyEncoding = false;

        public List<GameObject> activeInUse = new List<GameObject>();
        public List<GameObject> inactiveInUse = new List<GameObject>();

        bool lastState;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetNewState(CalculateNewState());
        }

        void Update()
        {
            bool newState = CalculateNewState();
            if (newState != lastState)
                SetNewState(newState);
        }

        bool CalculateNewState()
        {
            if (context == null || context.Data == null)
            {
                return lastState;
            }

            var cameras = GetCameras();
            var newState = cameras.Contains(context.Data);

            if (newState && activelyEncoding)
            {
                newState = IsActivelyEncoding(cameras);
            }

            return newState;
        }

        List<MixCastData.CameraCalibrationData> GetCameras()
        {
            if (displaying)  return MixCast.Desktop.displayingCameras;
            if (recording)   return MixCast.RecordingCameras;
            if (streaming)   return MixCast.StreamingCameras;
            if (timelapsing) return MixCast.TimelapseCameras;
            return new List<MixCastData.CameraCalibrationData>();
        }

        string GetCategory()
        {
            if (recording) return EventCenter.Category.Recording;
            if (streaming) return EventCenter.Category.Streaming;
            return null;
        }

        bool IsActivelyEncoding(List<MixCastData.CameraCalibrationData> cameras)
        {
            var category = GetCategory();

            if (string.IsNullOrEmpty(category))
            {
                return false;
            }

            List<MixCastData.CameraCalibrationData> activelyEncodingCameras;

            if (!MixCast.ActivelyEncoding.TryGetValue(category, out activelyEncodingCameras))
                return false;

            for (int i = 0; i < cameras.Count; i++)
            {
                if (activelyEncodingCameras.Contains(cameras[i]))
                    return true;
            }
            return false;
        }

        void SetNewState(bool newState)
        {
            lastState = newState;
            activeInUse.ForEach(g => g.SetActive(newState));
            inactiveInUse.ForEach(g => g.SetActive(!newState));
        }
    }
}
#endif
