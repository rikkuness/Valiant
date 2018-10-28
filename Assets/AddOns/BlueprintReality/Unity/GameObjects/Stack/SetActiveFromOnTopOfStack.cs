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

namespace BlueprintReality.GameObjects
{
	public class SetActiveFromOnTopOfStack : MonoBehaviour {
        public string stackId = "";

        public List<GameObject> active = new List<GameObject>();
        public List<GameObject> inactive = new List<GameObject>();

        private GameObjectStack stack;
        private GameObjectStackElement element;

        private bool lastState;

        private void OnEnable()
        {
            FindStack();
            ApplyState(CalculateNewState());
        }
        private void Update()
        {
            bool newState = CalculateNewState();
            if (newState != lastState)
                ApplyState(newState);
        }

        bool CalculateNewState()
        {
            if (stack == null || stack.stack.Count == 0)
                return true;
            return stack.stack[stack.stack.Count - 1] == element;
        }
        void ApplyState(bool newState)
        {
            active.ForEach(g => g.SetActive(newState));
            inactive.ForEach(g => g.SetActive(!newState));
            lastState = newState;
        }

        void FindStack()
        {
            Transform elementSearchObj = transform;
            Transform stackSearchObj = elementSearchObj.parent;
            while (stack == null && stackSearchObj != null)
            {
                GameObjectStack foundStack = stackSearchObj.GetComponent<GameObjectStack>();
                if (foundStack != null && (string.IsNullOrEmpty(stackId) || stackId == foundStack.id))
                {
                    stack = foundStack;
                    element = elementSearchObj.GetComponent<GameObjectStackElement>();
                    return;
                }

                elementSearchObj = stackSearchObj;
                stackSearchObj = elementSearchObj.parent;
            }
        }
	}
}
