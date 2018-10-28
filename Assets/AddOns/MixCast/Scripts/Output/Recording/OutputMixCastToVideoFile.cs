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
using System.IO;

namespace BlueprintReality.MixCast
{
    public class OutputMixCastToVideoFile : OutputMixCastBase
	{
        protected string fileName = "video.mp4";

		//settings for local file recording
		private const int FPS_RECORD_DEFAULT = 30;
        private const int BITRATE_DEFAULT = 500; //kbps
        private const int BITRATE_DEFAULT_FILE_FACTOR = 2;
        private const int FILERECORD_GOPSIZE_FACTOR = 1;
		
        protected override string Category { get { return EventCenter.Category.Recording; } }

        IEnumerator diskSpaceMonitorCoroutine;

        protected override void StartEncoder(MixCastCamera cam)
        {
            base.StartEncoder(cam);
#if !ENABLE_IL2CPP
            StartDiskSpaceMonitor();
#endif
        }

        protected override void StopEncoder()
        {
            base.StopEncoder();
#if !ENABLE_IL2CPP
            StopDiskSpaceMonitor();
#endif
        }

		protected override bool SetEncoderDefaults(MixCastCamera cam)
		{
			if (cam == null)
				return false;

			_uriOutput = fileName;
			_width = cam.Output.width;
			_height = cam.Output.height;

			if (context.Data != null)
				_bitrateKbps = (ulong)context.Data.recordingData.bitrateFileRecording;
			else
				_bitrateKbps = (ulong)(BITRATE_DEFAULT_FILE_FACTOR * _width * _height / BITS_IN_KILOBIT);

            //set the framerate from the Compositing framerate in Camera Settings UI
            Framerate = context.Data.outputFramerate == 0 ? 
                MixCast.Settings.global.targetFramerate : 
                context.Data.outputFramerate;

			_gopsize = Framerate * FILERECORD_GOPSIZE_FACTOR;

			return true;
		}

		protected override void RemoveCameras()
		{
			MixCastCamera cam = MixCastCamera.FindCamera(context);
            if (cam != null)
				MixCast.RecordingCameras.Remove(cam.context.Data);
		}

        void StartDiskSpaceMonitor()
        {
            var directory = Path.GetDirectoryName(fileName);
            var monitor = new DiskSpaceMonitor { directory = directory };

            monitor.OnLowDiskSpace += () =>
            {
                EventCenter.HandleEvent(Category, EventCenter.Result.Error, "Warning_Disk_Full");

                // Only show warning once during recording.
                StopDiskSpaceMonitor();
            };

            diskSpaceMonitorCoroutine = monitor.MonitorDiskSpace();
            StartCoroutine(diskSpaceMonitorCoroutine);
        }

        void StopDiskSpaceMonitor()
        {
            if (diskSpaceMonitorCoroutine == null)
            {
                return;
            }

            StopCoroutine(diskSpaceMonitorCoroutine);
            diskSpaceMonitorCoroutine = null;
        }
    }
}
#endif
