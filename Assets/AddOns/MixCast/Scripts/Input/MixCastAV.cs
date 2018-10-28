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
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Collections.Generic;
using AOT;

using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCastAV
    {
		#region Class Types
		public enum AUDIOCONFIG
		{
			NO_AUDIO,
			MICROPHONE_ONLY,
			DESKTOP_ONLY,
			MICROPHONE_AND_DESKTOP,
		}
		public enum REALSENSE_SENSORTYPE
		{
			REALSENSE_SENSORTYPE_RGB,
			REALSENSE_SENSORTYPE_DEPTH,
			REALSENSE_SENSORTYPE_OTHER, //doesn't work yet for this build
			REALSENSE_SENSORTYPE_COUNT,
		}
		public enum REALSENSE_OPTION
		{
			REALSENSE_OPTION_BACKLIGHT_COMPENSATION, //< Enable / disable color backlight compensation
			REALSENSE_OPTION_BRIGHTNESS, //< Color image brightness
			REALSENSE_OPTION_CONTRAST, //< Color image contrast
			REALSENSE_OPTION_EXPOSURE, //< Controls exposure time of color camera. Setting any value will disable auto exposure
			REALSENSE_OPTION_GAIN, //< Color image gain
			REALSENSE_OPTION_GAMMA, //< Color image gamma setting
			REALSENSE_OPTION_HUE, //< Color image hue
			REALSENSE_OPTION_SATURATION, //< Color image saturation setting
			REALSENSE_OPTION_SHARPNESS, //< Color image sharpness setting
			REALSENSE_OPTION_WHITE_BALANCE, //< Controls white balance of color image. Setting any value will disable auto white balance
			REALSENSE_OPTION_ENABLE_AUTO_EXPOSURE, //< Enable / disable color image auto-exposure
			REALSENSE_OPTION_ENABLE_AUTO_WHITE_BALANCE, //< Enable / disable color image auto-white-balance
			REALSENSE_OPTION_VISUAL_PRESET, //< Provide access to several recommend sets of option presets for the depth camera 
			REALSENSE_OPTION_LASER_POWER, //< Power of the F200 / SR300 projector, with 0 meaning projector off
			REALSENSE_OPTION_ACCURACY, //< Set the number of patterns projected per frame. The higher the accuracy value the more patterns projected. Increasing the number of patterns help to achieve better accuracy. Note that this control is affecting the Depth FPS 
			REALSENSE_OPTION_MOTION_RANGE, //< Motion vs. Range trade-off, with lower values allowing for better motion sensitivity and higher values allowing for better depth range
			REALSENSE_OPTION_FILTER_OPTION, //< Set the filter to apply to each depth frame. Each one of the filter is optimized per the application requirements
			REALSENSE_OPTION_CONFIDENCE_THRESHOLD, //< The confidence level threshold used by the Depth algorithm pipe to set whether a pixel will get a valid range or will be marked with invalid range
			REALSENSE_OPTION_EMITTER_ENABLED, //< Laser Emitter enabled 
			REALSENSE_OPTION_FRAMES_QUEUE_SIZE, //< Number of frames the user is allowed to keep per stream. Trying to hold-on to more frames will cause frame-drops.
			REALSENSE_OPTION_TOTAL_FRAME_DROPS, //< Total number of detected frame drops from all streams 
			REALSENSE_OPTION_AUTO_EXPOSURE_MODE, //< Auto-Exposure modes: Static, Anti-Flicker and Hybrid 
			REALSENSE_OPTION_POWER_LINE_FREQUENCY, //< Power Line Frequency control for anti-flickering Off/50Hz/60Hz/Auto 
			REALSENSE_OPTION_ASIC_TEMPERATURE, //< Current Asic Temperature 
			REALSENSE_OPTION_ERROR_POLLING_ENABLED, //< disable error handling 
			REALSENSE_OPTION_PROJECTOR_TEMPERATURE, //< Current Projector Temperature 
			REALSENSE_OPTION_OUTPUT_TRIGGER_ENABLED, //< Enable / disable trigger to be outputed from the camera to any external device on every depth frame 
			REALSENSE_OPTION_MOTION_MODULE_TEMPERATURE, //< Current Motion-Module Temperature 
			REALSENSE_OPTION_DEPTH_UNITS, //< Number of meters represented by a single depth unit 
			REALSENSE_OPTION_ENABLE_MOTION_CORRECTION, //< Enable/Disable automatic correction of the motion data 
			REALSENSE_OPTION_AUTO_EXPOSURE_PRIORITY, //< Allows sensor to dynamically ajust the frame rate depending on lighting conditions 
			REALSENSE_OPTION_COLOR_SCHEME, //< Color scheme for data visualization 
			REALSENSE_OPTION_HISTOGRAM_EQUALIZATION_ENABLED, //< Perform histogram equalization post-processing on the depth data 
			REALSENSE_OPTION_MIN_DISTANCE, //< Minimal distance to the target 
			REALSENSE_OPTION_MAX_DISTANCE, //< Maximum distance to the target 
			REALSENSE_OPTION_TEXTURE_SOURCE, //< Texture mapping stream unique ID 
			REALSENSE_OPTION_FILTER_MAGNITUDE, //< The 2D-filter effect. The specific interpretation is given within the context of the filter 
			REALSENSE_OPTION_FILTER_SMOOTH_ALPHA, //< 2D-filter parameter controls the weight/radius for smoothing.
			REALSENSE_OPTION_FILTER_SMOOTH_DELTA, //< 2D-filter range/validity threshold
			REALSENSE_OPTION_COUNT, //< Number of enumeration values. Not a valid input: intended to be used in for-loops. 
		}

        private struct LogMessage
        {
            public LogMessage(LogType level, string msg)
            {
                errorLevel = level;
                logMsg = String.Copy(msg);
            }

            public readonly LogType errorLevel;
            public readonly string logMsg;
        };


		//Audio Defaults
		public const int DEFAUDIO_CHANNELS = 2;
		public const int DEFAUDIO_SAMPLING = 44100;
		public const int DEFAUDIO_BITDEPTH = 16;

		#endregion

		#region Class Variables
		public const System.UInt16 STRLEN_DEVICENAME = 256;
        public const System.UInt16 STRLEN_DEVICEALTNAME = 256;
        public const int RT_BUFSIZE_DEFAULT = 0;
        public const int FORCE_RGBA = 2;
        public const int FORCE_RGB24 = 1;
		public const int flipVerticalTrue = 1;
		public const int flipVerticalFalse = 0;
		public static IntPtr audioAsyncInterface = IntPtr.Zero;
        public const int chunksPerSec = 10;
		public const int truncateAudioMs = 0;

		public const string texturePixelFormat = "rgba";

        public delegate void LibAVLogDelegate(string str);

        [MonoPInvokeCallback(typeof(LibAVLogDelegate))]
        static void messageCallback(string str) { RecordLogMessage(LogType.Log, str); }
        [MonoPInvokeCallback(typeof(LibAVLogDelegate))]
        static void warningCallback(string str) { RecordLogMessage(LogType.Warning, str); }
        [MonoPInvokeCallback(typeof(LibAVLogDelegate))]
        static void errorCallback(string str) { RecordLogMessage(LogType.Error, str); }

		public delegate void LibAVAudioDelegate(int param);

		static Queue<LogMessage> logQueue;
		static int audioCallBackCounter = 0;

		public const string CFG_NO_AUDIO = "NO_AUDIO";
		public const string CFG_MICROPHONE_ONLY = "MICROPHONE_ONLY";
		public const string CFG_DESKTOP_ONLY = "DESKTOP_ONLY";
		public const string CFG_MICROPHONE_AND_DESKTOP = "MICROPHONE_AND_DESKTOP";
								 
        #endregion

        #region Events

        public static event Action OnFailureToWriteFrame;

        #endregion

        #region Class Functions
        // Straight From the c++ Dll (unmanaged)
        [DllImport("MixCastAV", EntryPoint = "SetDebugLogFunctions")]
        public static extern void SetDebugLogFunctions(LibAVLogDelegate message, LibAVLogDelegate warning, LibAVLogDelegate error);

		[DllImport("MixCastAV", EntryPoint = "SetAudioCallBack")]
		public static extern void SetAudioCallBack(LibAVAudioDelegate audioDoneCallBack);
		
		private static void RecordLogMessage(LogType level, string msg)
        {
            LogMessage newMsg = new LogMessage(level, msg);
            logQueue.Enqueue(newMsg);
        }

		private static void AudioDone()
		{
			//Debug.Log("AudioDone callback()" + audioCallBackCounter);
			audioCallBackCounter++;
		}

        public static void LogOutput()
        {
            while (logQueue.Count > 0)
            {
                LogMessage msg = logQueue.Dequeue();

				if (msg.logMsg == null)
				{
					Debug.Log("MixCastAV log message was null");
					return;
				}
					

                #if UNITY_2017_1_OR_NEWER
                Debug.unityLogger.Log(msg.errorLevel, msg.logMsg);
                #else
                Debug.logger.Log(msg.errorLevel, msg.logMsg);
                #endif

                // Check for expected error messages and fire events accordingly.
                // This requires that we know the exact strings that LibAv reports, which isn't exactly robust.
                // In the future we should consider a better mechanism for surfacing these errors to the user.
                if (OnFailureToWriteFrame != null &&
                    msg.logMsg.StartsWith("MixCastAV: Error packet is null when writing to output file"))
                {
                    OnFailureToWriteFrame();
                }
            }
        }

		public static AUDIOCONFIG GetConfigurationFromBools(bool mic, bool desktop)
		{
            // none == 0 (0000), mic == 1 (0001), desktop == 2 (0010), both == desktop|mic (0011)
            int val = (desktop ? (int)AUDIOCONFIG.DESKTOP_ONLY    : 0)   // 0010
                    | (mic     ? (int)AUDIOCONFIG.MICROPHONE_ONLY : 0);  // 0001
            return (AUDIOCONFIG)val;
		}

		public static string GetStringFromAudioConfig(AUDIOCONFIG cfg)
		{
			switch (cfg)
			{
				case AUDIOCONFIG.NO_AUDIO:
					return CFG_NO_AUDIO;
				case AUDIOCONFIG.MICROPHONE_ONLY:
					return CFG_MICROPHONE_ONLY;
				case AUDIOCONFIG.DESKTOP_ONLY:
					return CFG_DESKTOP_ONLY;
				case AUDIOCONFIG.MICROPHONE_AND_DESKTOP:
					return CFG_MICROPHONE_AND_DESKTOP;
				default:
					return CFG_NO_AUDIO;
			}
		}

		static MixCastAV()
        {
            logQueue = new Queue<LogMessage>();

            SetDebugLogFunctions(messageCallback, warningCallback, errorCallback);


			//LibAVAudioDelegate audioCallBack = AudioDone;
			////LibAVAudioDelegate audioCallBack = AudioAsyncOutputMuxBase.AudioAsyncFeedCallback;
			//IntPtr audioCallBackPtr = Marshal.GetFunctionPointerForDelegate(audioCallBack);

			//SetAudioCallBack(audioCallBackPtr);
		}

		public static readonly int AUDIO_DELAY_MS_DEFAULT = 0;
		public static int AudioEncodeInterfaceCounter = 0; //dependent on the number of the cameras, this will be 

		//*******************
		//  COMMON CALLS
		//*******************
		[DllImport("MixCastAV", EntryPoint = "getCfgInputDataSize")]
		public static extern ulong getCfgInputDataSize(IntPtr cfg);

		[DllImport("MixCastAV", EntryPoint = "getCfgOutputDataSize")]
		public static extern ulong getCfgOutputDataSize(IntPtr cfg);

		[DllImport("MixCastAV", EntryPoint = "getVideoOutputWidth")]
		public static extern int getVideoOutputWidth(IntPtr cfg);

		[DllImport("MixCastAV", EntryPoint = "getVideoOutputHeight")]
		public static extern int getVideoOutputHeight(IntPtr cfg);

		[DllImport("MixCastAV", EntryPoint = "getVideoTransformContext")]
		public static extern IntPtr getVideoTransformContext(IntPtr cfg);

		[DllImport("MixCastAV", EntryPoint = "videoTransformConvertFrame")]
		public static extern int videoTransformConvertFrame(IntPtr vxf, IntPtr cfg);

		[DllImport("MixCastAV", EntryPoint = "audioTransformConvertSample")]
		public static extern void audioTransformConvertSample(IntPtr axf, IntPtr acfg);

		[DllImport("MixCastAV", EntryPoint = "getAudioTransformContext")]
		public static extern IntPtr getAudioTransformContext(IntPtr acfg);

		[DllImport("MixCastAV", EntryPoint = "freeVideoTransform")]
		public static extern int freeVideoTransform(IntPtr ptr);

		[DllImport("MixCastAV", EntryPoint = "freeAudioTransform")]
		public static extern int freeAudioTransform(IntPtr axf);

		//*******************
		//  DECODER CALLS
		//*******************

		[DllImport("MixCastAV", EntryPoint = "getVideoDecodeCfg")]
		public static extern IntPtr getVideoDecodeCfg(string deviceName, int inWidth, int inHeight, int inFps, string inPixFmt,
					int outWidth, int outHeight, int outFps, string outPixFmt, int flipVertical);

		///@brief Decode the next frame and place it into the buffer by reference, set gotFrame to 1 if the frame was decoded successfully, 
		///       and the frame is flipped vertically by setting flipVertical to 1 (default).
		///@note  C's bool is different from C#'s bool, since C's bool is an int, so using an int for flipVertical avoids unsafe code
		[DllImport("MixCastAV", EntryPoint = "decodeNextFrame")]
		public static extern int decodeNextFrame(IntPtr dec, IntPtr cfg);

		[DllImport("MixCastAV", EntryPoint = "getVideoDecodeContext")]
		public static extern IntPtr getVideoDecodeContext(IntPtr cfgVidDec);

		[DllImport("MixCastAV", EntryPoint = "getVideoDecodedBuffer")]
		public static extern int getVideoDecodedBuffer(IntPtr vxf, IntPtr cfgVidDec, ref byte[] buffer);

		[DllImport("MixCastAV", EntryPoint = "freeDecodeContext")]
		public static extern int freeDecodeContext(IntPtr vidDec);

		[DllImport("MixCastAV", EntryPoint = "freeVideoDecodeContext")]
		public static extern int freeVideoDecodeContext(IntPtr vidDec);
		

		[DllImport("MixCastAV", EntryPoint = "freeVideoCfg")]
		public static extern int freeVideoCfg(IntPtr cfgVidDec);

		//...Audio
		[DllImport("MixCastAV", EntryPoint = "getAudioDecodeCfg")]
		public static extern IntPtr getAudioDecodeCfg(string deviceName, int iNumChannels, int iSamplingRate, int iBitsPerSample,
		int oNumChannels, int oSamplingRate, int oBitsPerSample);

		[DllImport("MixCastAV", EntryPoint = "getAudioDecodeContext")]
		public static extern IntPtr getAudioDecodeContext(IntPtr aCfgDec);

		[DllImport("MixCastAV", EntryPoint = "getAudioDecodedBuffer")]
		public static extern void getAudioDecodedBuffer(IntPtr axf, IntPtr aCfgDec, ref byte[] buffer);

		[DllImport("MixCastAV", EntryPoint = "freeAudioDecodeContext")]
		public static extern int freeAudioDecodeContext(IntPtr audDec);

		[DllImport("MixCastAV", EntryPoint = "freeAudioCfg")]
		public static extern int freeAudioCfg(IntPtr aCfg);

		//>>>>>>>>>>>>>>>>>>>>>>>>>>>
		//  AUDIO DECODER INTERFACE
		//>>>>>>>>>>>>>>>>>>>>>>>>>>>
		[DllImport("MixCastAV", EntryPoint = "createAudioDecodeAsync")]
		public static extern IntPtr createAudioDecodeAsync(string deviceName, int numChannels, 
				int samplingRate, int bitsPerSample, int delayMs, AUDIOCONFIG audioCfg, int chunksPerSec = MixCastAV.chunksPerSec);

		[DllImport("MixCastAV", EntryPoint = "startAudioDecodeAsync")]
		public static extern int startAudioDecodeAsync(IntPtr audioAsyncInterface);

		[DllImport("MixCastAV", EntryPoint = "stopAudioDecodeAsync")]
		public static extern int stopAudioDecodeAsync(IntPtr audioAsyncInterface);
		
		//returns 0 if fresh, -1 if not fresh, -2 if it hasn't started
		[DllImport("MixCastAV", EntryPoint = "isBufferFreshAudioDecodeAsync")]
		public static extern int isBufferFreshAudioDecodeAsync(IntPtr audioAsyncInterface, int interfaceNumber);

		[DllImport("MixCastAV", EntryPoint = "getLevelsAudioDecodeAsyncMic")]
		public static extern int getLevelsAudioDecodeAsyncMic(IntPtr audioAsyncInterface, ref System.Double levels);

        [DllImport( "MixCastAV", EntryPoint = "getLevelsAudioDecodeAsyncDesktop" )]
        public static extern int getLevelsAudioDecodeAsyncDesktop( IntPtr audioAsyncInterface, ref System.Double levels );


        [DllImport("MixCastAV", EntryPoint = "freestopAudioDecodeAsync")]
		public static extern int freestopAudioDecodeAsync(IntPtr audioAsyncInterface);

		//if started, returns 0, else return -1
		[DllImport("MixCastAV", EntryPoint = "checkStartedAudioDecodeAsync")]
		public static extern int checkStartedAudioDecodeAsync(IntPtr audioAsyncInterface);
        
        [DllImport("MixCastAV", EntryPoint = "setCfgAudioDecodeAsync")]
        public static extern int setCfgAudioDecodeAsync(IntPtr audioAsyncInterface, AUDIOCONFIG audioCfg);
        
		//>>>>>>>>>>>>>>>>>>>>>>>>>>>
		//  VIDEO DECODER INTERFACE
		//>>>>>>>>>>>>>>>>>>>>>>>>>>>
		[DllImport("MixCastAV", CallingConvention = CallingConvention.Cdecl, EntryPoint = "CreateDecodeInterface")]
		public static extern int CreateDecodeInterface(IntPtr vidDec, IntPtr vidCfgDec, IntPtr vidTxfDec);

		[DllImport("MixCastAV", EntryPoint = "ReleaseDecodeInterface")]
		public static extern void ReleaseDecodeInterface(int interfaceID);

		[DllImport("MixCastAV", EntryPoint = "StartDecodeInterface")]
		public static extern void StartDecodeInterface(int interfaceID);

		[DllImport("MixCastAV", EntryPoint = "StopDecodeInterface")]
		public static extern void StopDecodeInterface(int interfaceID);

		[DllImport("MixCastAV", EntryPoint = "GetDecodeInterfaceRenderCallback")]
		public static extern IntPtr GetDecodeInterfaceRenderCallback();

		[DllImport("MixCastAV", EntryPoint = "SetDecodeInterfaceTexture")]
		public static extern void SetDecodeInterfaceTexture(int interfaceID, IntPtr unityTexture);

		//*******************
        //  ENCODER CALLS
        //*******************
        [DllImport("MixCastAV", EntryPoint = "getVideoEncodeCfg")]
        public static extern IntPtr getVideoEncodeCfg(string outputFileName, int inWidth, int inHeight,
					int inFps, StringBuilder srcPixFmt, int outWidth, int outHeight, int outFps, StringBuilder outPixFmt,
					int gopsize, ulong bitrate, StringBuilder codecType, StringBuilder codecName, int flipVertical);

		[DllImport("MixCastAV", EntryPoint = "getVideoEncodeContext")]
		public static extern IntPtr getVideoEncodeContext(IntPtr cfgVidEnc);

		[DllImport("MixCastAV", EntryPoint = "loadVideoBuffer")]
		public static extern int loadVideoBuffer(IntPtr cfgVidEnc, ref byte[] buffer);

		[DllImport("MixCastAV", EntryPoint = "encodeNextVideoFrame")]
		public static extern int encodeNextVideoFrame(IntPtr vidEnc, IntPtr cfgVidEnc);

		[DllImport("MixCastAV", EntryPoint = "writeEncodedVideoFrame")]
		public static extern int writeEncodedVideoFrame(IntPtr vidEnc, IntPtr cfgVidEnc);

		[DllImport("MixCastAV", EntryPoint = "writeTrailerCloseStreams")]
		public static extern int writeTrailerCloseStreams(IntPtr vidEnc);

		[DllImport("MixCastAV", EntryPoint = "uriCheck")]
		public static extern int uriCheck(StringBuilder uri);

		//Frees the audio or video encode context (vidEnc or audEnc)
		[DllImport("MixCastAV", EntryPoint = "freeEncodeContext")]
		public static extern int freeEncodeContext(IntPtr audOrVidEnc);

		[DllImport("MixCastAV", EntryPoint = "freeAudioEncodeContext")]
		public static extern int freeAudioEncodeContext(IntPtr audEnc);

		//...Audio
		[DllImport("MixCastAV", EntryPoint = "getAudioEncodeCfg")]
		public static extern IntPtr getAudioEncodeCfg(string outputFileName, int iNumChannels, int iSamplingRate, int iBitsPerSample,
								int oNumChannels, int oSamplingRate, int oBitsPerSample, ulong bitrate,
                                string codecType, string codecName, int numSamplesInChunk = MixCastAV.chunksPerSec, int truncateMsLength = MixCastAV.truncateAudioMs);

		[DllImport("MixCastAV", EntryPoint = "getAudioEncodeContext")]
		public static extern IntPtr getAudioEncodeContext(IntPtr cfgAudEnc);

		[DllImport("MixCastAV", EntryPoint = "loadAudioBuffer")]
		public static extern int loadAudioBuffer(IntPtr cfgAud, ref byte[] bufferOut);

		[DllImport("MixCastAV", EntryPoint = "encodeNextAudioSample")]
		public static extern int encodeNextAudioSample(IntPtr audEnc, IntPtr cfgAud);

		[DllImport("MixCastAV", EntryPoint = "writeEncodedAudioSample")]
		public static extern int writeEncodedAudioSample(IntPtr audEnc, IntPtr cfgAud);


		//...Build Muxer, expects the cfg's to be set
		//returns -3 if fail to setup, -2 if the vidEnc or -1 if audEnc is nullptr, 0 on success
		[DllImport("MixCastAV", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getAudioAndVideoEncodeContextMux")]
		public static extern int getAudioAndVideoEncodeContextMux(ref IntPtr audEnc, ref IntPtr vidEnc, IntPtr cfgAud, IntPtr cfgVid);

        [DllImport( "MixCastAV", EntryPoint = "setMicVolume" )]
        public static extern void setMicVolume( IntPtr audioAsyncInterface, float volume );

        [DllImport( "MixCastAV", EntryPoint = "setDesktopVolume" )]
        public static extern void setDesktopVolume( IntPtr audioAsyncInterface, float volume );


        //>>>>>>>>>>>>>>>>>>>>>>>>>>>
        //  AUDIO ENCODER INTERFACE
        //>>>>>>>>>>>>>>>>>>>>>>>>>>>

        [DllImport("MixCastAV", CallingConvention = CallingConvention.Cdecl, EntryPoint = "createAudioEncodeAsync")]
		public static extern IntPtr createAudioEncodeAsync(IntPtr asyncDecPtr, IntPtr vidEncsPtr, IntPtr cfgAudPtr, IntPtr audEncPtr, int chunksPerSec = MixCastAV.chunksPerSec);

		[DllImport("MixCastAV", EntryPoint = "freeAudioEncodeAsync")]
		public static extern int freeAudioEncodeAsync(IntPtr asyncAudioEncodeInterface);

		[DllImport("MixCastAV", EntryPoint = "startAudioEncodeAsync")]
		public static extern int startAudioEncodeAsync(IntPtr asyncAudioEncodeInterface);

		[DllImport("MixCastAV", EntryPoint = "stopAudioEncodeAsync")]
		public static extern int stopAudioEncodeAsync(IntPtr asyncAudioEncodeInterface);

		[DllImport("MixCastAV", EntryPoint = "checkStartedAudioEncodeAsync")]
		public static extern int checkStartedAudioEncodeAsync(IntPtr asyncAudioEncodeInterface);

		//0 is true, -1 is false
		[DllImport("MixCastAV", EntryPoint = "isBufferFreshAudioEncodeAsync")]
		public static extern int isBufferFreshAudioEncodeAsync(IntPtr asyncAudioEncodeInterface);
		
		[DllImport("MixCastAV", EntryPoint = "updateBufferAudioEncodeAsync")]
		public static extern int updateBufferAudioEncodeAsync(IntPtr asyncAudioEncodeInterface);

		[DllImport("MixCastAV", EntryPoint = "encodeWriteNextAudioEncodeAsync")]
		public static extern int encodeWriteNextAudioEncodeAsync(IntPtr asyncAudioEncodeInterface);


		//>>>>>>>>>>>>>>>>>>>>>>>>>>>
		//  VIDEO ENCODER INTERFACE
		//>>>>>>>>>>>>>>>>>>>>>>>>>>>
		[DllImport("MixCastAV", CallingConvention = CallingConvention.Cdecl, EntryPoint = "CreateEncodeInterface")]
        public static extern int CreateEncodeInterface(IntPtr vidEnc, IntPtr vidCfgEnc, IntPtr vidTxfEnc);

		[DllImport("MixCastAV", EntryPoint = "ReleaseEncodeInterface")]
        public static extern void ReleaseEncodeInterface(int interfaceID);

        [DllImport("MixCastAV", EntryPoint = "GetEncodeInterfaceRenderCallback")]
        public static extern IntPtr GetEncodeInterfaceRenderCallback();

        [DllImport("MixCastAV", EntryPoint = "SetEncodeInterfaceTexture")]
        public static extern void SetEncodeInterfaceTexture(int interfaceID, IntPtr unityTexture);

		[DllImport("MixCastAV", EntryPoint = "encoderSetDuplicateFrameCount")]
		public static extern void encoderSetDuplicateFrameCount(IntPtr vcfgEnc, int duplicateFrameCount);


		#endregion
	}
}
#endif
