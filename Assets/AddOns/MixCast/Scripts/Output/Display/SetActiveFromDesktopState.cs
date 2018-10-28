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

namespace BlueprintReality.MixCast {
	public class SetActiveFromDesktopState : MonoBehaviour {
        public enum StateType
        {
            ShowingUI, ShowingOutput
        }

        public StateType stateType = StateType.ShowingUI;

        public List<GameObject> active = new List<GameObject>();
        public List<GameObject> inactive = new List<GameObject>();

        private bool lastState;

        private void OnEnable()
        {
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
            switch(stateType)
            {
                case StateType.ShowingUI:
                    return MixCast.Desktop.ShowingUI;
                case StateType.ShowingOutput:
                    return MixCast.Desktop.ShowingOutput;
                default:
                    return false;
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
