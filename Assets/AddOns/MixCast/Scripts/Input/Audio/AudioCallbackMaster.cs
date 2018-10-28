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
using System.Runtime.InteropServices;
using AOT;

namespace BlueprintReality.MixCast
{
	[RequireComponent(typeof(AudioCallbackContext))]
	public class AudioCallbackMaster : MonoBehaviour
	{
		public static AudioCallbackContext audioCallContext;

		protected virtual void Reset()
		{
			audioCallContext = GetComponentInParent<AudioCallbackContext>();
		}
		protected virtual void OnEnable()
		{
			if (audioCallContext == null)
				audioCallContext = GetComponentInParent<AudioCallbackContext>();

			//set the callback function for libavstuff
			MixCastAV.SetAudioCallBack(myAudioCallBack);
		}

        [MonoPInvokeCallback(typeof(MixCastAV.LibAVAudioDelegate))]
        private static void myAudioCallBack(int param)
		{
            audioCallContext.Callback( param );
		}
		
	}//class
}//namespace
#endif
