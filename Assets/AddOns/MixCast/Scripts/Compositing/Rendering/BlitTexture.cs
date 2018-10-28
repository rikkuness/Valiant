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

namespace BlueprintReality.MixCast
{
    public class BlitTexture
    {
        public Texture2D Texture { get; set; }
        public Material Material { get; set; }

        public const float sizeProportion = 0.1f;

        public const float offsetProportion = 0.02f;

        public enum Position { BottomLeft, BottomRight, Middle };
        public Position texturePosition = Position.BottomLeft;

        public bool fullSize = false;

        public void SetTexturePosition (Position newPosition)
        {
            texturePosition = newPosition;
        }

        public BlitTexture()
        {
        }

        /// <summary>
        /// Info below is important, but not longer relevant here after greatly simplifying the original implementation of this class -
        /// Immediate camera and Buffered camera have different origin for RenderTexture.
        /// Immediate origin is in the center of its RenderTexture, Buffered is top left corner (the same as Quandrant).
        /// </summary>
        /// 
        /// <returns>Rect for DrawTexture.</returns>
        private Rect CalculateRect(MixCastCamera camera, RenderTexture renderTex, bool drawFullSize)
        {
            Vector2 renderSize = new Vector2(renderTex.width, renderTex.height);
            Vector2 watermarkSize = GetWatermarkSize(renderSize, drawFullSize);
            Vector2 offset = GetOffset(renderSize);

            switch (texturePosition)
            {
                case Position.BottomLeft:
                    return new Rect(offset.x, renderSize.y - watermarkSize.y - offset.y, watermarkSize.x, watermarkSize.y);
                case Position.BottomRight:
                    return new Rect(renderSize.x - watermarkSize.x - offset.x, renderSize.y - watermarkSize.y - offset.y, watermarkSize.x, watermarkSize.y);
                case Position.Middle:
                    return new Rect( renderSize.x * 0.5f - watermarkSize.x * 0.5f, renderSize.y * 0.5f - watermarkSize.y * 0.5f, watermarkSize.x, watermarkSize.y );
                default:
                    return new Rect(offset.x, renderSize.y - watermarkSize.y - offset.y, watermarkSize.x, watermarkSize.y);
            }
        }

        private Vector2 GetOffset(Vector2 renderSize)
        {
            float scaleX = offsetProportion * renderSize.x;
            float scaleY = offsetProportion * renderSize.y;
            
            float actualScale = Mathf.Min(scaleX, scaleY);

            return new Vector2(actualScale, actualScale);
        }

        private Vector2 GetWatermarkSize(Vector2 renderSize, bool drawFullSize = false)
        {
            float parentWidth = drawFullSize ? renderSize.x : sizeProportion * renderSize.x;
            float parentHeight = drawFullSize ? renderSize.y : sizeProportion * renderSize.y;
            float scaleWidth = parentWidth / (float)Texture.width;
            float scaleHeight = parentHeight / (float)Texture.height;

            float actualScale = Mathf.Min(scaleWidth, scaleHeight);
            float actualWidth = actualScale * Texture.width;
            float actualHeight = actualScale * Texture.height;

            return new Vector2(actualWidth, actualHeight);
        }

        public void ApplyToFrame( MixCastCamera camera ) {
            if (Texture == null)
            {
                return;
            }

            var renderTex = camera.Output as RenderTexture;

            if (!renderTex)
            {
                Debug.LogError("Attempting to blit texture to null render texture.");
                return;
            }

            Rect rect = CalculateRect(camera, renderTex, fullSize);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, renderTex.width, renderTex.height, 0);
            Graphics.SetRenderTarget(renderTex);
            bool oldSRGB = GL.sRGBWrite;
            GL.sRGBWrite = true;
            if (Material != null)
                Graphics.DrawTexture(rect, Texture, Material);
            else
                Graphics.DrawTexture(rect, Texture);
            GL.sRGBWrite = oldSRGB;
            GL.End();
            GL.PopMatrix();
        }
    }
}
#endif
