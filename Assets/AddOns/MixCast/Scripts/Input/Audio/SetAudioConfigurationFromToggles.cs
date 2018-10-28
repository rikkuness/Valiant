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
using UnityEngine.UI;
using System;


namespace BlueprintReality.MixCast
{
	public class SetAudioConfigurationFromToggles : CameraComponent
	{
		protected MixCastAV.AUDIOCONFIG audioConfig = MixCastAV.AUDIOCONFIG.NO_AUDIO;
		protected volatile bool togglesReady = false;
		private bool waitPreselectMicCnt = false;
		//private bool waitPreselectDesktopCnt = false;

		// Use this for initialization
		protected override void OnEnable()
		{
			base.OnEnable();

            if( context != null && context.Data != null && context.Data.audioData != null ) {
                
                if( AudioAsyncFeed.Instance(context.Data.id).audAsyncDec == IntPtr.Zero ) {
                    audioConfig = context.Data.audioData.audioConfig;
                }
            }
		}
		protected override void OnDisable()
		{
			base.OnDisable();
		}

		//HandleToggleMicTrigger gets called from Toggle script
		public void HandleToggleMicTrigger(bool newVal)
		{
			if (waitPreselectMicCnt == false)
				waitPreselectMicCnt = true;
			else
			{
                if( context != null && context.Data != null && context.Data.audioData != null )
                    context.Data.audioData.useAudioInput = newVal;
                HandleDataChanged();
            }
		}

		//HandleToggleDesktopTrigger gets called from Toggle script
		public void HandleToggleDesktopTrigger(bool newVal)
		{
            //we allow the desktop trigger to go through without filtering the preselect toggles
            //in order to initialize the state of the triggers and call HandleDataChanged() on initialization
            if( context != null && context.Data != null && context.Data.audioData != null )
                context.Data.audioData.useDesktopAudio = newVal;
			HandleDataChanged();
		}

		protected override void HandleDataChanged()
		{
			base.HandleDataChanged();
            if( context != null && context.Data != null && context.Data.audioData != null ) {
                //we always first set it to microphone + desktop mode inside SetPlay()
                AudioAsyncFeed.Instance( context.Data.id ).SetAudioConfiguration( context.Data.audioData.audioConfig );
            }
		}
    }//class
}//namespace
#endif
