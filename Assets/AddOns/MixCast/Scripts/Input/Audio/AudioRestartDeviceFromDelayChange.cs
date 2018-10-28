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
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
	public class AudioRestartDeviceFromDelayChange : CameraComponent
	{
		[SerializeField] bool checkRecording = true;
		[SerializeField] bool checkStreaming = true;
		bool isRecordingOrStreaming = false;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			isRecordingOrStreaming = MixCast.IsRecordingOrStreaming(checkRecording, checkStreaming);
		}

		protected override void OnDisable()
		{
			isRecordingOrStreaming = MixCast.IsRecordingOrStreaming(checkRecording, checkStreaming);
			if (isRecordingOrStreaming == false)
			{
				AudioAsyncFeed.Instance(context.Data.id).PlayWithSettings(context.Data);
			}
			//else do nothing

			base.OnDisable();
		}
	}
}
#endif
