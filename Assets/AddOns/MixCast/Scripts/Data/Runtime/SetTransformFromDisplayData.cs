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

namespace BlueprintReality.MixCast
{
    public class SetTransformFromDisplayData : CameraComponent
    {
        protected virtual void Update()
        {
            if (context.Data == null)
                return;

            //transform.localScale = context.Data.displayData.scale * Vector3.one;
            switch (context.Data.displayData.mode)
            {
                case MixCastData.SceneDisplayData.PlacementMode.Camera:
                    MixCastCamera target = MixCastCamera.FindCamera(context);
                    if (target != null)
                    {
                        transform.position = target.displayTransform.position;
                        transform.rotation = target.displayTransform.rotation;
                    }
                    
                    break;
                case MixCastData.SceneDisplayData.PlacementMode.World:
                    transform.localPosition = context.Data.displayData.position;
                    transform.localRotation = context.Data.displayData.rotation;
                    break;
                case MixCastData.SceneDisplayData.PlacementMode.Headset:
                    break;
            }
        }
    }
}
#endif
