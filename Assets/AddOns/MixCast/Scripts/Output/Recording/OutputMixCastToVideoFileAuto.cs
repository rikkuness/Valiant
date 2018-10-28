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
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintReality.MixCast
{
    [RequireComponent(typeof(GameObject))]
    public class OutputMixCastToVideoFileAuto : OutputMixCastToVideoFile
    {
        public const string FILE_EXT = ".mp4"; //for streaming, please use flv

        public GameObject AudioCallbackObject;

        protected override void OnEnable()
        {
            if (context == null)
                context = GetComponentInParent<CameraConfigContext>();

            if( AudioCallbackObject == null ) {
                if( transform.childCount > 0 ) {
                    AudioCallbackObject = transform.GetChild( 0 ).gameObject;
                } else AudioCallbackObject = gameObject;
            }

            fileName = GenerateFilename();

            base.OnEnable();

            if (encoderRunning == true)
                AudioCallbackObject.SetActive(true);
        }

        protected string GenerateFilename()
        {
            var applicationDirectory = MixCastFiles.GetApplicationDirectory();

            var generatedFileName = string.Format("{0} Camera {1}{2}",
                MixCastFiles.GenerateProceduralFilename(),
                GetCameraIndex()+1,
                FILE_EXT);

            return Path.Combine(applicationDirectory, generatedFileName).Replace("\\", "/");
        }

        protected int GetCameraIndex()
        {
            for (int i = 0; i < MixCastCamera.ActiveCameras.Count; i++)
            {
                var activeCamera = MixCastCamera.ActiveCameras[i];

                if (activeCamera.context.Data.id == context.Data.id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
#endif
