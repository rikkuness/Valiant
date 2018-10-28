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
using System.Collections;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class SetTextFromFPS : MonoBehaviour
    {
        public float updateFrequency = 0.5f;

        public string formatStr = "{00:0}";

        private void OnEnable()
        {
            StartCoroutine(Run());
        }
        private void OnDisable()
        {
            StopCoroutine("Run");
        }


        IEnumerator Run()
        {
            UnityEngine.UI.Text text = GetComponent<UnityEngine.UI.Text>();
            while(true)
            {
                int lastFrameCount = Time.frameCount;
                float lastTime = Time.unscaledTime;
                yield return new WaitForSecondsRealtime(updateFrequency);
                int framesElapsed = Time.frameCount - lastFrameCount;
                float timeElapsed = Time.unscaledTime - lastTime;
                text.text = string.Format(formatStr, (float)framesElapsed / timeElapsed);
            }
        }
    }
}
#endif
