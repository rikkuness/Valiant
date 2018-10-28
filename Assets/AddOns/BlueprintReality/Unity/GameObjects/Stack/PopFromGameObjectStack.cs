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
	public class PopFromGameObjectStack : MonoBehaviour {
        public bool all = false;
        public string stackId = "";
        
        public void PopTopElement()
        {
            GameObjectStack stack = FindStack();
            if (stack == null)
                return;

            if (all)
            {
                while (stack.stack.Count > 0)
                    stack.PopTopElement();
            }
            else
                stack.PopTopElement();
        }

        GameObjectStack FindStack()
        {
            Transform stackSearchObj = transform;
            while (stackSearchObj != null)
            {
                GameObjectStack foundStack = stackSearchObj.GetComponent<GameObjectStack>();
                if (foundStack != null && (string.IsNullOrEmpty(stackId) || stackId == foundStack.id))
                    return foundStack;
                stackSearchObj = stackSearchObj.parent;
            }
            return null;
        }
    }
}
