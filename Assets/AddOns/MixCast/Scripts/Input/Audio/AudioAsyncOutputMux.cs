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
    [RequireComponent(typeof(OutputMixCastBase))]
    public class AudioAsyncOutputMux : AudioAsyncOutputMuxBase
	{
        public OutputMixCastBase videoEncodeCopy;
        
        private IntPtr _copyVidEnc = IntPtr.Zero;
        private IntPtr _copyAudEnc = IntPtr.Zero;
        private IntPtr _copyCfgAud = IntPtr.Zero;
		
        

        protected override void OnEnable()
        {
            base.OnEnable();

            if(context != null && context.Data != null && context.Data.audioData != null) {
                _audioAltName = context.Data.audioData.audioAltName;
            } else {
                _audioAltName = AudioDeviceManager.ALTNAMEFORNULL;
            }

            StartCoroutine( setup() );
        }
        
        IEnumerator setup()
		{
            while(videoEncodeCopy.vidEnc == IntPtr.Zero)
			{
                yield return null;
            }

            //check if vidEnc is already ready
            if( videoEncodeCopy.vidEnc != IntPtr.Zero )
			{
				_copyCfgAud = videoEncodeCopy.cfgAud;
				_copyVidEnc = videoEncodeCopy.vidEnc;
                _copyAudEnc = videoEncodeCopy.audEnc;
                while( Setup(_audioAltName, _copyVidEnc, _copyAudEnc, _copyCfgAud ) < 0 )
				{
                    //it was just not yet ready
                    yield return null;
                }
                _ptrsReady = true;
            }
        }

        protected override void OnDisable()
        {
			videoSetupReady = false;

			Free();

			_copyAudEnc = IntPtr.Zero;
			_copyVidEnc = IntPtr.Zero;
			_copyCfgAud = IntPtr.Zero;
			_ptrsReady = false;

			base.OnDisable();
		}

        protected override void HandleCallback(int callbackDecoder)
        {
            if( callbackDecoder != (int)(decoder) ) {
                return;
            }

            base.HandleCallback( callbackDecoder );

            //videoSetupReady indicates the video encoder already finished setting up
            if (videoSetupReady == true)
            {
                //do setup
                if (videoEncodeCopy.vidEnc != IntPtr.Zero && _ptrsReady == false)
                {
					_copyCfgAud = videoEncodeCopy.cfgAud;
					_copyVidEnc = videoEncodeCopy.vidEnc;
                    _copyAudEnc = videoEncodeCopy.audEnc;
                    if (Setup(_audioAltName, _copyVidEnc, _copyAudEnc, _copyCfgAud) < 0)
                    {
                        //it was just not ready, and cannot finish setup
                        return;
                    }
                    _ptrsReady = true;
                }
                else if (_copyVidEnc == videoEncodeCopy.vidEnc && _ptrsReady == true && 
					isShuttingDown == false && videoEncodeCopy.audEnc != IntPtr.Zero)
                {
					EncodeWrite();
                }
            }
            
        }


    }//class
}//namespace
#endif
