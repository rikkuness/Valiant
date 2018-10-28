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
using System.Linq;
using UnityEngine;
using Intel.RealSense;

namespace BlueprintReality.MixCast.RealSense
{
    public class RealSenseInputFeed : InputFeed
    {
        public static readonly string[] DEVICE_NAMES = new string[]
        {
            "Intel(R) RealSense(TM) 415",
            "Intel(R) RealSense(TM) 435",
        };

        public const float DEFAULT_GAIN_VAL = 0.5f;
        public const float DEFAULT_EXPOSURE_VAL = 0.22702f;//0.0117f;
        public const float DEFAULT_INFRARED_EXPOSURE_VAL = 0.37106f;//0.05109f;
        public const float DEFAULT_WHITEBALANCE_VAL = 0.5f;

        private const int decimationAmount = 2;

        protected override float POLLING_MIN_CLEAR_WAIT { get { return 2f; } }

        public Texture RGBTexture   { get; protected set; }
        public Texture DepthTexture { get; protected set; }
        public Texture IRTexture    { get; set; }

        public override Texture Texture { get { return RGBTexture; } }

        public bool showInfrared = false;


        protected override bool SrcHasFlippedY
        {
            get
            {
                return true;
            }
        }

        public Sensor colorSensor { get; private set; }
        public Sensor depthSensor { get; private set; }
        public RealSenseDevice device { get; private set; }

        private int lastWidth, lastHeight;
        private string lastSerial;

        private GameObject deviceObj;
        private RealSenseStreamTexture colorTex;
        private RealSenseMixcastImages depthTex;
        private RealSenseStreamTexture irTex;

        private int depthWidth;
        private int depthHeight;

        public Material DepthMaskMaterial { get; private set; }
        private Material BlurMaterial;
        public Material InfraredMaterial;

        // Textures for rendering and blurring depth mask
        private RenderTexture DepthMask;
        private RenderTexture DepthMaskBlur;

        public override void StartRender()
        {
            base.StartRender();

            if (context.Data.subjectHasPixelDepth)
            {
                blitMaterial.SetTexture("_RsDepthMap", DepthTexture);
                blitMaterial.SetFloat("_RsDepthMultiplier", 100);
                blitMaterial.SetTexture("_BgStaticDepthMask", DepthMask);
            }

            if (FlipX)
                blitMaterial.EnableKeyword("PXL_FLIP_X");
            if (FlipY)
                blitMaterial.EnableKeyword("PXL_FLIP_Y");
        }
        public override void StopRender()
        {
            if (FlipX)
                blitMaterial.DisableKeyword("PXL_FLIP_X");
            if (FlipY)
                blitMaterial.DisableKeyword("PXL_FLIP_Y");

            base.StopRender();
        }

        protected override void ProcessTexture( RenderTextureFormat format = RenderTextureFormat.ARGB32 ) {
            if( showInfrared ) {
                base.ProcessTexture( RenderTextureFormat.R8 );
            }
            else {
                base.ProcessTexture( RenderTextureFormat.ARGB32 );
            }
        }

        protected override void PrepareProcessMaterial(Material mat)
        {
            mat.EnableKeyword("DEPTH_REALSENSE");
            mat.SetTexture("_RsDepthMap", DepthTexture);
            mat.SetFloat("_RsDepthMultiplier", 100);

            mat.SetTexture("_BgStaticDepthMask", DepthMask);

            base.PrepareProcessMaterial(mat);
        }
        protected override void CleanupProcessMaterial(Material mat)
        {
            mat.DisableKeyword("DEPTH_REALSENSE");

            base.CleanupProcessMaterial(mat);
        }

        private string cachedAltName = null;
        private string cachedSerialNumber = null;
        private string targetSerialNumber
        {
            get
            {
                if (cachedSerialNumber == null || cachedAltName == null || !cachedAltName.Equals(context.Data.deviceAltName))
                {
                    var rs_device_serial = RealSenseUtility.GetDeviceSerialFromAltName(context.Data.deviceAltName);

                    if (!string.IsNullOrEmpty(rs_device_serial))
                    {
                        cachedAltName = context.Data.deviceAltName;
                        cachedSerialNumber = rs_device_serial;
                    }
                }
                return cachedSerialNumber;
            }
        }

