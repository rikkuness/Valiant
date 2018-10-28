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
using UnityEngine.Rendering;

namespace BlueprintReality.MixCast
{
    public abstract class ApplyRemovalToInputFeed : CameraComponent
    {
        protected static List<ApplyRemovalToInputFeed> activeRemovals = new List<ApplyRemovalToInputFeed>();

        protected override void OnEnable()
        {
            base.OnEnable();

            InputFeed.OnProcessInputStart += StartRender;
            InputFeed.OnProcessInputEnd += StopRender;

            activeRemovals.Add(this);
        }
        protected override void OnDisable()
        {
            activeRemovals.Remove(this);

            InputFeed.OnProcessInputStart -= StartRender;
            InputFeed.OnProcessInputEnd -= StopRender;

            base.OnDisable();
        }

        protected abstract void StartRender(InputFeed feed);
        protected abstract void StopRender(InputFeed feed);
        protected abstract bool IsPossible();

        protected bool OtherMethodActive()
        {
            for (int i = 0; i < activeRemovals.Count; i++)
                if (this != activeRemovals[i] && activeRemovals[i].context.Data == context.Data)
                    return true;
            return false;
        }
    }
}
#endif
