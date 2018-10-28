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

namespace BlueprintReality.MixCast
{
    public class SetActiveFromIsolationMode : CameraComponent
    {
        public List<MixCastData.IsolationMode> matchModes = new List<MixCastData.IsolationMode>();
        public bool mustBeCalibrated = false;
        public bool checkCalibrationOnly = false;
        public bool requireAll = false;

        public List<GameObject> activeIfMatch = new List<GameObject>();
        public List<GameObject> inactiveIfMatch = new List<GameObject>();

        private bool lastState;

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

            if( requireAll ) {
                int count = 0;
                for( int i = 0; i < matchModes.Count; i++ ) {
                    if( IsModeActive( matchModes[i] ) ) {
                        if( checkCalibrationOnly && IsModeCalibrated( matchModes[i] ) ) {
                            ++count;
                        } else if( IsModeActive( matchModes[i] ) && (!mustBeCalibrated || IsModeCalibrated( matchModes[i] )) ) {
                            ++count;
                        }
                    } else {
                        ++count;
                    }
                }
                return count == matchModes.Count;
            } else {
                for( int i = 0; i < matchModes.Count; i++ ) {
                    if( checkCalibrationOnly && IsModeCalibrated( matchModes[i] ) ) {
                        return true;
                    } else if( IsModeActive( matchModes[i] ) && (!mustBeCalibrated || IsModeCalibrated( matchModes[i] )) ) {
                        return true;
                    }
                }
                return false;
            }
            

            
        }

        bool IsModeActive(MixCastData.IsolationMode mode)
        {
            switch (mode)
            {
                case MixCastData.IsolationMode.StaticSubtraction:
                    return context.Data.staticSubtractionData.active;
                case MixCastData.IsolationMode.Chromakey:
                    return context.Data.chromakeying.active;
                case MixCastData.IsolationMode.StaticDepth:
                    return context.Data.staticDepthData.active;
                default:
                    return false;
            }
        }

        bool IsModeCalibrated(MixCastData.IsolationMode mode)
        {
            switch (mode)
            {
                case MixCastData.IsolationMode.StaticSubtraction:
                    return context.Data.staticSubtractionData.calibrated;
                case MixCastData.IsolationMode.Chromakey:
                    return context.Data.chromakeying.calibrated;
                case MixCastData.IsolationMode.StaticDepth:
                    return context.Data.staticDepthData.calibrated;
                default:
                    return false;
            }
        }

        void SetNewState(bool newState)
        {
            lastState = newState;
            activeIfMatch.ForEach(g => g.SetActive(newState));
            inactiveIfMatch.ForEach(g => g.SetActive(!newState));
        }
    }
}
#endif
