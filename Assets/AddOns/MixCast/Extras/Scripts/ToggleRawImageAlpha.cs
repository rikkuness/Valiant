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
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{

    public class ToggleRawImageAlpha : MonoBehaviour {

        public RawImage objectToToggle;

        public void Toggle()
        {
            if (objectToToggle.gameObject.activeSelf)
            {
                objectToToggle.color = new Color(objectToToggle.color.r, objectToToggle.color.g, objectToToggle.color.b, 0);
                //objectToToggle.gameObject.SetActive(false);
            }
            else
            {
                objectToToggle.color = new Color(objectToToggle.color.r, objectToToggle.color.g, objectToToggle.color.b, 1);
                //objectToToggle.gameObject.SetActive(true);
            }
        }

        public void ToggleIfMixCast()
        {
            if (MixCast.Active)
            {
                Toggle();
            }
        }
    }
}
#endif
