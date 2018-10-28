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
using System;
using UnityEngine;
using BlueprintReality.GameObjects;

namespace BlueprintReality.MixCast
{
    public class BuildTargetWarnings : MonoBehaviour
    {

        public bool warnOn32BitBuild = true;

        // Use this for initialization
        void Start()
        {

#if UNITY_EDITOR
                return;
#else

            switch (IntPtr.Size)
            {
                case 4:
                    if (warnOn32BitBuild)
                    {
                        ToastCenter.ShowToast("Field_Warning", "Action_x86Build", "MixCast is built for 32 bit (x86) platform but 64 bit (x64) is required. This build of MixCast may not work correctly", 10f);
                        Debug.Log("MixCast is built for 32 bit (x86) platform but 64 bit (x64) is required. This build of MixCast may not work correctly");
                    }
                    break;
                case 8:
                    //Debug.Log("MixCast is built for 64bit (x64) platform");
                    break;
                default:
                    Debug.Log("Hit default case in BuiltTargetWarnings; IntPtr.Size is returning neither 8 (for x64) or 4 (for x86)");
                    break;
            }
#endif
        }
    }
}
#endif
