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
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BlueprintReality.MixCast
{
    public class InputFeed : CameraComponent
    {
        public class FramePlayerData
        {
            public float playerDist;

            public Vector3 playerHeadPos;
            public Vector3 playerBasePos;
            public Vector3 playerLeftHandPos;
            public Vector3 playerRightHandPos;
        }

        private const string KEYWORD_CROP_PLAYER = "CROP_PLAYER";
        private const string KEYWORD_FLIP_X = "PXL_FLIP_X";
        private const string KEYWORD_FLIP_Y = "PXL_FLIP_Y";

        protected virtual float POLLING_DELAY { get { return 0.5f; } }
        protected virtual float POLLING_MIN_CLEAR_WAIT { get { return 1f; } }

        public static List<InputFeed> ActiveFeeds = new List<InputFeed>();
        public static InputFeed FindInputFeed(CameraConfigContext context)
        {
            for (int i = 0; i < ActiveFeeds.Count; i++)
            {
                var feed = ActiveFeeds[i];
                if (feed.context.Data == context.Data)
                {
                    return feed;
                }
            }
            return null;
        }
        public static event Action<InputFeed> OnProcessInputStart;
        public static event Action<InputFeed> OnProcessInputEnd;

        public virtual bool ShouldRender { get { return wasCameraConnected; } }


        public Vector3 playerHeadsetOffset = new Vector3(0, 0, -0.05f);     //HMD tracked point about 5cm in front of center of skull

        public Material blitMaterial;

        public virtual Texture Texture
        {
            get
            {
                return null;
            }
        }

        public bool FlipX
        {
            get
            {
                return context.Data.deviceData.flipX;
            }
        }
        public bool FlipY
        {
            get
            {
                return SrcHasFlippedY != context.Data.deviceData.flipY;
            }
        }
        protected virtual bool SrcHasFlippedY
        {
            get
            {
                return false;
            }
        }

        private Material processTextureMat;
        public Material ProcessTextureMaterial
        {
            get
            {
                return processTextureMat;
            }
        }


        private RenderTexture processedTexture;
        public RenderTexture ProcessedTexture
        {
            get
            {
                return processedTexture;
            }
        }

        public static event Action<InputFeed> OnBeforeProcessTexture;
        public static event Action<InputFeed> OnAfterProcessTexture;

        public event Action OnRenderStarted;
        public event Action OnRenderEnded;

        private Material setMaterial;
        protected FrameDelayQueue<FramePlayerData> frames;


        protected float timeSinceLastDevicePoll = 0f;
        protected bool wasCameraConnected = true;
        protected bool isResettingCamera = false;
        protected MixCastData.OutputMode lastValidMode = MixCastData.OutputMode.Immediate;

        protected virtual void Awake()
        {
            processTextureMat = new Material(Shader.Find("Hidden/MixCast/Process Input"));

            frames = null;
        }

        protected override void OnEnable()
        {
            frames = new FrameDelayQueue<FramePlayerData>();

            ActiveFeeds.Add(this);

            base.OnEnable();
            Invoke("HandleDataChanged", 0.01f);
        }

        protected override void OnDisable()
        {
            ActiveFeeds.Remove(this);

            frames = null;

            ClearTexture();
            base.OnDisable();
        }

        private void OnApplicationQuit()
        {
            frames = null;
            ClearTexture();
        }

        protected virtual void LateUpdate()
        {
            if(context.Data == null) {
                return;
            }

            timeSinceLastDevicePoll += Time.deltaTime;
            if( timeSinceLastDevicePoll >= POLLING_DELAY + (isResettingCamera ? POLLING_MIN_CLEAR_WAIT : 0f) ) {

                bool isConnected = FeedDeviceManager.IsVideoDeviceConnected(context.Data.deviceAltName);
                if( wasCameraConnected && !isConnected ) {
                    isResettingCamera = true;
                    ClearTexture();
                    context.Data.unplugged = true;
                    lastValidMode = context.Data.outputMode;
                    context.Data.outputMode = MixCastData.OutputMode.Immediate;
                }
                else if( !wasCameraConnected && isConnected ) {
                    timeSinceLastDevicePoll = 0f;
                    FeedDeviceManager.BuildDeviceList();
                    SetTexture();
                    context.Data.unplugged = false;
                    isResettingCamera = false;
                    context.Data.outputMode = lastValidMode;
                } else {
                    timeSinceLastDevicePoll = 0f;
                }
                wasCameraConnected = isConnected;
            }
        }

        public virtual void StartRender()
        {
            if (context.Data == null || frames == null)
                return;

            frames.delayDuration = (context.Data.unplugged == false && context.Data.outputMode == MixCastData.OutputMode.Buffered) ? context.Data.bufferTime : 0;
            frames.Update();

            ProcessTexture();

            if (blitMaterial != null && ProcessedTexture != null)
            {
                blitMaterial.mainTexture = ProcessedTexture;

                if (context.Data.croppingData.active)
                    blitMaterial.EnableKeyword(KEYWORD_CROP_PLAYER);

                FramePlayerData oldFrameData = frames.OldestFrameData;
                if (oldFrameData != null)
                {
                    blitMaterial.SetFloat("_PlayerDist", oldFrameData.playerDist);
                    blitMaterial.SetVector("_PlayerHeadPos", oldFrameData.playerHeadPos);
                    blitMaterial.SetVector("_PlayerLeftHandPos", oldFrameData.playerLeftHandPos);
                    blitMaterial.SetVector("_PlayerRightHandPos", oldFrameData.playerRightHandPos);
                    blitMaterial.SetVector("_PlayerBasePos", oldFrameData.playerBasePos);
                }

                float scale = MixCastCameras.Instance.RoomTransform != null ? MixCastCameras.Instance.RoomTransform.TransformVector(Vector3.forward).magnitude : 1;
                blitMaterial.SetFloat("_PlayerScale", scale);

                blitMaterial.SetFloat("_PlayerHeadCropRadius", context.Data.croppingData.headRadius);
                blitMaterial.SetFloat("_PlayerHandCropRadius", context.Data.croppingData.handRadius);
                blitMaterial.SetFloat("_PlayerFootCropRadius", context.Data.croppingData.baseRadius);

                //update the player's depth for the material
                FrameDelayQueue<FramePlayerData>.Frame<FramePlayerData> nextFrame = frames.GetNewFrame();
                if (nextFrame.data == null)
                    nextFrame.data = new FramePlayerData();

                FillTrackingData(nextFrame);
            }

            if (OnRenderStarted != null)
                OnRenderStarted();
        }

        public virtual void StopRender()
        {
            if (context.Data.croppingData.active)
                blitMaterial.DisableKeyword(KEYWORD_CROP_PLAYER);

            if (OnRenderEnded != null)
                OnRenderEnded();
        }

        protected virtual void SetTexture()
        {

        }

        protected virtual void ClearTexture()
        {

        }

        protected virtual void ProcessTexture( RenderTextureFormat format = RenderTextureFormat.ARGB32 )
        {
            if (processedTexture != null && Texture != null)
            {
                if (processedTexture.width != Texture.width || processedTexture.height != Texture.height)
                {
                    processedTexture.Release();
                    processedTexture = null;
                }
            }
            if (processedTexture == null && Texture != null)
            {
                processedTexture = new RenderTexture(Texture.width, Texture.height, 0, format, RenderTextureReadWrite.sRGB);
                processedTexture.autoGenerateMips = processedTexture.useMipMap = false;
                processedTexture.wrapMode = TextureWrapMode.Clamp;
            }

            ProcessTextureMaterial.mainTexture = Texture;

            PrepareProcessMaterial(ProcessTextureMaterial);

            if (OnBeforeProcessTexture != null)
                OnBeforeProcessTexture(this);
            if (OnProcessInputStart != null)
                OnProcessInputStart(this);

            Graphics.SetRenderTarget(ProcessedTexture);
            GL.Clear(true, true, Color.clear);

            bool oldSRGB = GL.sRGBWrite;
            GL.sRGBWrite = false;
            Graphics.Blit(Texture, processedTexture, ProcessTextureMaterial);
            GL.sRGBWrite = oldSRGB;

            Graphics.SetRenderTarget(null);

            if (OnProcessInputEnd != null)
                OnProcessInputEnd(this);
            if (OnAfterProcessTexture != null)
                OnAfterProcessTexture(this);

            CleanupProcessMaterial(ProcessTextureMaterial);
        }

        //Returns the forward distance from the camera to the player plane in camera-space
        public float CalculatePlayerDistance(Camera cam)
        {
            float nearestDistance = (cam.nearClipPlane + 0.0000001f); // Add a fairly small value to avoid clipping, consider depth buffer precision when changed (Possibly 2^(−24) = 5.96e-08, not sure)
            Vector3 devicePos;
            Quaternion deviceRot;
            if (!string.IsNullOrEmpty(context.Data.actorTrackingDeviceId) && context.Data.actorTrackingDevice != "Hmd")
            {
                if (!TrackedDeviceManager.Instance.GetDeviceTransformByGuid(context.Data.actorTrackingDeviceId, out devicePos, out deviceRot))
                    return nearestDistance;
            }
            else
            {
                if (!TrackedDeviceManager.Instance.GetDeviceTransformByRole(TrackedDeviceManager.DeviceRole.Head, out devicePos, out deviceRot))
                    return nearestDistance;
                devicePos += deviceRot * playerHeadsetOffset;
            }

            Vector3 playerPositionWorld = MixCastCameras.Instance.RoomTransform.TransformPoint(devicePos);
            Vector3 playerPositionLocal = cam.transform.InverseTransformPoint(playerPositionWorld);
            return playerPositionLocal.z;
        }

        protected virtual void PrepareProcessMaterial(Material mat)
        {
            FramePlayerData oldFrameData = frames.OldestFrameData;
            if (oldFrameData != null)
                mat.SetFloat("_PlayerDist", oldFrameData.playerDist);

            if (FlipX)
                mat.EnableKeyword(KEYWORD_FLIP_X);
            if (FlipY)
                mat.EnableKeyword(KEYWORD_FLIP_Y);
        }

        protected virtual void CleanupProcessMaterial(Material mat)
        {
            if( context.Data.croppingData.active )
                processTextureMat.DisableKeyword(KEYWORD_CROP_PLAYER);

            if (FlipX)
                mat.DisableKeyword(KEYWORD_FLIP_X);
            if (FlipY)
                mat.DisableKeyword(KEYWORD_FLIP_Y);
        }

        void FillTrackingData(FrameDelayQueue<FramePlayerData>.Frame<FramePlayerData> frame)
        {
            MixCastCamera cam = MixCastCamera.FindCamera(context);
            if (cam != null && cam.gameCamera != null)
                frame.data.playerDist = cam.gameCamera.transform.TransformVector(Vector3.forward).magnitude * CalculatePlayerDistance(cam.gameCamera); //Scale distance by camera scale

            frame.data.playerHeadPos = GetTrackingPosition(TrackedDeviceManager.DeviceRole.Head);
            frame.data.playerBasePos = new Vector3(frame.data.playerHeadPos.x, 0, frame.data.playerHeadPos.z);
            frame.data.playerLeftHandPos = GetTrackingPosition(TrackedDeviceManager.DeviceRole.LeftHand);
            frame.data.playerRightHandPos = GetTrackingPosition(TrackedDeviceManager.DeviceRole.RightHand);

            if (MixCastCameras.Instance.RoomTransform != null)
            {
                Transform roomTransform = MixCastCameras.Instance.RoomTransform;
                frame.data.playerHeadPos = roomTransform.TransformPoint(frame.data.playerHeadPos);
                frame.data.playerBasePos = roomTransform.TransformPoint(frame.data.playerBasePos);
                frame.data.playerLeftHandPos = roomTransform.TransformPoint(frame.data.playerLeftHandPos);
                frame.data.playerRightHandPos = roomTransform.TransformPoint(frame.data.playerRightHandPos);
            }
        }
        Vector3 GetTrackingPosition(TrackedDeviceManager.DeviceRole role)
        {
            Vector3 pos;
            Quaternion rot;
            if (!TrackedDeviceManager.Instance.GetDeviceTransformByRole(role, out pos, out rot))
                TrackedDeviceManager.Instance.GetDeviceTransformByRole(TrackedDeviceManager.DeviceRole.Head, out pos, out rot);
            return pos;
        }
    }
}
#endif
