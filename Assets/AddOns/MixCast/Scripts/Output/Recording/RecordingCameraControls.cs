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
namespace BlueprintReality.MixCast 
{
	public class RecordingCameraControls : CameraComponent 
	{
        public void StartCameraRecording()
        {
            if (context.Data == null)
            {
                return;
            }

            MixCast.RecordingCameras.Add(context.Data);
        }

        public void StopCameraRecording()
        {
            if (context.Data == null)
            {
                return;
            }

            MixCast.RecordingCameras.Remove(context.Data);
        }

        public void ToggleCameraRecording()
        {
            if (context.Data == null)
            {
                return;
            }

            if (MixCast.RecordingCameras.Contains(context.Data))
            {
                StopCameraRecording();
            }
            else
            {
                StartCameraRecording();
            }
        }

        public void StartCameraStreaming()
        {
            if (context.Data == null)
            {
                return;
            }

            MixCast.StreamingCameras.Add(context.Data);
        }

        public void StopCameraStreaming()
        {
            if (context.Data == null)
            {
                return;
            }

            MixCast.StreamingCameras.Remove(context.Data);
        }

        public void ToggleCameraStreaming()
        {
            if (context.Data == null)
            {
                return;
            }

            if (MixCast.StreamingCameras.Contains(context.Data))
            {
                StopCameraStreaming();
            }
            else
            {
                StartCameraStreaming();
            }
        }

        public void ToggleCameraTimelapse()
        {
            if (context.Data == null)
            {
                return;
            }

            if (MixCast.TimelapseCameras.Contains(context.Data))
            {
                StopCameraTimelapse();
            }
            else
            {
                StartCameraTimelapse();
            }
        }

        public void StopCameraTimelapse()
        {
            if (context.Data == null)
            {
                return;
            }

            MixCast.TimelapseCameras.Remove(context.Data);
        }

        public void StartCameraTimelapse()
        {
            if (context.Data == null)
            {
                return;
            }

            MixCast.TimelapseCameras.Add(context.Data);
        }
    }
}
#endif
