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
    public class SetRenderingForMixCast : MonoBehaviour
    {
        public List<Renderer> targets = new List<Renderer>();
        [UnityEngine.Serialization.FormerlySerializedAs("renderForMixCast")]
        public bool renderForMixedReality = false;
        public bool renderForThirdPerson = false;

        protected virtual void OnEnable()
        {
            MixCastCamera.GameRenderStarted += HandleMixCastRenderStarted;
            MixCastCamera.GameRenderEnded += HandleMixCastRenderEnded;

            if (targets.Count == 0)
                GetComponentsInChildren<Renderer>(targets);

            //Set targets to the desired state during standard Unity rendering (not MixCast)
            targets.ForEach(r => r.enabled = !(renderForMixedReality || renderForThirdPerson));
        }
        protected virtual void OnDisable()
        {
            MixCastCamera.GameRenderStarted -= HandleMixCastRenderStarted;
            MixCastCamera.GameRenderEnded -= HandleMixCastRenderEnded;
        }


        private void HandleMixCastRenderStarted(MixCastCamera cam)
        {
            for( int i = 0; i < targets.Count; i++ )
            {
                targets[i].enabled = !string.IsNullOrEmpty(cam.context.Data.deviceName) ? renderForMixedReality : renderForThirdPerson;
            }
        }
        private void HandleMixCastRenderEnded(MixCastCamera cam)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].enabled = !(!string.IsNullOrEmpty(cam.context.Data.deviceName) ? renderForMixedReality : renderForThirdPerson);
            }
        }


        private void Reset()
        {
            targets.Clear();
            GetComponentsInChildren<Renderer>(targets);
        }
    }
}
#endif
