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
using System.Text;
using System;

namespace BlueprintReality.MixCast
{
	public static class AudioDeviceManager
	{
		[System.Serializable]
		public class AudioInfo
		{
			public uint channel = 0;
			public uint samplingRate = 0;
			public uint avgBytesPerSec = 0;
			public uint blockAlign = 0;
			public uint bitPerSample = 0;
		}
		[System.Serializable]
		public class DeviceInfo
		{
			public string name = "";
			public string altname = "";
			public int deviceIndex = 0;
			public List<AudioInfo> outputs = new List<AudioInfo>();
		}
		public static List<DeviceInfo> devices = new List<DeviceInfo>();

		public const string FRIENDLYNAMEFORNULL = "NONE";
		public const string ALTNAMEFORNULL = "NONE";

		public static void BuildDeviceList()
		{
			devices.Clear();
			IntPtr devCtx = DirectShowDevices.buildDeviceList(DirectShowDevices.DEVICETYPE.AudioCaptureDevice);

			int numDevices = DirectShowDevices.getNumberDevices(devCtx);
			if (numDevices == 0)
			{
				DirectShowDevices.freeDeviceList( DirectShowDevices.DEVICETYPE.AudioCaptureDevice, devCtx );
				return;
			}

            string tmpStrAltName = string.Empty;
            string tmpName = string.Empty;


			for (int i = 0; i < numDevices; i++)
			{
				DeviceInfo devInfo = new DeviceInfo();

				//save the friendly name
				DirectShowDevices.getDeviceNameFromStr(devCtx, out tmpName, i);
				devInfo.name = tmpName;

				//save the alternative name
				DirectShowDevices.getDeviceAltNameFromStr(devCtx, out tmpStrAltName, i);
				devInfo.altname = tmpStrAltName.ToString();

				//set the device index from built list
				devInfo.deviceIndex = i;

				devInfo.outputs.Clear();

				int numStreams = DirectShowDevices.getNumStreamParamsInDeviceFromIndex(devCtx, i);

				List<AudioInfo> checkDuplicates = new List<AudioInfo>();

				for (int j = 0; j < numStreams; j++)
				{
					DirectShowDevices.AudioStreamParams tmpParams = new DirectShowDevices.AudioStreamParams();
					AudioInfo outputInfo = new AudioInfo();

					DirectShowDevices.getAudioStreamParamsInDeviceFromIndex(devCtx, out tmpParams, i, j);
					outputInfo.channel = tmpParams.nChannels;
					outputInfo.samplingRate = tmpParams.nSamplesPerSec;
					outputInfo.avgBytesPerSec = tmpParams.nAvgBytesPerSec;
					outputInfo.blockAlign = tmpParams.nBlockAlign;
					outputInfo.bitPerSample = tmpParams.wBitsPerSample;

					//check duplicates
					bool found = false;

					//find duplicate resolutions, and remove the lower framerate ones, first by producing a list of them
					foreach (var a in checkDuplicates)
					{
						if (a.channel == outputInfo.channel && a.samplingRate == outputInfo.samplingRate
							&& a.avgBytesPerSec == tmpParams.nAvgBytesPerSec && a.blockAlign == tmpParams.nBlockAlign)
						{
							found = true;

							//found first occurence, we can get out of here
							break;
						}
					}

					if (found == false)
						checkDuplicates.Add(outputInfo);

					AddOutputEntry(devInfo, outputInfo);
				}

                //add this device to the list along with its stream info
                if( devInfo.outputs.Count > 0 ) {
                    devInfo.outputs.Sort( SortEntriesDescendingBytesPerSec );
                    devices.Add( devInfo );
                }

				tmpStrAltName = null;
			}

			DirectShowDevices.freeDeviceList( DirectShowDevices.DEVICETYPE.AudioCaptureDevice, devCtx );
            RebuildMixCastAudioDevices(44100);
        }

        public static void RebuildMixCastAudioDevices(int samplingRate) {
            MixCast.Settings.audioDevices.Clear();
            for( int i = 0; i < devices.Count; i++ ) {
                if( devices[i] != null ) {
                    MixCastData.AudioCalibrationData audioDevice = new MixCastData.AudioCalibrationData();
                    audioDevice.audioDeviceName = devices[i].name;
                    audioDevice.audioDeviceAltName = devices[i].altname;
                    if( devices[i].outputs.Count > 0 ) {
                        int index = Utility.Find.Index<AudioInfo>(devices[i].outputs, o => o != null && o.samplingRate == samplingRate);
                        AudioInfo output;
                        output = devices[i].outputs[index >= 0 ? index : 0];
                        audioDevice.avgBytesPerSec = output.avgBytesPerSec;
                        audioDevice.bitsPerSample = output.bitPerSample;
                        audioDevice.blockAlign = output.blockAlign;
                        audioDevice.channel = output.channel;
                        audioDevice.samplingRate = output.samplingRate;
                    }
                    MixCast.Settings.audioDevices.Add( audioDevice );
                }
            }
        }

		static string GetValue(string line, string key)
		{
			int valStartIndex = line.IndexOf(key + "=");
			valStartIndex += key.Length;
			valStartIndex++;   //for = char
			string fromValStart = line.Substring(valStartIndex);
			int valEndIndex = fromValStart.IndexOf(" ");
			if (valEndIndex == -1)
				valEndIndex = fromValStart.Length;
			valEndIndex += valStartIndex;
			return line.Substring(valStartIndex, valEndIndex - valStartIndex);
		}
		static void AddOutputEntry(DeviceInfo devInfo, AudioInfo outputInfo)
		{
			if (Utility.Find.Object<AudioInfo>(devInfo.outputs, o =>
			{
				return o.channel == outputInfo.channel && o.samplingRate == outputInfo.samplingRate
				&& o.bitPerSample == outputInfo.bitPerSample && o.avgBytesPerSec == outputInfo.avgBytesPerSec
				&& o.samplingRate == outputInfo.samplingRate;
			}) == null)
				devInfo.outputs.Add(outputInfo);
		}

		static int SortEntries(AudioInfo x, AudioInfo y)
		{
			int bavgBytesPerSecComp = x.avgBytesPerSec.CompareTo(y.avgBytesPerSec);
			if (bavgBytesPerSecComp != 0)
				return bavgBytesPerSecComp;

			return x.bitPerSample.CompareTo(y.bitPerSample);
		}
		static int SortEntriesDescendingBytesPerSec(AudioInfo x, AudioInfo y)
		{
			int compareBitsPerSample= -x.bitPerSample.CompareTo(y.bitPerSample);
			if (compareBitsPerSample != 0)
				return compareBitsPerSample;

			int compareavgBytesPerSec = -x.avgBytesPerSec.CompareTo(y.avgBytesPerSec);
			return compareavgBytesPerSec;
		}

        public static MixCastData.AudioCalibrationData GetAudioDeviceByAltName(string altname) {
            if(string.IsNullOrEmpty(altname))
                return null;
            int index = Utility.Find.Index<MixCastData.AudioCalibrationData>( MixCast.Settings.audioDevices, ad => ad != null && ad.audioDeviceAltName.Contains(altname) );
            if(index >= 0) {
                return MixCast.Settings.audioDevices[index];
            }
            return null;
        }

    }//class
}//namespace
#endif
