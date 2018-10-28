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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
    public class SetActiveFromToggle : MonoBehaviour
    {
        public Toggle toggle;

        public List<GameObject> on = new List<GameObject>();
        public List<GameObject> off = new List<GameObject>();

        private void OnEnable()
        {
            toggle.onValueChanged.AddListener(HandleValueChanged);
            HandleValueChanged(toggle.isOn);
        }

        private void HandleValueChanged(bool val)
        {
            on.ForEach(g => g.SetActive(val));
            off.ForEach(g => g.SetActive(!val));
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(HandleValueChanged);
        }
    }
}
