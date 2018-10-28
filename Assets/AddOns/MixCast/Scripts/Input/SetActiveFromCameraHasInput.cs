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

namespace BlueprintReality.MixCast {
	public class SetActiveFromCameraHasInput : CameraComponent {
        [SerializeField] bool setActiveIfNoInput = false;

        public List<GameObject> active = new List<GameObject>();
        public List<GameObject> inactive = new List<GameObject>();

        private bool lastState;

        protected override void OnEnable()
        {
            base.OnEnable();
            ApplyState(CalculateNewState());
        }

        private void Update()
        {
            bool newState = CalculateNewState();
            if (newState != lastState)
                ApplyState(newState);
        }

        bool CalculateNewState()
        {
            if(setActiveIfNoInput) {
                return context.Data != null && string.IsNullOrEmpty( context.Data.deviceName );
            }
            else {
                return context.Data != null && !string.IsNullOrEmpty( context.Data.deviceName );
            }
            
        }
        void ApplyState(bool newState)
        {
            active.ForEach(g => g.SetActive(newState));
            inactive.ForEach(g => g.SetActive(!newState));
            lastState = newState;
        }
    }
}
#endif
