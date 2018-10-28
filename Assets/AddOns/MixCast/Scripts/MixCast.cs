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
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class MixCast
    {
        public const string WEBSITE_URL = "https://mixcast.me/route.php?dest=mixcast";
        public const string VERSION_STRING = "2.0.2";

        public static bool Active { get; protected set; }

        public static event Action MixCastEnabled;
        public static event Action MixCastDisabled;


        public static MixCastData Settings { get; protected set; }
        public static MixCastData.SecureData SecureSettings { get; protected set; }

        public static MixCastProjectSettings ProjectSettings { get; protected set; }

        public static MixCastDesktop Desktop { get; protected set; }

        public static readonly List<MixCastData.CameraCalibrationData> RecordingCameras = new List<MixCastData.CameraCalibrationData>();
        public static readonly List<MixCastData.CameraCalibrationData> StreamingCameras = new List<MixCastData.CameraCalibrationData>();
        public static readonly List<MixCastData.CameraCalibrationData> TimelapseCameras = new List<MixCastData.CameraCalibrationData>();

        public static readonly Dictionary<string, List<MixCastData.CameraCalibrationData>> ActivelyEncoding = new Dictionary<string, List<MixCastData.CameraCalibrationData>>()
        {
            { EventCenter.Category.Recording, new List<MixCastData.CameraCalibrationData>() },
            { EventCenter.Category.Streaming, new List<MixCastData.CameraCalibrationData>() },
        };

        static MixCast()
        {
            ProjectSettings = Resources.Load<MixCastProjectSettings>("MixCast_ProjectSettings");

            if (!Application.isPlaying)
                return;

            Settings = MixCastRegistry.ReadData();
			SecureSettings = MixCastRegistry.ReadSecureData();

            Desktop = new MixCastDesktop();
        }

        public static void SetActive(bool active)
        {
            if (Active == active)
                return;
            if (active && !SdkTamperDetection.CheckFiles())
                return;

            Active = active;
            if (Active)
            {
                if( MixCastEnabled != null )
                    MixCastEnabled();

                EventCenter.Initialize();
                GameObjects.ToastCenter.Initialize();
                Utility.RunWithDelay.RunCoroutine(SetAutoStartForCameras());
            }
            else
            {
                if( MixCastDisabled != null )
                    MixCastDisabled();
                EventCenter.Disable();
                GameObjects.ToastCenter.Disable();
            }

        }

        //Returns true if compareTo is a later version than current
        public static bool IsVersionBLaterThanVersionA(string versionA, string versionB)
        {
            if (versionA == versionB)
                return false;

            string[] versionBNums = versionB.Split('.');
            string[] versionANums = versionA.Split('.');

            for (int i = 0; i < versionBNums.Length && i < versionANums.Length; i++)
            {
                try {
                    int versionBNum = int.Parse(versionBNums[i]);
                    int versionANum = int.Parse(versionANums[i]);

                    if (versionBNum > versionANum)
                        return true;
                    else if (versionBNum < versionANum)
                        return false;

                } catch (FormatException) {
                    Debug.LogError("version check failed due to invalid string format");
                    return false;
                }
            }

            if (versionBNums.Length > versionANums.Length)
                return true;

            return false;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("MixCast/Go to Website")]
#endif
        public static void GoToWebsite()
        {
            Application.OpenURL(WEBSITE_URL);
        }

        static System.Collections.IEnumerator SetAutoStartForCameras()
        {
            if (Settings == null || Settings.cameras == null)
            {
                yield break;
            }

            while(BlueprintReality.Utility.ScreenUtility.AreAllScenesLoaded() == false) {
                yield return new WaitForFixedUpdate();
            }
            foreach (var cam in Settings.cameras)
            {
                AudioAsyncFeed.Instance( cam.id ); // make sure it actually starts decoding first
                Utility.RunWithDelay.NextFrame( () => SetAutoStartForCamera( cam ) );
            }
        }

        static void SetAutoStartForCamera(MixCastData.CameraCalibrationData cam)
        {
            if (cam == null || cam.recordingData == null)
            {
                return;
            }

            AutoStartRecording(cam);
            AutoStartStreaming(cam);
            AutoStartTimelapse(cam);
        }

        static void AutoStartRecording(MixCastData.CameraCalibrationData cam)
        {
            if (!cam.recordingData.autoStartRecording)
            {
                return;
            }

            RecordingCameras.Add(cam);
        }

        static void AutoStartStreaming(MixCastData.CameraCalibrationData cam)
        {
            if (!cam.recordingData.autoStartStreaming)
            {
                return;
            }

            var cameraStreamUrl = StreamingServiceUtility.ConstructStreamUrl(cam.recordingData);
            var globalStreamUrl = StreamingServiceUtility.ConstructStreamUrl(Settings.global);

            var isStreamingConfigured = !string.IsNullOrEmpty(cameraStreamUrl) ||
                                        !string.IsNullOrEmpty(globalStreamUrl);

            if (!isStreamingConfigured)
            {
                return;
            }

            StreamingCameras.Add(cam);
        }

        static void AutoStartTimelapse(MixCastData.CameraCalibrationData cam)
        {
            if (!cam.recordingData.autoStartTimelapse)
            {
                return;
            }

            TimelapseCameras.Add(cam);
        }

        public static bool IsRecordingOrStreaming( bool recording = true, bool streaming = true ) {
            int index = -1;
            if( MixCast.Desktop.displayingCameras.Count == 0 ) {
                return false;
            }
            var cam = MixCast.Desktop.displayingCameras[0];
            if( cam == null ) {
                return false;
            }

            string camId = cam.id;

            if( recording ) {
                index = Utility.Find.Index<MixCastData.CameraCalibrationData>( MixCast.RecordingCameras, ( rc ) => rc != null && rc.id == camId );
                if( index >= 0 ) return true;
            }
            if( streaming ) {
                index = Utility.Find.Index<MixCastData.CameraCalibrationData>( MixCast.StreamingCameras, ( sc ) => sc != null && sc.id == camId );
                if( index >= 0 ) return true;
            }
            return false;
        }
    }
}
#endif
