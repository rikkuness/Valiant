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
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
	public class SetDesktopStateFromToggle : MonoBehaviour
    {
        public enum StateType
        {
            ShowUI, ShowOutput
        }

        public StateType stateType = StateType.ShowUI;
        public bool invert = false;
        public Toggle toggle;

        void OnEnable()
        {
            if (toggle == null)
                toggle = GetComponentInParent<Toggle>();

            toggle.onValueChanged.AddListener(HandleToggleChanged);
            toggle.isOn = CalculateNewState();
        }
        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(HandleToggleChanged);
        }

        private void Update()
        {
            if (toggle.isOn != CalculateNewState())
                toggle.isOn = CalculateNewState();
        }

        private void HandleToggleChanged(bool newVal)
        {
            if (invert)
                newVal = !newVal;

            switch(stateType)
            {
                case StateType.ShowUI:
                    MixCast.Desktop.ShowingUI = newVal;
                    break;
                case StateType.ShowOutput:
                    MixCast.Desktop.ShowingOutput = newVal;
                    break;
            }
        }
        bool CalculateNewState()
        {
            bool newState = false;
            switch (stateType)
            {
                case StateType.ShowUI:
                    newState = MixCast.Desktop.ShowingUI;
                    break;
                case StateType.ShowOutput:
                    newState = MixCast.Desktop.ShowingOutput;
                    break;
            }
            if (invert)
                return !newState;
            else
                return newState;
        }
    }
}
#endif
