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

using UnityEngine;

namespace BlueprintReality.Events
{
    public class ShowHideFromEvent : MonoBehaviour
    {
        public GameObject[] targets;

        public void ShowHide()
        {
            foreach (var target in targets)
            {
                target.SetActive(!target.activeInHierarchy);
            }
        }
    }
}
