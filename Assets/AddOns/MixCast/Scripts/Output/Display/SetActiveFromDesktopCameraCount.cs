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
    public class SetActiveFromDesktopCameraCount : MonoBehaviour
    {
        public int minimumCameraCount = 1;
        public int maximumCameraCount = 1;

        public List<GameObject> activeInUse = new List<GameObject>();
        public List<GameObject> inactiveInUse = new List<GameObject>();

        private bool lastState;

        protected void OnEnable()
        {
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
            return MixCast.Desktop.DisplaySlots >= minimumCameraCount && MixCast.Desktop.DisplaySlots <= maximumCameraCount;
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
