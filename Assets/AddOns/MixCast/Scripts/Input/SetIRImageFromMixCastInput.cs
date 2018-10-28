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
using System.Threading;
using Intel.RealSense;


namespace BlueprintReality.MixCast
{
    [RequireComponent( typeof( RawImage ) )]
    public class SetIRImageFromMixCastInput: CameraComponent {
        public bool takeProcessed = false;
        public bool setScale = false;

        private RealSense.RealSenseInputFeed feed;
        private RawImage  image;

        private const string IRShaderName = "Hidden/MixCast/BW";

        protected override void OnEnable() {
            image = GetComponent<RawImage>();

            base.OnEnable();

            if( feed == null || !feed.isActiveAndEnabled || feed.context.Data != context.Data || feed.Texture == null ) {
                feed = (RealSense.RealSenseInputFeed)RealSense.RealSenseInputFeed.FindInputFeed( context );
            }

            if( feed != null ) {
                feed.ShowInfrared( true );
            }

            HandleDataChanged();
        }

        private void LateUpdate() {
            CheckMaterial();

            image.texture = feed.IRTexture;

            if( feed != null ) {
                image.uvRect = new Rect(feed.FlipX ? 1 : 0, feed.FlipY ? 1 : 0, feed.FlipX ? -1 : 1, feed.FlipY ? -1 : 1);
                image.SetMaterialDirty();

                if( setScale && image.texture != null ) {
                    image.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, (float)image.texture.width / image.texture.height * image.rectTransform.rect.height );
                }
            } else {
                image.texture = null;
            }
        }

        protected override void OnDisable() {
            if( feed == null || !feed.isActiveAndEnabled || feed.context.Data != context.Data || feed.Texture == null ) {
                feed = (RealSense.RealSenseInputFeed)RealSense.RealSenseInputFeed.FindInputFeed( context );
            }
            feed.ShowInfrared( false );
            base.OnDisable();
        }

        private void CheckMaterial()
        {
            if (image != null && !image.material.shader.name.Equals(IRShaderName))
            {
                Material infraredMat = new Material(Shader.Find(IRShaderName));
                infraredMat.SetFloat("Gamma", 0.45f);
                infraredMat.renderQueue = 3000;
                image.material = infraredMat;
            }
        }
    }
}
#endif
