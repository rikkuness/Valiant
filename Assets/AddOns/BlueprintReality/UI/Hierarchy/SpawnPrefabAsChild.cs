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

namespace BlueprintReality.UI {
	public class SpawnPrefabAsChild : MonoBehaviour {
        public GameObject prefab;

        public bool Spawned { get; protected set; }

        protected void Awake()
        {
            if (Spawned)
                return;
            SpawnPrefabAsChild[] others = GetComponents<SpawnPrefabAsChild>();
            int index = System.Array.IndexOf(others, this);
            for( int i = 0; i < index; i++ )
            {
                if (!others[i].Spawned)
                    return;
            }
            GameObject instance = Instantiate(prefab, transform.position, transform.rotation, transform);
            instance.name = prefab.name;
            instance.transform.localScale = prefab.transform.localScale;
            Spawned = true;
            if (index < others.Length - 1)
                others[index + 1].Awake();
        }
    }
}
