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
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace BlueprintReality.MixCast
{
	public class AudioDevice
	{
		private IntPtr cfgAudDec = IntPtr.Zero;
		private IntPtr audDec = IntPtr.Zero;
		private IntPtr aTxfDec = IntPtr.Zero;
		private IntPtr audAsyncDec = IntPtr.Zero;


		//defaults for the audio we want is 16-bit Stereo 44100Hz
		private const int AUDIO_NUMCHANNELS = 2;
		private const int AUDIO_SAMPLINGRATE = 44100;
		private const int AUDIO_BITSPERSAMPLE = 16;

		//buffer variables
		//private ulong udatasize = 0;
		protected System.Single _levels = 0.0f;
		public System.Single getLevels { get { return _levels; } }
		
		//state variables
		private volatile bool startRun = false;
		public bool getStartRun { get { return startRun; } }


		public AudioDevice(string deviceName, int numChannels, int samplingRate, int bitsPerSample)
		{

			if (cfgAudDec != IntPtr.Zero || audDec != IntPtr.Zero || aTxfDec != IntPtr.Zero)
				return;

			//allocate the data and config context
			cfgAudDec = MixCastAV.getAudioDecodeCfg(deviceName, numChannels, samplingRate, bitsPerSample, 
				AUDIO_NUMCHANNELS, AUDIO_SAMPLINGRATE, AUDIO_BITSPERSAMPLE);
			
			//setup the audio decode codec
			audDec = MixCastAV.getVideoDecodeContext(cfgAudDec);

			//setup the audio transformer for decode
			aTxfDec = MixCastAV.getVideoTransformContext(cfgAudDec);

			if (cfgAudDec != IntPtr.Zero && audDec != IntPtr.Zero && aTxfDec != IntPtr.Zero)
				Debug.Log("Started Audio Device");
			else
				return;

			//udatasize = LibAvStuff.getCfgOutputDataSize(cfgAudDec);

			//create the audio asynchronous interface
			//audAsyncDec = LibAvStuff.createAudioDecodeAsync(audDec, cfgAudDec, aTxfDec);
		}

		public void Play()
		{
			if (MixCastAV.startAudioDecodeAsync(audAsyncDec) < 0)
			{
				Debug.LogError("Failed Starting Audio Device Async Interface." + audAsyncDec);
				startRun = false;
			}
			else 
				startRun = true;
		}

		public void Stop()
		{
			if (cfgAudDec != IntPtr.Zero || audDec != IntPtr.Zero || aTxfDec != IntPtr.Zero)
				_killDecoder();
			
			startRun = false;
		}
		

		private void _killDecoder()
		{
			bool resFreeDec = false;
			bool resFreeCfg = false;
			bool resFreeTxf = false;

			MixCastAV.freestopAudioDecodeAsync(audAsyncDec);
			System.Threading.Thread.Sleep(2); //untested amount of sleep time in ms needed to avoid race condition
            audAsyncDec = IntPtr.Zero;

            //free the decoder
            if (audDec != IntPtr.Zero)
				resFreeDec = MixCastAV.freeAudioDecodeContext(audDec) == 0 ? true : false;
			audDec = IntPtr.Zero;

			//free the data config
			if (cfgAudDec != IntPtr.Zero)
				resFreeCfg = MixCastAV.freeAudioCfg(cfgAudDec) == 0 ? true : false;
			cfgAudDec = IntPtr.Zero;

			//free the transformer
			if (aTxfDec != IntPtr.Zero)
				resFreeTxf = MixCastAV.freeAudioTransform(aTxfDec) == 0 ? true : false;
			aTxfDec = IntPtr.Zero;


			if (resFreeDec == false || resFreeCfg == false || resFreeTxf == false)
			{
				Debug.LogError("Error Freeing Audio Device. " + audDec);
			}
		}

	}//classs
}//namespace
#endif