        protected override void SetTexture() {
            ApplyDefaultDeviceParams();

            if( deviceObj != null && device != null ) {
                return; // avoid the constant creation of devices that throw exceptions when the RS cam isn't plugged in.
            }

            if( !context.Data.deviceName.ToLower().Contains( "realsense" ) )
                return;

            int width = context.Data.deviceFeedWidth;
            int height = context.Data.deviceFeedHeight;
            int framerate = 30;

            FeedDeviceManager.OutputInfo depthOutput = FindBestDepthOutput(width, height);
            if( depthOutput == null ) {
                return; // not supported
            }
            depthWidth = (int)depthOutput.width;
            depthHeight = (int)depthOutput.height;

            deviceObj = new GameObject( "Device" );
            deviceObj.transform.SetParent( transform );
            deviceObj.SetActive( false );

            device = deviceObj.AddComponent<RealSenseDevice>();

            device.DeviceConfiguration.mode = RealSenseConfiguration.Mode.Live;

            if ( !string.IsNullOrEmpty( targetSerialNumber ) ) {
                device.DeviceConfiguration.RequestedSerialNumber = targetSerialNumber;
                lastSerial = targetSerialNumber;
            } else {
                Debug.LogError( "Can't find appropriate real sense!" );
                return;
            }


            device.DeviceConfiguration.Profiles = new VideoStreamRequest[]
            {
                new VideoStreamRequest() {
                    Stream = Stream.Color,
                    Width = width,
                    Height = height,
                    Framerate = framerate,
                    Format = Format.Rgb8,
                },
                new VideoStreamRequest()
                {
                    Stream = Stream.Depth,
                    Width = depthWidth,
                    Height = depthHeight,
                    Framerate = framerate,
                    Format = Format.Z16,
                },
                new VideoStreamRequest()
                {
                    Stream = Stream.Infrared,
                    StreamIndex = 1,
                    Width = depthWidth, // must match depth stream
                    Height = depthHeight,
                    Framerate = framerate,
                    Format = Format.Y8,
                }
            };

            DepthMaskMaterial = new Material(Shader.Find("Hidden/MixCast/Depth Mask"));
            BlurMaterial = new Material(Shader.Find("Hidden/MixCast/Separable Blur"));
            InfraredMaterial = new Material( Shader.Find("Hidden/MixCast/BW") ); // from RS2 sdk in Plugins/Realsense2/Shaders

            DepthMask = new RenderTexture(depthWidth / decimationAmount / 2, depthHeight / decimationAmount / 2, 0, RenderTextureFormat.Default);

            device.processMode = RealSenseDevice.ProcessMode.Multithread;//.UnityThread;

            colorTex = deviceObj.AddComponent<RealSenseStreamTexture>();
            colorTex.realSenseDevice = device;
            colorTex.sourceStreamType = Stream.Color;
            colorTex.textureFormat = TextureFormat.RGB24;
            colorTex.textureBinding = new RealSenseStreamTexture.TextureEvent();
            colorTex.textureBinding.AddListener(HandleColorFrameUpdated);

            depthTex = deviceObj.AddComponent<RealSenseMixcastImages>();
            depthTex.realSenseDevice = device;
            depthTex.decimationAmount = decimationAmount;
            depthTex.alignTo = Stream.Color;
            depthTex.sourceStreamType = Stream.Depth;
            depthTex.textureFormat = TextureFormat.R16;
            depthTex.textureBinding = new RealSenseStreamTexture.TextureEvent();
            depthTex.textureBinding.AddListener(HandleDepthFrameUpdated);

            irTex = deviceObj.AddComponent<RealSenseStreamTexture>();
            irTex.realSenseDevice = device;
            irTex.sourceStreamType = Stream.Infrared;
            irTex.textureFormat = TextureFormat.Alpha8;
            irTex.textureBinding = new RealSenseStreamTexture.TextureEvent();
            irTex.textureBinding.AddListener( HandleIRFrameUpdated );

            var initSettingsComponent = deviceObj.AddComponent<InitRealSenseSettings>();
            initSettingsComponent.feed = this;

            deviceObj.SetActive(true);

            lastWidth = width;
            lastHeight = height;

            base.SetTexture();

            OnProcessInputStart += ProcessDepthMask;

            ReinitializeLibAVSettings();
            context.Data.isInUseElsewhere = false;
        }

