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
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCastCamera : CameraComponent
    {
        public static List<MixCastCamera> ActiveCameras { get; protected set; }
        public static MixCastCamera Current { get; protected set; } //Assigned to the MixCastCamera that is being processed between FrameStarted and FrameEnded

        public static MixCastCamera FindCamera(CameraConfigContext context)
        {
            foreach(var cam in ActiveCameras)
            {
                if(cam.context.Data == context.Data)
                {
                    return cam;
                }
            }
            return null;
        }

        public static event System.Action<MixCastCamera> FrameStarted;
        public static event System.Action<MixCastCamera> FrameEnded;

        public static event System.Action<MixCastCamera> GameRenderStarted;
        public static event System.Action<MixCastCamera> GameRenderEnded;

        public Transform displayTransform;
        public Camera gameCamera;

        public Texture Output { get; protected set; }

        public bool IsInUse
        {
            get
            {
                if (context.Data == null || context.Data.unplugged)
                    return false;
                return MixCast.Desktop.CameraInUse(context.Data) || MixCast.RecordingCameras.Contains(context.Data) || MixCast.StreamingCameras.Contains(context.Data);
            }
        }

        private bool IsInUseElsewhere
        {
            get { return context != null && context.Data != null && context.Data.isInUseElsewhere; }
        }

        private bool IsPluggedIn
        {
            get { return context != null && context.Data != null && !context.Data.unplugged; }
        }

        private int width;
        private int height;

        private static BlitTexture noCamBlit;

        static MixCastCamera()
        {
            ActiveCameras = new List<MixCastCamera>();
        }

        protected virtual void Awake()
        {
            noCamBlit = new BlitTexture();
            noCamBlit.SetTexturePosition( BlitTexture.Position.Middle );
            noCamBlit.Material = new Material( Shader.Find( "Hidden/MixCast/Watermark" ) );
            noCamBlit.Texture = Resources.Load<Texture2D>( "icon_camera_missing" );
            noCamBlit.fullSize = true;
        }

        protected override void OnEnable()
        {
            if (gameCamera != null)
            {
                gameCamera.stereoTargetEye = StereoTargetEyeMask.None;
                gameCamera.enabled = false;
            }

            base.OnEnable();

            if( context.Data != null )
            {
                if (string.IsNullOrEmpty(context.Data.deviceName))
                {
                    context.Data.deviceUseAutoFoV = false; // resolve edge case where someone with old 1.5.2 data with autofov = true and no cam input, would see auto fov enabled by default on first virtual camera created in 2.0
                }
                BuildOutput();
            }
                
            HandleDataChanged();

            ActiveCameras.Add(this);

            if(context.Data != null) {
                context.Data.unplugged = !FeedDeviceManager.IsVideoDeviceConnected( context.Data.deviceAltName );
            }
        }
        protected override void OnDisable()
        {
            ReleaseOutput();

            ActiveCameras.Remove(this);

            base.OnDisable();

            if (gameCamera != null)
                gameCamera.enabled = true;
        }

        protected override void HandleDataChanged()
        {
            base.HandleDataChanged();

            if (context.Data == null)
                return;

            if (context.Data.deviceFoV > 0 && gameCamera != null)
                gameCamera.fieldOfView = context.Data.deviceFoV;

            UpdateOutputDimensions();

            if (MixCast.RecordingCameras.Count <= 0 && MixCast.StreamingCameras.Count <= 0)
            {
                if (width != Output.width || height != Output.height)
                {
                    ReleaseOutput();
                    BuildOutput();
                }
            }
        }

        protected virtual void LateUpdate()
        {
            HandleDataChanged();
        }

        protected virtual void BuildOutput()
        {
            bool isHdr = false;
            if (gameCamera != null)
            {
#if UNITY_5_6_OR_NEWER
                isHdr = gameCamera.allowHDR;
#else
                isHdr = gameCamera.hdr;
#endif
            }

            UpdateOutputDimensions();

            Output = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                antiAliasing = CalculateAntiAliasingValue(),
                autoGenerateMips = false,
                useMipMap = false,
            };
        }

        protected void UpdateOutputDimensions()
        {
            width = CalculateOutputWidth();
            height = CalculateOutputHeight();
        }

        protected virtual void ReleaseOutput()
        {
            if (Output != null)
            {
                (Output as RenderTexture).Release();
                Output = null;
            }
        }

        protected int CalculateOutputWidth()
        {

            var outputWidth = context.Data.outputWidth;

            // use screen size if window size is selected in output menu dropdown
            if (context.Data.outputResolution == MixCastData.OutputResolution.WindowSize)
            {
                outputWidth = Screen.width; // required so that output feed scales when window size changes
                context.Data.outputWidth = outputWidth; // required so that Output menu correctly identifies output resolution as window size after rescaling
            }

            if (outputWidth <= 0)
            {
                var outputHeight = context.Data.outputHeight;

                outputWidth = outputHeight > 0 ?
                    Mathf.RoundToInt((float)outputHeight * Screen.width / Screen.height) :
                    Screen.width;
            }

            // Limit output resolution of free licensees to 720p.
            var maxWidth = MixCast.SecureSettings.IsFreeLicense ? MixCastData.CameraCalibrationData.MAX_WIDTH_FREE : int.MaxValue;
            return Mathf.Min(outputWidth, maxWidth);
        }

        protected int CalculateOutputHeight()
        {
            var outputHeight = context.Data.outputHeight;

            // use screen size if window size is selected in output menu dropdown
            if (context.Data.outputResolution == MixCastData.OutputResolution.WindowSize)
            {
                outputHeight = Screen.height; // required so that output feed scales when window size changes
                context.Data.outputHeight = outputHeight; // required so that Output menu correctly identifies output resolution as window size after rescaling
            }

            if (outputHeight <= 0)
            {
                var outputWidth = context.Data.outputWidth;

                outputHeight = outputWidth > 0 ?
                    Mathf.RoundToInt((float)outputWidth * Screen.height / Screen.width) :
                    Screen.height;
            }

            // Limit output resolution of free licensees to 720p.
            var maxHeight = MixCast.SecureSettings.IsFreeLicense ? MixCastData.CameraCalibrationData.MAX_HEIGHT_FREE : int.MaxValue;
            return Mathf.Min(outputHeight, maxHeight);
        }

        protected int CalculateAntiAliasingValue()
        {
#if UNITY_5_6_OR_NEWER
            if (gameCamera != null && !gameCamera.allowMSAA)
                return 1;
#endif
            if (gameCamera.actualRenderingPath == RenderingPath.DeferredShading)
                return 1;

            if (MixCast.ProjectSettings.overrideQualitySettingsAA)
                return 1 << MixCast.ProjectSettings.overrideAntialiasingVal;    //saved as 2^x
            else
                return QualitySettings.antiAliasing;
        }

        public virtual void RenderScene()
        {

        }

        #region Event Firing
        protected void RenderGameCamera( Camera cam, RenderTexture target ) {
            if( GameRenderStarted != null )
                GameRenderStarted( this );

            cam.targetTexture = target;
            cam.aspect = (float)target.width / target.height;
            cam.Render();

            if (GameRenderEnded != null)
                GameRenderEnded(this);
            cam.targetTexture = null;
        }

		protected void StartFrame()
        {
            Current = this;
            if ( FrameStarted != null )
                FrameStarted(this);
        }

        protected void CompleteFrame()
        {
            if (!IsPluggedIn || IsInUseElsewhere) {
                noCamBlit.ApplyToFrame(this);
            }

            Current = null;
            
            if (FrameEnded != null)
                FrameEnded(this);

            Graphics.SetRenderTarget(null);
        }
        #endregion
    }
}
#endif
