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
using System;
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
    public class AudioAsyncFeedInitializer : CameraComponent
    {
        string camID = "";
        // Use this for initialization
        protected override void OnEnable()
        {
            base.OnEnable();

            if (context == null || context.Data == null || context.Data.audioData == null)
            {
                Debug.Log("The audio context is null on startup");
                return;
            }
            camID = context.Data.id;
            AudioAsyncFeed.Instance(camID); // adds it to the database...
        }

        private void OnDestroy() {
            AudioAsyncFeed.RemoveCamera(camID); // context may have been deleted already
        }
    }//class
}
#endif
