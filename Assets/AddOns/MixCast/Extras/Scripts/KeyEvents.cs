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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
    public class KeyEvents : MonoBehaviour
    {
        public List<KeyCode> keys = new List<KeyCode>();
        public bool all = false;

        public bool ignoreIfFieldActive = true;

        public UnityEngine.Events.UnityEvent onDown = new UnityEngine.Events.UnityEvent();
        public UnityEngine.Events.UnityEvent onUp = new UnityEngine.Events.UnityEvent();

        bool lastState;

        void OnEnable()
        {
            SetState(CalculateNewState());
        }
        void Update()
        {
            bool newState = CalculateNewState();
            if (newState != lastState)
            {
                SetState(newState);
                if (newState)
                    onDown.Invoke();
                else
                    onUp.Invoke();
            }
        }

        bool CalculateNewState()
        {
            if (keys.Count == 0)
                return false;
            if (ignoreIfFieldActive && UnityEngine.UI.InputField.allSelectables.Find(s => s is InputField && (s as InputField).isFocused) != null)
                return false;

            if (all)
            {
                for (int i = 0; i < keys.Count; i++)
                    if (!Input.GetKey(keys[i]))
                        return false;
                return true;
            }
            else
            {
                for (int i = 0; i < keys.Count; i++)
                    if (Input.GetKey(keys[i]))
                        return true;
                return false;
            }
        }
        void SetState(bool newState)
        {
            lastState = newState;
        }
    }
}
