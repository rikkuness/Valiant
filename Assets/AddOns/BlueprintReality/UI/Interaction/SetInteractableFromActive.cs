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

namespace BlueprintReality.UI
{
    public class SetInteractableFromActive : MonoBehaviour
    {
        public Selectable button;
        public List<GameObject> targets = new List<GameObject>();
        public bool all;

        void OnEnable()
        {
            if (button == null)
            {
                button = GetComponent<Selectable>();
            }
        }

        void Update()
        {
            button.interactable = CalculateNewState();
        }

        bool CalculateNewState()
        {
            if (all)
            {
                for (int i = 0; i < targets.Count; i++)
                    if (!targets[i].activeSelf)
                        return false;
                return true;
            }
            else
            {
                for (int i = 0; i < targets.Count; i++)
                    if (targets[i].activeSelf)
                        return true;
                return false;
            }
        }
    }
}
