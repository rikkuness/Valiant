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

namespace BlueprintReality.MixCast.Utility {
    public class RunWithDelay: MonoBehaviour {
        static RunWithDelay _instance = null;

        public static RunWithDelay Instance() {
            if(_instance == null) {
                _instance = new GameObject().AddComponent<RunWithDelay>();
                DontDestroyOnLoad( _instance.gameObject );
                _instance.name = "RunWithDelay";
            }
            return _instance;
        }

        public static void RunCoroutine( IEnumerator doThis ) {
            Instance().StartCoroutine( doThis );
        }

        public static void NextFrame( System.Action doThis ) {
            Instance().StartCoroutine( Instance().RunNextFrameCoroutine( doThis ) );
        }

        private IEnumerator RunNextFrameCoroutine( System.Action coroutine ) {
            yield return new WaitForFixedUpdate();
            coroutine();
        }
    }
}
