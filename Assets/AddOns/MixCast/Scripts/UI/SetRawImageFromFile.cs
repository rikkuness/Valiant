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
using UnityEngine.Events;
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
    [RequireComponent(typeof(RawImage))]
    public class SetRawImageFromFile : MonoBehaviour
    {
        public string filepath;
        public bool setImageOnStart;
        public UnityEvent OnImageLoaded;

        void Start()
        {
            if (setImageOnStart)
            {
                LoadImage();
            }
        }

        public void LoadImage()
        {
            StartCoroutine(LoadImageAsync());
        }

        private IEnumerator LoadImageAsync()
        {
            if (string.IsNullOrEmpty(filepath))
            {
                Debug.LogError("No filepath set for raw texture!");

                yield break;
            }

            WWW www = new WWW("file://" + filepath);

            yield return www;

            if (!string.IsNullOrEmpty(www.error) || www.texture == null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                GetComponent<RawImage>().texture = www.texture;

                OnImageLoaded.Invoke();

                gameObject.SetActive(true);
            }
        }
    }
}
#endif
