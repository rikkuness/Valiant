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
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


namespace BlueprintReality.MixCast
{
	public class DirectShowDevices
	{
		#region Class Types
		[StructLayout(LayoutKind.Sequential)]
		public struct VideoStreamParams
		{
			public uint bitrate;
			public uint height;
			public uint width;
			public uint framerate;
			public uint numPlanes;
			public uint bitsPerPixel;
			public int pixFmt;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct AudioStreamParams
		{
			public uint nChannels;       // number of channels (i.e. mono, stereo...)
			public uint nSamplesPerSec;  // sample rate 
			public uint nAvgBytesPerSec; // for buffer estimation 
			public uint nBlockAlign;     // block size of data 
			public uint wBitsPerSample;  // number of bits per sample of mono data 
		}
		public enum DEVICETYPE
		{
			AudioCaptureDevice = 0,
			VideoCaptureDevice = 1,
			CameraSensors = 2, 
			UnknownDevice = 3,
		}
		#endregion //Class Types

		#region Class Variables
		public const System.UInt16 STRLEN_DEVICENAME = 256;
		public const System.UInt16 STRLEN_DEVICEALTNAME = 256;
	
		#endregion //Class Variables

		#region Class Functions
		[DllImport("DshowDevices", EntryPoint = "buildDeviceList")]
		public static extern IntPtr buildDeviceList(DEVICETYPE type);

		[DllImport("DshowDevices", EntryPoint = "freeDeviceList")]
		public static extern bool freeDeviceList( DEVICETYPE type, IntPtr devList);
        
		[DllImport("DshowDevices", EntryPoint = "getNumberDevices")]
		public static extern int getNumberDevices(IntPtr devList);
		
		[DllImport("DshowDevices", CharSet = CharSet.Ansi, EntryPoint = "getDeviceNameFromIndex")]
		public static extern void getDeviceNameFromIndex(IntPtr devList, byte[] str, int index);

		//Use this to call the above deviceName in order to get the correct ASCII conversion
		public static void getDeviceNameFromStr(IntPtr devList, out string str, int index)
		{
			byte[] z = new byte[STRLEN_DEVICENAME];
			getDeviceNameFromIndex(devList, z, index);
 			str = Encoding.ASCII.GetString(z).Trim('\0');
			z = null;
		}

		[DllImport("DshowDevices", EntryPoint = "getDeviceAltNameFromIndex")]
		public static extern void getDeviceAltNameFromIndex(IntPtr devList, byte[] str, int index);

        public static void getDeviceAltNameFromStr( IntPtr devList, out string str, int index ) {
            byte[] z = new byte[STRLEN_DEVICENAME];
            getDeviceAltNameFromIndex( devList, z, index );
            str = Encoding.ASCII.GetString( z ).Trim( '\0' );
            z = null;
        }


        [DllImport("DshowDevices", EntryPoint = "getNumStreamParamsInDeviceFromIndex")]
		public static extern int getNumStreamParamsInDeviceFromIndex(IntPtr devList, int devIndex);

		[DllImport("DshowDevices", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getAudioStreamParamsInDeviceFromIndex")]
		public static extern void getAudioStreamParamsInDeviceFromIndex(IntPtr devCtx, out AudioStreamParams sParams, int devIndex, int streamIndex);

		[DllImport("DshowDevices", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getVideoStreamParamsInDeviceFromIndex")]
		public static extern void getVideoStreamParamsInDeviceFromIndex(IntPtr devCtx, out VideoStreamParams sParams, int devIndex, int streamIndex);

		[DllImport("DshowDevices", EntryPoint = "avPixFmtToString")]
		public static extern void avPixFmtToString(int AvPixelFormat, StringBuilder pixFmtString);

		[DllImport("DshowDevices", EntryPoint = "fmtStringToAvPixFmt")]
		public static extern int fmtStringToAvPixFmt(StringBuilder formatString);


		///@brief Checks if the device altname is within the list of devices on the system based on deviceType.
		/// example: int ret = buildDeviceNamesFind(DEVICETYPE.VideoCaptureDevice, 
		///						new StringBuilder("@device_pnp_\\\\?\\root#media#0001#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\\global"));
		///						
		///@return Returns 0 upon success, -1 if it could not find the device
		[DllImport("DshowDevices", EntryPoint = "buildDeviceNamesFind", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall )]
		public static extern int buildDeviceNamesFind(DEVICETYPE type, [MarshalAs( UnmanagedType.LPStr )]string altName);

		#endregion //Class Functions
	}
}
#endif
