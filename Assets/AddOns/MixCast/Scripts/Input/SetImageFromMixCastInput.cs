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
    public class SetImageFromMixCastInput : CameraComponent
    {
        public bool takeProcessed = false;
        public bool setScale = false;

        private InputFeed feed;
        private RawImage image;
        private AspectRatioFitter fitter;

        protected override void OnEnable()
        {
            image = GetComponent<RawImage>();
            fitter = GetComponent<AspectRatioFitter>();
            if (fitter == null && setScale)
            {
                //Fallback to legacy behaviour which is equivalent to Height Controls Width
                fitter = gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            }

            base.OnEnable();

            HandleDataChanged();
        }


        private void LateUpdate()
        {
            if (feed == null || !feed.isActiveAndEnabled || feed.context.Data != context.Data || feed.Texture == null)
            {
                feed = InputFeed.FindInputFeed(context);
            }

            if (feed != null)
            {
                image.texture = takeProcessed ? feed.ProcessedTexture : feed.Texture;

                if (takeProcessed)
                    image.uvRect = new Rect(0, 0, 1, 1);
                else
                    image.uvRect = new Rect(feed.FlipX ? 1 : 0, feed.FlipY ? 1 : 0, feed.FlipX ? -1 : 1, feed.FlipY ? -1 : 1);

                image.SetMaterialDirty();

                if (fitter != null && image.texture != null)
                {
                    fitter.aspectRatio = (float)image.texture.width / image.texture.height;
                }
            }
            else
                image.texture = null;
        }
    }
}
#endif
