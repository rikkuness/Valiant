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
    public class SetActiveFromOutputResolution : CameraComponent
    {
        public List<MixCastData.OutputResolution> matches = new List<MixCastData.OutputResolution>();

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
            if (context == null || context.Data == null)
                return false;
            for (int i = 0; i < matches.Count; i++)
                if (matches[i] == context.Data.outputResolution)
                    return true;
            return false;
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
