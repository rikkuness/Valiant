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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace BlueprintReality.MixCast {
    public abstract class OutputMixCastBase: CameraComponent {
        //required components for LibAvStuff
        protected IntPtr _vidEnc = IntPtr.Zero; //video encoder
        protected IntPtr _vCfgEnc = IntPtr.Zero; //video encoding data
        protected IntPtr _vTxfEnc = IntPtr.Zero; //video transform for encoding

        protected IntPtr _audEnc = IntPtr.Zero; //audio encoder created from mux setup
        protected IntPtr _aCfgEnc = IntPtr.Zero; //audio config settings for mux

        public IntPtr vidEnc { get { return _vidEnc; } }
        public IntPtr audEnc { get { return _audEnc; } }
        public IntPtr cfgAud { get { return _aCfgEnc; } }
        
		public AudioAsyncOutputMuxBase audioAsyncEnc = null;

        //libavstuff encode interface
        protected int encodeInterface = -1;
        private RenderTexture _cameraOutputTexture = null;

        private bool _encoderRunning = false;

        protected bool encoderRunning {
            get { return _encoderRunning; }
            set {
                _encoderRunning = value;
                UpdateActivelyEncodingCameras();
            }
        }

        //threading variables
        protected Thread _encoderInitThread = null;
        protected object _encoderInitLock = new object();
        protected Mutex _encoderMutex = null;

        //time calculation for framerate related stuff
        protected DateTime StartEncodingTime = DateTime.MinValue;
        protected int _encodedFrameCount = 0;
        protected float timeOvershot = 0;
        protected float _frameDuration;

        //for detecting resizing
        private int _lastHeight = 0;
        private int _lastWidth = 0;

        //important set of parameters for encoding video
        protected string _uriOutput = string.Empty;
        protected int _height = 0;
        protected int _width = 0;
        protected ulong _bitrateKbps = 0;
        protected int _gopsize = 0;
        protected int _framerate = 0;
        protected const int FLIP_VERTICAL = 1; //1 or 0 to flip
        protected const int BITS_IN_KILOBIT = 1000;

        //important set of parameters for encoding audio
        public uint AudioBitrate = 192000; //192kbps (pretty good for AAC), try 64kbps, 128kbps, 224kbps, 256kbps, 320kbps
        //these are defaults for the intermediate format that is passed between decoder and encoder
        private const string dummyName = "dummy.pcm";
        private const int DEFCHANNELS = 2;
        private const int DEFSAMPLERATE = 44100;
        private const int DEFBITDEPTH = 16;
        //AAC
        private const int AAC_BITDEPTH = 32;
        private const int OUT_SAMPLERATE = 44100;
        //MP2
        //uses 16bit
        const string MP2_CODECNAME  = "mp2";        
        const string AAC_CODECNAME = "aac";

        //constants and accessors
        protected virtual StringBuilder SRC_PIX_FMT { get { return new StringBuilder("rgba"); } }
		protected virtual StringBuilder DST_PIX_FMT { get { return new StringBuilder("yuv420p"); } }
		protected virtual StringBuilder CODEC_TYPE { get { return new StringBuilder("h264"); } }
		protected virtual StringBuilder CODEC_NAME { get { return new StringBuilder("libopenh264"); } }
		protected ulong BitRate { get { return _bitrateKbps * BITS_IN_KILOBIT; } }
		protected int GopSize { get { return _gopsize; } }
		protected float FrameDuration { get { return _frameDuration; } }

		protected virtual string Category { get { return ""; } }

		protected int Framerate
		{
			set
			{
				_framerate = value;
				if (_framerate < 1) { _framerate = 1; }
				_frameDuration = 1f / _framerate;
			}
			get { return _framerate; }
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			// Force LibAvStuff's static constructor to run. This prevents the scenario where
			// accessing a LibAV function for the first time from inside a thread crashes the app.
			// This happens after enabling the "Begin recording on MixCast start" option.
			RuntimeHelpers.RunClassConstructor(typeof(MixCastAV).TypeHandle);

			MixCastCamera cam = MixCastCamera.FindCamera(context);
			Assert.IsNotNull(cam);

			if (SetEncoderDefaults(cam))
			{
				StartCoroutine(Run());
			}
			else
				RemoveCameras();
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			StopCoroutine("Run");

			StopEncoder();
		}

		//abstract method to be overridden by derived classes
		//please define _width, _height,  _gopsize, _bitrateKbps, _framerate
		// and _uriOutput if it hasn't been set yet
		protected abstract bool SetEncoderDefaults(MixCastCamera cam);
		virtual protected void RemoveCameras() { }

		protected virtual void StartEncoder(MixCastCamera cam)
		{
			StartEncodingTime = DateTime.Now;
			_encodedFrameCount = 0;

			_lastHeight = cam.Output.height;
			_lastWidth = cam.Output.width;
		
			if (encoderRunning)
				return;

			encoderRunning = false;

			if (_vidEnc != IntPtr.Zero && _vCfgEnc != IntPtr.Zero && _vTxfEnc != IntPtr.Zero &&
				_audEnc != IntPtr.Zero && _aCfgEnc != IntPtr.Zero)
			{
				Debug.Log("Video encoder is not yet ready, it is still initialized from before");
				EventCenter.HandleEvent(Category, EventCenter.Result.Error);
				return;
			}

			StartCoroutine(InitEncoder());
		}

		protected virtual void StopEncoder()
		{
			_encoderInitThread = null; 

			if (encoderRunning == false || (_vCfgEnc == IntPtr.Zero && _vidEnc == IntPtr.Zero && 
				_vTxfEnc == IntPtr.Zero && _audEnc == IntPtr.Zero && _aCfgEnc == IntPtr.Zero))
			{
				Debug.Log("De-initializing encoder: you must first initialize an encoder");
				EventCenter.HandleEvent(Category, EventCenter.Result.Error);
				return;
			}

			StopEncoderAsync(_encoderInitLock);
			EventCenter.HandleEvent(Category, EventCenter.Result.Stopped);
			EventCenter.HandleEvent(Category, EventCenter.Result.Success,
				string.Format("{0} {1}", Text.Localization.Get("Info_Encoding_Stopped"), _uriOutput.Replace( '/', '\\' )),
				false);
		}

		protected IEnumerator InitEncoder()
		{
		    if (_vidEnc != IntPtr.Zero || _vTxfEnc != IntPtr.Zero || _vCfgEnc != IntPtr.Zero ||
				_audEnc != IntPtr.Zero || _aCfgEnc != IntPtr.Zero || 
				_encoderInitThread != null)
		    {
		        yield break;
		    }

			_encoderInitThread = new Thread(new ParameterizedThreadStart(InitEncoderAsync));
			_encoderInitThread.Start(this);

			while (_encoderInitThread.IsAlive)
			{
				yield return new WaitForSeconds(0.2f);
			}
			_encoderInitThread = null;
		}

		private void InitEncoderAsync(object sender)
		{
			var that = (OutputMixCastBase)sender;

			Debug.Log("Recording Video Stream: " + that._uriOutput);

			lock (that._encoderInitLock)
			{
				if (that.BuildEncoder(that._uriOutput) == false)
				{
					that.RemoveCameras();
					EventCenter.HandleEvent(that.Category, EventCenter.Result.Error);
					return;
				}

				that.encoderRunning = true;

				EventCenter.HandleEvent(that.Category, EventCenter.Result.Started);
			}
		}


		protected bool BuildEncoder(string outputPath)
		{
			if (_vCfgEnc != IntPtr.Zero || _vidEnc != IntPtr.Zero || _vTxfEnc != IntPtr.Zero || 
				_audEnc != IntPtr.Zero || _aCfgEnc != IntPtr.Zero)
			{
				Debug.LogError("Could not setup the encoder, previous session is still running");
				return false;
			}
			
			encodeInterface = -1;

			//build our encoder here
			_vCfgEnc = MixCastAV.getVideoEncodeCfg(outputPath, _width, _height,
				Framerate, SRC_PIX_FMT, _width, _height, Framerate, DST_PIX_FMT,
				GopSize, BitRate, CODEC_TYPE, CODEC_NAME, FLIP_VERTICAL);

			//Debug.LogWarningFormat( "vCfgEnc: w({0}), h({1}), src_pix({2}), dst_pix({3}), GopSize({4}), bitrate({5}), codec_type({6}), codec_name({7})",
			//    _width, _height, SRC_PIX_FMT, DST_PIX_FMT, GopSize, BitRate, CODEC_TYPE, CODEC_NAME );
			
			//_aCfgEnc = LibAvStuff.getAudioEncodeCfg(new StringBuilder(dummyName), DEFCHANNELS, DEFSAMPLERATE, DEFBITDEPTH,
			//    DEFCHANNELS, DEFSAMPLERATE, DEFBITDEPTH, AudioBitrate, new StringBuilder(MP2_CODECNAME), new StringBuilder(MP2_CODECNAME));

			//for AAC, we should be using 32 bit depth
			_aCfgEnc = MixCastAV.getAudioEncodeCfg(dummyName, DEFCHANNELS, DEFSAMPLERATE, DEFBITDEPTH,
				DEFCHANNELS, DEFSAMPLERATE, AAC_BITDEPTH, AudioBitrate, AAC_CODECNAME, AAC_CODECNAME);

			int ret = MixCastAV.getAudioAndVideoEncodeContextMux(ref _audEnc, ref _vidEnc, _aCfgEnc, _vCfgEnc);
            if (_vidEnc == IntPtr.Zero || _audEnc == IntPtr.Zero || ret < 0)
			{
				Debug.LogError("Could not setup the encoder, please check configuration");
				EventCenter.HandleEvent(Category, EventCenter.Result.Error, "Warning_Video_Encoder_Error", true);
				MixCastAV.freeVideoCfg(_vCfgEnc);
				MixCastAV.freeAudioCfg(_aCfgEnc);
                _vCfgEnc = IntPtr.Zero;
				_aCfgEnc = IntPtr.Zero;
				return false;
			}
			
			_vTxfEnc = MixCastAV.getVideoTransformContext(_vCfgEnc);
			if (_vTxfEnc == IntPtr.Zero)
			{
				Debug.LogError("Could not setup the video transformer for encoding, please check configuration");
				EventCenter.HandleEvent(Category, EventCenter.Result.Error, "Warning_Video_Encoder_Error", true);
				MixCastAV.freeVideoCfg(_vCfgEnc);
				_vCfgEnc = IntPtr.Zero;
                _vidEnc = IntPtr.Zero;
				return false;
			}

			if (_bitrateKbps <= 0)
				_bitrateKbps = (ulong)(_width * _height / BITS_IN_KILOBIT);

			if (_vidEnc != IntPtr.Zero && _vCfgEnc != IntPtr.Zero && _vTxfEnc != IntPtr.Zero)
				encodeInterface = MixCastAV.CreateEncodeInterface(_vidEnc, _vCfgEnc, _vTxfEnc);

			return (encodeInterface != -1);
		}
		
		protected virtual void StopEncoderAsync(System.Object encoderLock)
		{
			if (encodeInterface != -1)
			{
				MixCastAV.ReleaseEncodeInterface(encodeInterface);
				encodeInterface = -1;
			}

			double ElapseTime = (DateTime.Now - StartEncodingTime).TotalSeconds;
			double ElapsedEncodedTime = (double)_encodedFrameCount / (double)Framerate;
			double PercentEncoded = 100.0f * (ElapsedEncodedTime / ElapseTime);
			int PercentEncodedInt = (int)PercentEncoded;
			Debug.Log("% of possible frames encoded:" + PercentEncodedInt + "%");
			
			StartEncodingTime = DateTime.MinValue;
			_encodedFrameCount = 0;
			ReleaseRenderTexture(encoderLock);

			if (encoderRunning)
			{
				EventCenter.HandleEvent(Category, EventCenter.Result.Stopped);
				EventCenter.HandleEvent(Category, EventCenter.Result.Success,
					string.Format("{0} {1}", Text.Localization.Get("Info_Encoding_Stopped"), _uriOutput.Replace('/', '\\')),
					false);
			}

			encoderRunning = false;
		}

		void ReleaseRenderTexture(object encoderLock)
		{
			if (_cameraOutputTexture != null)
				_cameraOutputTexture.Release();
			_cameraOutputTexture = null;

		    if (_vidEnc != IntPtr.Zero && _vCfgEnc != IntPtr.Zero && _vTxfEnc != IntPtr.Zero &&
				_aCfgEnc != IntPtr.Zero && _audEnc != IntPtr.Zero)
		    {
		        IntPtr _vidEncCopy = _vidEnc;
		        IntPtr _vCfgEncCopy = _vCfgEnc;
		        IntPtr _vTxfEncCopy = _vTxfEnc;
				IntPtr _aCfgEncCopy = _aCfgEnc;
				IntPtr _audEncCopy = _audEnc;


				int msTimeout = 1000/MixCastAV.chunksPerSec;

		        new Thread(() =>
		        {
		            lock (encoderLock)
		            {
						Thread.Sleep(msTimeout);
		                //Debug.Log("Asynchronously cleaning up encoder: " + _encCopy.ToString());
		                MixCastAV.writeTrailerCloseStreams(_vidEncCopy);
						MixCastAV.freeVideoCfg(_vCfgEncCopy);
						MixCastAV.freeAudioEncodeContext(_audEncCopy);
						MixCastAV.freeVideoTransform(_vTxfEncCopy);
						MixCastAV.freeAudioCfg(_aCfgEncCopy);
					}
		        }).Start();
		    }

		    _vidEnc = IntPtr.Zero;
			_vCfgEnc = IntPtr.Zero;
			_vTxfEnc = IntPtr.Zero;
			_aCfgEnc = IntPtr.Zero;
			_audEnc = IntPtr.Zero;
        }

		protected IEnumerator Run()
		{
			MixCastCamera cam = MixCastCamera.FindCamera(context);
			if (cam == null || cam.Output == null)
				Debug.Log("No MixCast camera found");
			
			if (encoderRunning == false)
				StartEncoder(cam);

			while (isActiveAndEnabled)
			{
				bool running = false;
				if (_vidEnc != IntPtr.Zero && _vCfgEnc != IntPtr.Zero && _vTxfEnc != IntPtr.Zero &&
					_audEnc != IntPtr.Zero && _aCfgEnc != IntPtr.Zero)
					running = true;

				if (!running)
					yield return null;

				cam = MixCastCamera.FindCamera(context);
                bool hasOutput = cam != null && cam.Output != null;
				bool resized = hasOutput && (cam.Output.width != _lastWidth || cam.Output.height != _lastHeight);

				if (resized || (!hasOutput && running))
					StopEncoderAsync(_encoderInitLock);
				if (resized || (hasOutput && !running))
					StartEncoder(cam);

				//Encoding time management
				if (running)
				{
					float startTime = Time.unscaledTime;
					float timeDiff = 0;

					while (timeDiff < _frameDuration)
					{
						timeDiff = Time.unscaledTime - startTime + timeOvershot;

						//it has reached the frame duration, break out and encode
						if (timeDiff >= _frameDuration)
							break;

						yield return null;
					}

					timeOvershot = timeDiff % _frameDuration;

					int frameCount = Math.Max(1, (int)(timeDiff / _frameDuration));
					SendMixCastOutput(cam, frameCount);
				}
			}
		}

		protected void ResizeTexture(int width, int height)
		{
			if (_cameraOutputTexture != null &&
				_cameraOutputTexture.width == width &&
				_cameraOutputTexture.height == height)
			{
				return;
			}

			if (_cameraOutputTexture != null)
			{
				DestroyImmediate(_cameraOutputTexture);
			}

			_cameraOutputTexture = new RenderTexture(width, height, 0);
			_cameraOutputTexture.Create();

			MixCastAV.SetEncodeInterfaceTexture(encodeInterface, _cameraOutputTexture.GetNativeTexturePtr());
		}

		protected void SendMixCastOutput(MixCastCamera cam, int duplicateFrameCount)
		{
			if (!encoderRunning)
			{
				return;
			}

			ResizeTexture(_width, _height);
			Graphics.Blit(cam.Output, _cameraOutputTexture);
			MixCastAV.encoderSetDuplicateFrameCount(_vCfgEnc, duplicateFrameCount);
			GL.IssuePluginEvent(MixCastAV.GetEncodeInterfaceRenderCallback(), encodeInterface);

			_encodedFrameCount += duplicateFrameCount;
		}
		
		void UpdateActivelyEncodingCameras()
		{
			List<MixCastData.CameraCalibrationData> activelyEncoding;

			if (!MixCast.ActivelyEncoding.TryGetValue(Category, out activelyEncoding))
			{
				return;
			}

			if (encoderRunning)
			{
				activelyEncoding.Add(context.Data);
			}
			else
			{
				activelyEncoding.Remove(context.Data);
			}
		}
	}
}
#endif
