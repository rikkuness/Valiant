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
    public class SetActiveFromStreamConfigured : CameraComponent
    {
        public List<GameObject> on = new List<GameObject>();
        public List<GameObject> off = new List<GameObject>();

        private bool lastState = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            ApplyState(CalculateState());
        }
        private void Update()
        {
            bool newState = CalculateState();
            if (newState != lastState)
                ApplyState(newState);
        }

        bool CalculateState()
        {
            bool cameraSpecific = context.Data != null && context.Data.recordingData.perCamStreamService != MixCastData.StreamingService.None;
            MixCastData.StreamingService streamingService;
            string streamingUrl;
            string streamingKey;

            if (cameraSpecific)
            {
                streamingService = context.Data.recordingData.perCamStreamService;
                streamingUrl = context.Data.recordingData.perCamStreamUrl;
                streamingKey = context.Data.recordingData.perCamStreamKey;
            }
            else
            {
                streamingService = MixCast.Settings.global.defaultStreamService;
                streamingUrl = MixCast.Settings.global.defaultStreamUrl;
                streamingKey = MixCast.Settings.global.defaultStreamKey;
            }

            switch (streamingService)
            {
                case MixCastData.StreamingService.None:
                    return false;
                case MixCastData.StreamingService.Custom:
                    return !string.IsNullOrEmpty(streamingUrl);
                case MixCastData.StreamingService.Facebook:
                case MixCastData.StreamingService.Mixer:
                case MixCastData.StreamingService.Twitch:
                case MixCastData.StreamingService.Twitter:
                case MixCastData.StreamingService.YouTube:
                    return !string.IsNullOrEmpty(streamingKey);
                default:
                    return false;
            }
        }

        void ApplyState(bool state)
        {
            for (int i = 0; i < on.Count; i++)
                on[i].SetActive(state);
            for (int i = 0; i < off.Count; i++)
                off[i].SetActive(!state);

            lastState = state;
        }
    }
}
#endif