        public void ShowInfrared(bool shouldShow) {
            showInfrared = shouldShow;
            if(shouldShow) {
                OnProcessInputStart -= ProcessDepthMask;
                OnProcessInputStart += ProcessInfrared;
            } else {
                OnProcessInputStart -= ProcessInfrared;
                OnProcessInputStart += ProcessDepthMask;
            }
        }

        private void HandleIRFrameUpdated( Texture tex ) {
            IRTexture = tex;
        }

        private void HandleColorFrameUpdated(Texture tex)
        {
            RGBTexture = tex;

            if(device == null || device.ActiveProfile == null) {
                return;
            }

            // Find colour sensor
            foreach (var sensor in device.ActiveProfile.Device.Sensors)
            {
               if (!sensor.Options[Option.DepthUnits].Supported)
               {
                    colorSensor = sensor;
                    UpdateColorSensorOptions();
                    break;
               }
            }
        }
        private void HandleDepthFrameUpdated(Texture tex)
        {
            DepthTexture = tex;

            if( device == null || device.ActiveProfile == null ) {
                return;
            }

            // Find depth sensor
            foreach (var sensor in device.ActiveProfile.Device.Sensors)
            {
                if (sensor.Options[Option.DepthUnits].Supported)
                {
                    depthSensor = sensor;
                    UpdateDepthSensorOptions();
                }
            }
        }

        private void ProcessInfrared( InputFeed feed ) {
            if( context.Data == null ) {
                return;
            }

            // Set up material
            InfraredMaterial.mainTexture = IRTexture;

            Graphics.Blit( IRTexture, DepthMask, InfraredMaterial );

        }

        private void ProcessDepthMask(InputFeed feed)
        {
            if (context.Data == null)
            {
                return;
            }

            // Set up material
            DepthMaskMaterial.EnableKeyword("DEPTH_REALSENSE");
            DepthMaskMaterial.SetFloat("_RsDepthMultiplier", 100);
            DepthMaskMaterial.mainTexture = DepthTexture;

            DepthMaskMaterial.SetFloat("_BgStaticDepth_MaxDepth", context.Data.staticDepthData.maxDepth);

            DepthMaskBlur = RenderTexture.GetTemporary(depthWidth / decimationAmount / 2, depthHeight / decimationAmount / 2, 0, RenderTextureFormat.Default);

            Graphics.Blit(DepthTexture, DepthMask, DepthMaskMaterial);

            BlurMaterial.SetTexture( "_ImageTex", RGBTexture);

            // Blur result
            BlurMaterial.EnableKeyword("BLUR_HORIZONTAL");
            Graphics.Blit(DepthMask, DepthMaskBlur, BlurMaterial);
            BlurMaterial.DisableKeyword("BLUR_HORIZONTAL");

            BlurMaterial.EnableKeyword("BLUR_VERTICAL");
            Graphics.Blit(DepthMaskBlur, DepthMask, BlurMaterial);
            BlurMaterial.DisableKeyword("BLUR_VERTICAL");


            RenderTexture.ReleaseTemporary(DepthMaskBlur);
        }

