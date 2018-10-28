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
    public class SetImageFromMixCastOutput : CameraComponent
    {
        public ScaleType scaleType = ScaleType.ScaleAlongHorizontal;

        public enum ScaleType
        {
            None,
            ScaleAlongHorizontal,
            Inset
        }

        private MixCastCamera cam;
        private RawImage image;
        private AspectRatioFitter fitter;

        protected override void OnEnable()
        {
            image = GetComponent<RawImage>();
            fitter = GetComponent<AspectRatioFitter>();
            if( fitter == null && scaleType != ScaleType.None )
            {
                fitter = gameObject.AddComponent<AspectRatioFitter>();
                if (scaleType == ScaleType.ScaleAlongHorizontal)
                    fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                else //if (scaleType == ScaleType.Inset)
                    fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            }

            base.OnEnable();

            HandleDataChanged();
        }


        private void LateUpdate()
        {
            if (cam == null || cam.context.Data != context.Data || !cam.isActiveAndEnabled)
                cam = MixCastCamera.FindCamera(context);

            if (cam != null)
            {
                image.texture = cam.Output;
                //Comment out to remove memory allocation in editor
                //image.SetMaterialDirty();

                if (image.texture != null)
                {
                    if (fitter != null)
                        fitter.aspectRatio = (float)image.texture.width / image.texture.height;
                }
            }
            else
                image.texture = null;
        }
    }
}
#endif
