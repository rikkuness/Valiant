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


namespace BlueprintReality.MixCast
{
    
	public class AudioAsyncOutputMuxBase : AudioCallbackComponent
	{
        protected bool isShuttingDown = false;
		private IntPtr _audAsyncEncode = IntPtr.Zero;
        protected string _audioAltName = null;

        //assumes these are setup
        protected IntPtr _audAsyncDecodeCopy = IntPtr.Zero; //assumes an global AudioAsyncFeed
		protected IntPtr _vidEncCopy = IntPtr.Zero;
		protected IntPtr _cfgAudCopy = IntPtr.Zero;
		protected IntPtr _audEncCopy = IntPtr.Zero;

        //object state for callback
        protected bool videoSetupReady = false;
		protected bool IsOverAccessBuffer = false;
		protected bool _ptrsReady = false;
		protected int encodeInterfaceNumber = -1;

        protected int Setup(string audioAltName, IntPtr vidEnc, IntPtr audEnc, IntPtr cfgAud)
		{
			if (context == null || context.Data == null || vidEnc == IntPtr.Zero || cfgAud == IntPtr.Zero || audEnc == IntPtr.Zero)
			{
				Debug.LogError("The encode or data objects are not yet setup for creating audio encoder");
				return -1;
			}
            _audioAltName = audioAltName;

			//this uses a different number each time, and the isBufferFreshAudioEncodeAsync() API call will 
			//clean the number and its access for if it is not used for two callbacks or more
			encodeInterfaceNumber = MixCastAV.AudioEncodeInterfaceCounter++;

            //TODO:
            //for persistent encode async run, and the aud async becomes dereferenced for whatever reason
            //if (_vidEncCopy != IntPtr.Zero && _audEncCopy != IntPtr.Zero && _cfgAudCopy != IntPtr.Zero)
            //{}

            if (_audAsyncEncode != IntPtr.Zero )
			{
                MixCastAV.stopAudioEncodeAsync( _audAsyncEncode );
                MixCastAV.freeAudioEncodeAsync( _audAsyncEncode );
				_audAsyncEncode = IntPtr.Zero;
			}

			//assumes an universal AudioAsyncFeed
			_audAsyncDecodeCopy = AudioAsyncFeed.Instance( context.Data.id ).audAsyncDec;
			_vidEncCopy = vidEnc;
			_audEncCopy = audEnc;
            _cfgAudCopy = cfgAud;

            _audAsyncEncode = MixCastAV.createAudioEncodeAsync( _audAsyncDecodeCopy, _vidEncCopy, _cfgAudCopy, _audEncCopy, MixCastAV.chunksPerSec);

            if (_audAsyncEncode == IntPtr.Zero)
			{
				Debug.LogError("Could not setup audio encode async interface");
				return -1;
			}

			if (MixCastAV.startAudioEncodeAsync(_audAsyncEncode) < 0)
			{
				MixCastAV.freeAudioEncodeAsync(_audAsyncEncode);
				_audAsyncEncode = IntPtr.Zero;
				Debug.LogError("Could not start audio encode async interface");
				return -1;
			}
#if _DEBUG
            Debug.LogWarning( string.Format( "Encode started for decoder: {0} with encoder: {1}", (int)_audAsyncDecodeCopy, (int)_audAsyncEncode ));
#endif
            return 0;
        }
		
		protected int EncodeWrite()
		{
			if ( _audAsyncEncode == IntPtr.Zero || context == null || context.Data == null || _audAsyncDecodeCopy == IntPtr.Zero )
			{
                if( isShuttingDown )
                    return -1;
                Debug.LogError("Audio async encode interface is not yet initialized.");
				return -1;
			}
            var feed = AudioAsyncFeed.Instance( context.Data.id );
			bool isFresh = false;
			
			//push the audio samples to the output muxer if ready
            if ( feed != null &&
				MixCastAV.checkStartedAudioEncodeAsync(_audAsyncEncode) == 0 &&
				feed.isRunning == true && _audAsyncDecodeCopy != IntPtr.Zero)
			{
				//check if the audio stream started, if it hasn't then, return early
				if (MixCastAV.checkStartedAudioDecodeAsync(_audAsyncDecodeCopy) != 0)
					return -1;

				isFresh = feed.BufferFresh(encodeInterfaceNumber);
				if (isFresh == true)
				{
					if (MixCastAV.updateBufferAudioEncodeAsync(_audAsyncEncode) < 0)
						Debug.LogError("Error updating the audio async encode interface in pulling new data.");
				}
			}
			else
			{
                if( isShuttingDown )
                    return -1;

				if (isFresh == false)
					Debug.LogError("Error, the buffer was not fresh from the decoder audio async feed");
				else if (MixCastAV.checkStartedAudioEncodeAsync(_audAsyncEncode) != 0)
					Debug.LogError("Error, the encoder was not yet started or had problems starting");
				//the buffer is not fresh from the audio decode async interface, may help to catch some weird bugs
				else if (IsOverAccessBuffer == true)
					Debug.Log("Checked buffer, but the buffer from the decode async interface was not yet ready to encode. " + encodeInterfaceNumber);

				IsOverAccessBuffer = true;
				return -1; //since we are using a callback to write when ready, this is abnormal, if it happens often
			}
			
			IsOverAccessBuffer = false;
			return 0;
		}

		public void Free()
		{
            if (_audAsyncEncode != IntPtr.Zero)
            {
                if (MixCastAV.checkStartedAudioEncodeAsync( _audAsyncEncode ) == 0)
				{
					if (MixCastAV.freeAudioEncodeAsync(_audAsyncEncode) < 0)
					{
						Debug.LogError("Error freeing audio encode interface.\n");
					}
				}
				_audAsyncEncode = IntPtr.Zero;
			}
		}

		public void SetPtrsReady(bool newVal)
		{
			_ptrsReady = newVal;
		}

		//for objects to set this callback to be ready
		public void SetCallbackReady(bool newVal)
        {
			//Debug.Log("Callback is now ready");
            videoSetupReady = newVal;
        }

        private void ShutDown() 
        {
            isShuttingDown = true;
            Free();
            
            if(context == null || context.Data == null) {
                return;
            }
            var feed = AudioAsyncFeed.Instance( context.Data.id );
            if( feed != null && feed.isRunning == true ) {
                feed.Stop();
            }

            _vidEncCopy = IntPtr.Zero;
            _audEncCopy = IntPtr.Zero;
            _cfgAudCopy = IntPtr.Zero;
        }

        private void OnDestroy() {
            ShutDown();
        }

        private void OnApplicationQuit() {
            ShutDown();
        }
    }//class
}//namespace
#endif
