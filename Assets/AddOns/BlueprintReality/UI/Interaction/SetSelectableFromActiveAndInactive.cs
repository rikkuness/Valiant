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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintReality.UI
{
    public class SetSelectableFromActiveAndInactive : MonoBehaviour
    {
        public Selectable button;

        public List<GameObject> activeTargets = new List<GameObject>();
        public List<GameObject> inactiveTargets = new List<GameObject>();
        public bool all = false;

        private void OnEnable()
        {
            if (button == null)
                button = GetComponent<Selectable>();

            Update();
        }

        void Update()
        {
            button.interactable = all ? activeTargets.Find(t => t.activeSelf == false) == null : activeTargets.Find(t => t.activeSelf == true) != null;
            button.interactable = all ? inactiveTargets.Find(t => t.activeSelf == false) == null : inactiveTargets.Find(t => t.activeSelf == false) != null;
        }
    }
}
