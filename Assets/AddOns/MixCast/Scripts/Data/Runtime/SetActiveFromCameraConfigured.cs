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

namespace BlueprintReality.MixCast {
    public class SetActiveFromCameraConfigured : CameraComponent
    {
        private bool lastState;

        public List<GameObject> activeIfMatch = new List<GameObject>();
        public List<GameObject> inactiveIfMatch = new List<GameObject>();

        protected override void OnEnable() {
            base.OnEnable();
            SetNewState( CalculateNewState() );
        }

        private void Update() {
            bool newState = CalculateNewState();
            if( newState != lastState )
                SetNewState( newState );
        }

        bool CalculateNewState() {
            return context.Data.isTransformConfigured;
        }

        void SetNewState( bool newState ) {
            lastState = newState;
            activeIfMatch.ForEach( g => g.SetActive( newState ) );
            inactiveIfMatch.ForEach( g => g.SetActive( !newState ) );
        }
    }
}
#endif
