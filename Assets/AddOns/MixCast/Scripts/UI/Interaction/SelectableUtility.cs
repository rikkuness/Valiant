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
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
    public static class SelectableUtility
    {
        /// <summary>
        /// Returns true if any input field in the scene currently has keyboard focus.
        /// </summary>
        public static bool isInputFieldFocused
        {
            get
            {
                for (int i = 0; i < Selectable.allSelectables.Count; i++)
                {
                    if (Selectable.allSelectables[i] is InputField && (Selectable.allSelectables[i] as InputField).isFocused)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
#endif
