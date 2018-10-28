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
using UnityEngine;

namespace BlueprintReality.UI
{
    public class SetVertScrollbarPosOnEnable : MonoBehaviour
    {

        [SerializeField] float yPos = 1.0f;

        void OnEnable()
        {
            StartCoroutine(ScrollTo(yPos));
        }

        IEnumerator ScrollTo(float pos)
        {
            yield return new WaitForEndOfFrame();
            UnityEngine.UI.Scrollbar[] scrollbars = GetComponents<UnityEngine.UI.Scrollbar>();
            for (int i = 0; i < scrollbars.Length; i++)
            {
                switch (scrollbars[i].direction)
                {
                    case UnityEngine.UI.Scrollbar.Direction.BottomToTop: scrollbars[i].value = yPos; break;
                    case UnityEngine.UI.Scrollbar.Direction.TopToBottom: scrollbars[i].value = 1.0f - yPos; break;
                    default:
                        scrollbars[i].value = yPos; break;

                }
            }
        }
    }
}
#endif
