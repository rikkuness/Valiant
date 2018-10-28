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

using UnityEngine;

namespace BlueprintReality.MixCast {
    public class OutputMixCastToNetwork : OutputMixCastBase
    {
        public bool force720p = false;
        public bool force1080p = false;
		
        private const int BITRATE_DEFAULT = 1000;
        private const int WIDTH_1080P = 1920;
        private const int WIDTH_720P = 1280;
        private const int HEIGHT_1080P = 1080;
        private const int HEIGHT_720P = 720;
        private const int FPS_RECORD_DEFAULT = 30;
        private const int GOP_SIZE_DEFAULT = 60;
        private const int STREAMING_GOPSIZE_FACTOR = 3;
		private const int BITRATE_DEFAULT_STREAM_FACTOR = 1;

        private bool shouldStopStreaming;

		protected override string Category {get { return EventCenter.Category.Streaming; }}

        void Update()
        {
            if (shouldStopStreaming)
            {
                shouldStopStreaming = false;
                RemoveCameras();
                EventCenter.HandleEvent(Category, EventCenter.Result.Error, "Warning_Streaming_Interrupted");
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            MixCastAV.OnFailureToWriteFrame -= HandleFailureToWriteFrame;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            MixCastAV.OnFailureToWriteFrame += HandleFailureToWriteFrame;
        }

        protected override bool SetEncoderDefaults(MixCastCamera cam)
        {
			if (_vidEnc != IntPtr.Zero || _vCfgEnc != IntPtr.Zero || _vTxfEnc != IntPtr.Zero)
			{
				Debug.Log("Initializing encoder: encoder must be shut down first!");
				EventCenter.HandleEvent(Category, EventCenter.Result.Error);
				return false;
			}

			if (!Uri.IsWellFormedUriString(_uriOutput, UriKind.Absolute))
			{
				Debug.LogError("OutputMixCastToNetwork has invalid url: " + _uriOutput);
				EventCenter.HandleEvent(Category, EventCenter.Result.Error, "Warning_Streaming_UrlInvalid");
				return false;
			}


			if (force1080p)
			{
				_width = WIDTH_1080P;
				_height = HEIGHT_1080P;
			}
			else if (force720p)
			{
				_width = WIDTH_720P;
				_height = HEIGHT_720P;
			}
			else if (cam != null && cam.Output != null)
			{
				_width = cam.Output.width;
				_height = cam.Output.height;
			}
			else
			{
				Debug.LogWarning("could not determine correct encoder output dimensions");
				_width = WIDTH_720P;
				_height = HEIGHT_720P;
			}

			if (context.Data != null)
				_bitrateKbps = (ulong)context.Data.recordingData.perCamStreamBitrate;
			else
				_bitrateKbps = (ulong)MixCast.Settings.global.defaultStreamBitrate;

			//this is approximatly 1mbps for 1920x1080 video
			if (_bitrateKbps == 0)
				_bitrateKbps = (ulong)(BITRATE_DEFAULT_STREAM_FACTOR * _width * _height / BITS_IN_KILOBIT);


            //set the framerate from the Compositing framerate in Camera Settings UI
            Framerate = context.Data.outputFramerate == 0 ?
               MixCast.Settings.global.targetFramerate :
               context.Data.outputFramerate;

            _gopsize = Framerate * STREAMING_GOPSIZE_FACTOR;

			return true;
        }

		protected override void RemoveCameras()
		{
			MixCastCamera cam = MixCastCamera.FindCamera(context);
            if (cam != null)
				MixCast.StreamingCameras.Remove(cam.context.Data);
		}

        void HandleFailureToWriteFrame()
        {
            shouldStopStreaming = true;
        }
	}
}
#endif
