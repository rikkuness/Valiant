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
using UnityEngine.Rendering;

namespace BlueprintReality.MixCast
{
    public class ImmediateMixCastCamera : MixCastCamera
    {
        //Simple MixedRealityCamera that renders the game camera into the Output. Additional logic can be attached to the game camera as CommandBuffers in order to insert the real feed
        private Material postBlit;
        private CommandBuffer postBuff;

        private RenderingPath lastRenderPath;

        protected override void Awake()
        {
            base.Awake();
        
            postBlit = new Material(Shader.Find("Hidden/BPR/AlphaWrite"));
            postBuff = new CommandBuffer();
            postBuff.Blit(null, BuiltinRenderTextureType.CameraTarget, postBlit);
        }
        private void OnDestroy()
        {
            postBuff.Dispose();
            postBuff = null;
            postBlit = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            gameCamera.AddCommandBuffer(CameraEvent.AfterEverything, postBuff);
        }
        protected override void OnDisable()
        {
            gameCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, postBuff);

            base.OnDisable();
        }

        public override void RenderScene()
        {
            StartFrame();

            //Update shader properties for Feeds to access
            Shader.SetGlobalFloat("_CamNear", gameCamera.nearClipPlane);
            Shader.SetGlobalFloat("_CamFar", gameCamera.farClipPlane);
            Shader.SetGlobalMatrix("_CamToWorld", gameCamera.cameraToWorldMatrix);
            Shader.SetGlobalMatrix("_WorldToCam", gameCamera.worldToCameraMatrix);
            Shader.SetGlobalMatrix("_CamProjection", gameCamera.projectionMatrix);

            for (int i = 0; i < InputFeed.ActiveFeeds.Count; i++)
                if (InputFeed.ActiveFeeds[i].context.Data == context.Data && InputFeed.ActiveFeeds[i].Texture != null )
                    InputFeed.ActiveFeeds[i].StartRender();
            RenderGameCamera(gameCamera, Output as RenderTexture);
            for (int i = 0; i < InputFeed.ActiveFeeds.Count; i++)
                    if (InputFeed.ActiveFeeds[i].context.Data == context.Data && InputFeed.ActiveFeeds[i].Texture != null)
                        InputFeed.ActiveFeeds[i].StopRender();

            CompleteFrame();
        }
    }
}
#endif
