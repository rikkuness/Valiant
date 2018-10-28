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
	public class PushToGameObjectStack : MonoBehaviour {
        public GameObject prefab;
        public bool popFirst = false;
        public string stackId = "";
        
        public void PushElement()
        {
            GameObjectStack stack = FindStack();
            if (stack == null)
                return;

            if (popFirst)
                stack.PopTopElement();
            stack.SpawnObject(prefab);
        }

        protected GameObjectStack FindStack()
        {
            Transform stackSearchObj = transform;
            while (stackSearchObj != null)
            {
                GameObjectStack foundStack = stackSearchObj.GetComponent<GameObjectStack>();
                if (foundStack != null && (string.IsNullOrEmpty(stackId) || stackId == foundStack.id))
                    return foundStack;
                stackSearchObj = stackSearchObj.parent;
            }
            // not found, so look elsewhere
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for(int i = 0; i < sceneCount; i++) {
                GameObject[] goList = UnityEngine.SceneManagement.SceneManager.GetSceneAt( i ).GetRootGameObjects();
                if(goList != null && goList.Length > 0) {
                    for(int j = 0; j < goList.Length; j++) {
                        GameObjectStack[] stacks = goList[j].GetComponentsInChildren<GameObjectStack>();
                        foreach(var stack in stacks) {
                            if(stack.id == stackId) {
                                return stack;
                            }
                        }
                    }
                }
            }
            return null;
        }
	}
}
