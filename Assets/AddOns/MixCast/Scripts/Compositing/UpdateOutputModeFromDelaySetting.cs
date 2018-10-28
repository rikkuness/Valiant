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

namespace BlueprintReality.MixCast
{
    public class UpdateOutputModeFromDelaySetting : CameraComponent
    {
        private bool lastState;
        private float lastDelay = -1f;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetNewState(CalculateNewState());
        }

        private void Update()
        {
            bool newState = CalculateNewState();
            if (newState != lastState)
                SetNewState(newState);
        }

        bool CalculateNewState()
        {
            if (context.Data == null)
                return false;
            if( (context.Data.unplugged ? 0f : context.Data.bufferTime) != lastDelay )
                return true;
            return false;
        }

        void SetNewState( bool newState ) {
            if( context.Data != null ) {
                lastDelay = context.Data.bufferTime;
            }
            UpdateOutputMode();
            lastState = newState;
        }

        private void UpdateOutputMode() {
            if( context == null || context.Data == null )
            {
                return;
            }
            if( Mathf.Approximately(context.Data.bufferTime, 0) || context.Data.unplugged)
            {
                context.Data.outputMode = MixCastData.OutputMode.Immediate;
            } else
            {
                context.Data.outputMode = MixCastData.OutputMode.Buffered;
            }
        }
    }
}
#endif
