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
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
    [RequireComponent(typeof(RawImage))]
    public class FitRawImage : MonoBehaviour
    {
        public enum Mode
        {
            FitToParent = 0,
        }

        public Mode mode;

        public void Fit()
        {
            var image = GetComponent<RawImage>();

            if (image.texture == null)
            {
                return;
            }

            switch (mode)
            {
                case Mode.FitToParent:

                    FitToParent(image);

                    break;
            }
        }

        public void FitToParent(RawImage image)
        {
            float texAspect = 1f * image.texture.width / image.texture.height;

            var parentRect = transform.parent.GetComponent<RectTransform>();
            if (parentRect == null)
            {
                return;
            }

            var imageRect = image.rectTransform;

            float parentAspect = parentRect.rect.width / parentRect.rect.height;

            Vector2 size;
            Vector3 pos = imageRect.position;

            if (texAspect >= parentAspect)
            {
                size.x = parentRect.rect.width;
                size.y = parentRect.rect.width / texAspect;

                pos.y += (parentRect.rect.height - size.y) / 2f;
            }
            else
            {
                size.x = parentRect.rect.height * texAspect;
                size.y = parentRect.rect.height;

                pos.x += (parentRect.rect.width - size.x) / 2f;
            }

            imageRect.position = pos;
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
    }
}
#endif
