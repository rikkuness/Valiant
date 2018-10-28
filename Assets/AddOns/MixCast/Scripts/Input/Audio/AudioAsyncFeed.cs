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
	public class AudioAsyncFeed : MonoBehaviour
	{
		public IntPtr audAsyncDec = IntPtr.Zero;

		public enum RETURNCHANGETYPE
		{
			ErrorCreating = -1,
			MadeNewDevice = 0,
			ConfigurationChangeOnly = 1, 
			NothingNewDoNothing = 2,
		}

		//state variables
		public bool isRunning = false;

        //singleton
        private static Dictionary<string,AudioAsyncFeed> _instance;
		private AudioAsyncFeed() { }

		//settings variables
		private string _camID = AudioDeviceManager.ALTNAMEFORNULL;
        private string _adeviceAltName = "";
		private int _adeviceChannels = 2;
		private int _adeviceBitsPerSample = 16;
		private int _adeviceSamplingRate = 44100;
		private int _adeviceDelayMs = 0;
		private MixCastAV.AUDIOCONFIG _adeviceConfiguration = MixCastAV.AUDIOCONFIG.NO_AUDIO;
        
        private static bool _suppressingPlayback = false;
        private static bool _shuttingDown = false;

        private readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        private MixCastData.CameraCalibrationData camData;
        private MixCastData.AudioCalibrationData micData;

        //[SerializeField] string deviceName; // just for debugging feedback, don't use. commenting out so no import warning in SDK

        private void Awake()
        {
            //Instance( "None" );
            InitInternal();
        }

        protected void OnEnable() {
            StartCoroutine( PlayWhenReady() );
        }

        IEnumerator PlayWhenReady() {
            while( _instance == null || string.IsNullOrEmpty( _camID ) || _camID == AudioDeviceManager.ALTNAMEFORNULL ) {
                yield return waitForFixedUpdate;
            }
            yield return waitForEndOfFrame;
            camData = MixCast.Settings.GetCameraByID(_camID);
            if(camData != null ) {
                PlayWithSettings(camData);
            }
        }

        public static bool IsSuppressingPlayback() {
            return _suppressingPlayback;
        }

        public static void SuppressPlayback(bool suppress) {
            _suppressingPlayback = suppress;
        }

        static void InitInternal() {
            if( _instance == null ) {
                _instance            = new Dictionary<string, AudioAsyncFeed>();
                AudioAsyncFeed feed  = new GameObject().AddComponent<AudioAsyncFeed>();
                feed.gameObject.name = "AudioSyncFeed Manager";
                feed._camID          = AudioDeviceManager.ALTNAMEFORNULL;

                _instance.Add( AudioDeviceManager.ALTNAMEFORNULL, feed );
               
                DontDestroyOnLoad( feed.gameObject );
            }
        }

        public bool BufferFresh(int interfaceNumber)
		{ 
			if (MixCastAV.checkStartedAudioDecodeAsync(audAsyncDec) == 0)
			{
				int res = MixCastAV.isBufferFreshAudioDecodeAsync(audAsyncDec, interfaceNumber);
				if (res == -2)
					Debug.LogError("The async audio decode interface buffer is not ready because it has not yet started.");
#if _DEBUG
                Debug.LogWarning( string.Format("Buffer Freshness: {0} - {1}", (res >= 0 ? "Yes" : "No"), interfaceNumber));
#endif
				return res == 0 ? true : false;
			}
				
			else return false;
		}

        bool AddCameraFeedIfMissing(string camID) {
            if( _shuttingDown )
                return false;

            string name = camID;
            if( string.IsNullOrEmpty( name ) || name == AudioDeviceManager.ALTNAMEFORNULL )
                return false;
            
            var root = _instance[AudioDeviceManager.ALTNAMEFORNULL];
            if( root == null && _shuttingDown == false )
                throw new ArgumentNullException( string.Format( "AudioAsyncFeed::AddCameraFeedIfMissing() - Awake not yet called the first time.", camID ) ); // should never happen

            var newInstance = root.gameObject.AddComponent<AudioAsyncFeed>();
            newInstance._camID = camID;
            newInstance.UpdateSettings();
            _instance.Remove( name );
            _instance.Add( name, newInstance );
            return true;
        }

        public static void RemoveCamera(string camID) {
            var feed = Instance(camID);
            Destroy( feed );
            _instance.Remove( camID );
        }

		public static AudioAsyncFeed Instance(string camID)
		{
            string name = camID;
            if( string.IsNullOrEmpty( name ) )
                name = AudioDeviceManager.ALTNAMEFORNULL;

            if( _instance == null ) {
                InitInternal();
            }
            
            if(_instance.ContainsKey(name)) {
                return _instance[name];
            }

            if(_instance[AudioDeviceManager.ALTNAMEFORNULL].AddCameraFeedIfMissing( camID )) {
                return _instance[name];
            }
            return null;
		}

        void UpdateSettings() {
            if( string.IsNullOrEmpty( _camID ) ) {
                _camID = AudioDeviceManager.ALTNAMEFORNULL;
            }

            MixCastData.CameraCalibrationData curCam = MixCast.Settings.GetCameraByID( _camID );
            if( curCam == null || curCam.audioData == null)
                return; // shouldn't happen, but just in case.

            var device = AudioDeviceManager.GetAudioDeviceByAltName( curCam.audioData.audioAltName );
            if( device != null ) {
                _adeviceAltName       = device.audioDeviceAltName;
                _adeviceBitsPerSample = (int)device.bitsPerSample;
                _adeviceChannels      = (int)device.channel;
                _adeviceConfiguration = device.defaultSettings.audioConfig;
                _adeviceSamplingRate  = (int)device.samplingRate;
				_adeviceDelayMs		  = (int)device.desktopDelayMs;
				// deviceName            = device.audioDeviceName; // commenting out so no import warning in SDK
            }

        }
		
        public bool IsDesktopAvailable() {
            return audAsyncDec != IntPtr.Zero;
        }

		void OnDisable()
		{
			//stop async interface
			if (audAsyncDec != IntPtr.Zero)
				Stop();
		}



        public RETURNCHANGETYPE PlayWithSettings( MixCastData.CameraCalibrationData cam )
		{
            if(_suppressingPlayback) {
                return RETURNCHANGETYPE.NothingNewDoNothing;
            }

			cam.audioData.delayMs = (int)cam.bufferTime;
			micData = AudioDeviceManager.GetAudioDeviceByAltName(cam.audioData.audioAltName);
            if( cam != null )
			{
                if( micData != null )
				{
                    return SetPlay( cam.audioData.audioAltName, (int)micData.channel, (int)micData.samplingRate, (int)micData.bitsPerSample,
                        cam.audioData.audioConfig, cam.audioData.volume, cam.audioData.desktopVolume, cam.audioData.delayMs );
                }
                cam.audioData.useAudioInput = false; // mic is null
                return SetPlay( "", 0, 0, 0, cam.audioData.audioConfig, 0.0f, cam.audioData.desktopVolume, cam.audioData.delayMs );
            }
            return SetPlay( "", 0, 0, 0, MixCastAV.AUDIOCONFIG.NO_AUDIO, 0.0f, 0.0f, 0 );
        }

		//multi tool function
		public RETURNCHANGETYPE SetPlay(string altName, int numChannels, int samplingRate, int bitsPerSample,
            MixCastAV.AUDIOCONFIG audioConfig, float micVolume, float desktopVolume, int delayMs)
		{
			if ( _suppressingPlayback ) {
                return RETURNCHANGETYPE.NothingNewDoNothing;
            }
            //Debug.Log( "SetPlay()" );
            if (delayMs > 1000)
			{
				Debug.LogWarning("Delay is too high for the audio, " + delayMs + "ms, setting it to 1000ms.");
				delayMs = 1000;
			}

			//create the audio asynchronous interface
			string DeviceNameSwitch = altName;
			int nChannelsSwitch = numChannels;
			int samplingRateSwitch = samplingRate;
			int bitsPerSampleSwitch = bitsPerSample;
            MixCastAV.AUDIOCONFIG configSwitch = audioConfig;

            //when the string is null, we want to use some defaults for a null audio track still
            if (string.IsNullOrEmpty( altName ) == true || altName.Contains( AudioDeviceManager.ALTNAMEFORNULL ) )
			{
				//dummy info for null track when no data found
				DeviceNameSwitch = AudioDeviceManager.ALTNAMEFORNULL;
				nChannelsSwitch = MixCastAV.DEFAUDIO_CHANNELS;
				samplingRateSwitch = MixCastAV.DEFAUDIO_SAMPLING;
				bitsPerSampleSwitch = MixCastAV.DEFAUDIO_BITDEPTH;
			}
			

			//if it is exactly the same as last configuration
			if (audAsyncDec != IntPtr.Zero)
			{
				if (_adeviceAltName == altName &&
					_adeviceBitsPerSample == bitsPerSample &&
					_adeviceChannels == numChannels &&
					_adeviceSamplingRate == samplingRate &&
					_adeviceDelayMs == delayMs) 
                {
                    if( _adeviceConfiguration == audioConfig ) {
                        //Debug.LogWarning( "No audio change for " + altName );
                        return RETURNCHANGETYPE.NothingNewDoNothing; //nothing to do since it is the same as last time
                    } else {
                        // only audioConfig changed
                        SetAudioConfiguration( audioConfig );
                        //Debug.LogWarning( "Audio Config: " + audioConfig.ToString() );
                        return RETURNCHANGETYPE.ConfigurationChangeOnly;
                    }
                }
				else
				{
					Stop();
				}
			}

			//Debug.LogError("devicename: " + deviceName + ", nCh: " + numChannels + ", sampling: " + samplingRate + ", bitsPer: " + bitsPerSample + ", cfg: " + audioConfig);
			audAsyncDec = MixCastAV.createAudioDecodeAsync(DeviceNameSwitch, nChannelsSwitch, 
                samplingRateSwitch, bitsPerSampleSwitch, delayMs, MixCastAV.AUDIOCONFIG.MICROPHONE_AND_DESKTOP, MixCastAV.chunksPerSec);
			
			//Debug.Log("delay is set to : " + delayMs);

			if (audAsyncDec == IntPtr.Zero)
			{
                //Debug.LogError("Error creating Audio Device Async Interface." + audAsyncDec);
                Debug.LogWarning( "Error creating decoder" );
				return RETURNCHANGETYPE.ErrorCreating;
			}
			else //audAsyncDec is already ready
			{
                //successfully created, so save the variables
                _adeviceAltName       = DeviceNameSwitch;
                _adeviceChannels      = nChannelsSwitch;
				_adeviceSamplingRate  = samplingRateSwitch;
				_adeviceBitsPerSample = bitsPerSampleSwitch;
				_adeviceConfiguration = configSwitch;
				_adeviceDelayMs		  = delayMs;

				MixCastAV.setMicVolume(audAsyncDec, micVolume);
				MixCastAV.setDesktopVolume(audAsyncDec, desktopVolume);
                Play();
                SetAudioConfiguration( audioConfig );
                //set intended configuration
                //if (LibAvStuff.checkStartedAudioDecodeAsync(audAsyncDec) == 0)

                // deviceName = _adeviceAltName; // commenting out so no import warning in SDK

                return RETURNCHANGETYPE.MadeNewDevice;
			}
		}

        public void SetAudioConfiguration(MixCastAV.AUDIOCONFIG cfgType)
        {
			if (audAsyncDec != IntPtr.Zero)
			{
				//useful for debugging
				//if (cfgType == MixCastAV.AUDIOCONFIG.MICROPHONE_AND_DESKTOP)
				//	Debug.Log("The audio mode is set to : MICROPHONE_AND_DESKTOP");
				//if (cfgType == MixCastAV.AUDIOCONFIG.DESKTOP_ONLY)
				//	Debug.Log("The audio mode is set to : DESKTOP_ONLY");
				//if (cfgType == MixCastAV.AUDIOCONFIG.MICROPHONE_ONLY)
				//	Debug.Log("The audio mode is set to : MICROPHONE_ONLY");
				//if (cfgType == MixCastAV.AUDIOCONFIG.NO_AUDIO)
				//	Debug.Log("The audio mode is set to : NO_AUDIO");

				MixCastAV.setCfgAudioDecodeAsync(audAsyncDec, cfgType);
				_adeviceConfiguration = cfgType; // save the configuration change
			}
            //Debug.LogWarning( "Setting audio config to " + cfgType.ToString() );
        }

        public void SetMicVolume(float vol) {
            if(audAsyncDec != IntPtr.Zero) {
                MixCastAV.setMicVolume( audAsyncDec, vol );
            }
        }

        public void SetDesktopVolume(float vol) {
            if( audAsyncDec != IntPtr.Zero ) {
                MixCastAV.setDesktopVolume( audAsyncDec, vol );
            }
        }

        public void SetQuality(int quality) {
            if(audAsyncDec != IntPtr.Zero ) {
                // not implemented anymore
                // this feature was cut for now.
            }
        }

        public float GetMicMeterLevel() {
            double level = 0.0;
            if(audAsyncDec != IntPtr.Zero ) {
                MixCastAV.getLevelsAudioDecodeAsyncMic( audAsyncDec, ref level );
            }
            return (float)level;
        }

        public float GetDesktopMeterLevel() {
            double level = 0.0;
            if( audAsyncDec != IntPtr.Zero ) {
                MixCastAV.getLevelsAudioDecodeAsyncDesktop( audAsyncDec, ref level );
            }
            return (float)level;
        }



        //do not call this more than once when it is running
        protected void Play()
		{
            if( isRunning == false ) {
                if( MixCastAV.startAudioDecodeAsync( audAsyncDec ) < 0 ) {
                    Debug.LogError( "Failed Starting Audio Device Async Interface." + audAsyncDec );
                    isRunning = false;
                } else {
                    //Debug.LogWarning( "Playing audio" );
                    isRunning = true;
                    //Debug.LogWarning( "audio decode started for: " + (int)audAsyncDec );
                }
            }
		}

		public void Stop()
		{
			if (audAsyncDec != IntPtr.Zero)
				_killDecoder();

			isRunning = false;
		}

        public double GetLevelsMic() {
            Double level = 0d;
            if( audAsyncDec != IntPtr.Zero ) {
                MixCastAV.getLevelsAudioDecodeAsyncMic( audAsyncDec, ref level );
            }
            return level;
        }

        public double GetLevelsDesktop() {
            Double level = 0d;
            if( audAsyncDec != IntPtr.Zero ) {
                MixCastAV.getLevelsAudioDecodeAsyncDesktop( audAsyncDec, ref level );
            }
            return level;
        }

		private void _killDecoder()
		{
			bool resFreeDec = false;
			
			if ( audAsyncDec != IntPtr.Zero )     
                resFreeDec = MixCastAV.freestopAudioDecodeAsync( audAsyncDec ) == 0;
            audAsyncDec = IntPtr.Zero;

			if (resFreeDec == false)
				Debug.LogError("Error Freeing Audio Device Async Interface. " + audAsyncDec);
		}

        private void OnApplicationQuit() {
            _shuttingDown = true;
        }
    }//class
} //namespace

#endif
