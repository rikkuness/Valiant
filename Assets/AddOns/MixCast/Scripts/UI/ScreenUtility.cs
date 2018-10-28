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
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlueprintReality.Utility
{
    public class ScreenUtility
    {

        public static GameObject FindObjectInTopScene(string optionalTargetParentName = "")
        {
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                GameObject[] gameObjects = scene.GetRootGameObjects();

                if (!string.IsNullOrEmpty(optionalTargetParentName))
                {
                    for (int j = 0; j < gameObjects.Length; j++)
                    {
                        if (gameObjects[j].activeInHierarchy)
                        {

#if UNITY_5_6_OR_NEWER
                            Transform t = gameObjects[j].transform.Find(optionalTargetParentName);
#else
                            Transform t = gameObjects[j].transform.FindChild(optionalTargetParentName);
#endif

                            if (t != null)
                            {
                                return t.gameObject;
                            }
                        }
                    }
                }
                else if (gameObjects.Length > 0)
                {
                    return gameObjects[0];
                }
            }

            // no gameobject found if we're here        
            return null;
        }

        public static GameObject InstantiateInTopScene(GameObject prefab, string optionalTargetParentName = "")
        {
            GameObject target = ScreenUtility.FindObjectInTopScene(optionalTargetParentName);

            GameObject newObj = GameObject.Instantiate<GameObject>(prefab, target != null ? target.transform : null, target == null);
            if (string.IsNullOrEmpty(optionalTargetParentName))
            {
                newObj.transform.SetParent(null);
            }
            newObj.transform.SetAsLastSibling();
            return newObj;
        }

        public static bool AreAllScenesLoaded()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isLoaded == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
#endif
