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
    public class GameObjectStack : MonoBehaviour {
        public string id = "";

        public List<GameObjectStackElement> stack = new List<GameObjectStackElement>();

        public void SpawnObject(GameObject prefab)
        {
            prefab.SetActive(false);
            GameObject instance = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, transform);
            if( instance.transform is RectTransform )
            {
                RectTransform rect = instance.transform as RectTransform;
                RectTransform parentRect = GetComponentInParent<RectTransform>();
                if (parentRect != null)
                {
                    rect.transform.localPosition = Vector3.zero;
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentRect.rect.width);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentRect.rect.height);
                }
            }
            prefab.SetActive(true);

            GameObjectStackElement element = instance.GetComponent<GameObjectStackElement>();
            if (element == null)
                element = instance.AddComponent<GameObjectStackElement>();

            stack.Add(element);

            instance.SetActive(true);
        }
        public void PopTopElement()
        {
            if (stack.Count == 0)
                return;
            GameObjectStackElement instance = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            Destroy(instance.gameObject);
        }

        public void RemoveElement( GameObject element ) {
            if(element == null) {
                return;
            }
            GameObjectStackElement instance = element.GetComponent<GameObjectStackElement>();
            if(instance == null) {
                return;
            }
            int index = stack.FindIndex( se => se == instance );
            if(index != -1) {
                stack.RemoveAt( index );
                Destroy( instance.gameObject );
            }
        }
    }
}