        protected override void ClearTexture()
        {
            RGBTexture = null;
            DepthTexture = null;
            IRTexture = null;

            if( colorTex != null && colorTex.textureBinding != null)
                colorTex.textureBinding.RemoveListener(HandleColorFrameUpdated);
            if( depthTex != null && depthTex.textureBinding != null)
                depthTex.textureBinding.RemoveListener(HandleDepthFrameUpdated);
            if( irTex != null && irTex.textureBinding != null )
                irTex.textureBinding.RemoveListener( HandleIRFrameUpdated );
            DestroyImmediate( deviceObj );
            deviceObj = null;
            DestroyImmediate( device );
            device = null;

            base.ClearTexture();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if( isResettingCamera || !wasCameraConnected ) {
                return;
            }

            if (context.Data == null || !FeedDeviceManager.IsRealSense( context.Data ))
                return;

            ApplyDefaultDeviceParams();

            Texture renderingTexture = showInfrared ? IRTexture : RGBTexture;

            if ( renderingTexture != null && (lastWidth != context.Data.deviceFeedWidth || lastHeight != context.Data.deviceFeedHeight || lastSerial != targetSerialNumber) )
            {
                ClearTexture();
                return; //Give system a frame to shut down
            }
            if( renderingTexture == null && context.Data.deviceFeedHeight != 0 && context.Data.deviceFeedWidth != 0 )
            {
                SetTexture();
            }
        }

        private void UpdateColorSensorOptions()
        {
            colorSensor.Options[Option.EnableAutoExposure].Value = 0;
            colorSensor.Options[Option.EnableAutoWhiteBalance].Value = 0;
        }

        private void UpdateDepthSensorOptions()
        {
            depthSensor.Options[Option.EnableAutoExposure].Value = 1;
        }

        private void UpdateIRSensorOptions() {

        }

        void ApplyDefaultDeviceParams()
        {
            if (context.Data == null)
                return;

            if (context.Data.deviceData.exposure < 0)
                context.Data.deviceData.exposure = DEFAULT_EXPOSURE_VAL;
            if (context.Data.deviceData.gain < 0)
                context.Data.deviceData.gain = DEFAULT_GAIN_VAL;
            if (context.Data.deviceData.whiteBalance < 0)
                context.Data.deviceData.whiteBalance = DEFAULT_WHITEBALANCE_VAL;
            if (context.Data.deviceData.infraredExposure < 0)
                context.Data.deviceData.infraredExposure = DEFAULT_INFRARED_EXPOSURE_VAL;
        }

        IEnumerator ReinitLibAVCoroutine() {
            yield return new WaitForEndOfFrame();

            if (context == null || context.Data == null || context.Data.deviceData == null)
            {
                yield break;
            }

            RealSenseUtility.SetOption(colorSensor, Option.Exposure, context.Data.deviceData.exposure);
            RealSenseUtility.SetOption(colorSensor, Option.Gain, context.Data.deviceData.gain);
            RealSenseUtility.SetOption(colorSensor, Option.WhiteBalance, context.Data.deviceData.whiteBalance);
            RealSenseUtility.SetOption(depthSensor, Option.Exposure, context.Data.deviceData.infraredExposure);
        }

        void ReinitializeLibAVSettings() {
            StartCoroutine( ReinitLibAVCoroutine() );
        }

        /// <summary>
        /// Chooses the most appropriate depth resolution for the given RGB camera resolution.
        /// Some resolutions supported by the RGB camera, e.g. 960x540, are unsupported by the stereo module,
        /// in which case we need to choose the next best option.
        /// </summary>
        static FeedDeviceManager.OutputInfo FindBestDepthOutput(int width, int height)
        {
            if (FeedDeviceManager.realSenseDepthOutputs.Count == 0)
            {
                return null;
            }

            var aspectRatio = (float)width / height;

            try {
                // .First throws an exception if there is no "first"...
                return FeedDeviceManager.realSenseDepthOutputs.First( output =>
                     output.width <= width &&
                     output.height <= height &&
                     output.MatchesAspectRatio( aspectRatio )
                );
            } catch {
                return null;
            }
        }
    }
}
#endif
