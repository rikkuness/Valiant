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

namespace BlueprintReality.MixCast
{
    [RequireComponent(typeof(GameObject))]
    public class OutputMixCastToUserNetwork : OutputMixCastToNetwork
    {
        public bool debug = false;

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
                

            var cameraSpecific = context.Data != null &&
                                 context.Data.recordingData.perCamStreamService != MixCastData.StreamingService.None;

            _uriOutput = cameraSpecific ?
                StreamingServiceUtility.ConstructStreamUrl(context.Data.recordingData, debug) :
                StreamingServiceUtility.ConstructStreamUrl(MixCast.Settings.global, debug);

            base.OnEnable();

            if (encoderRunning == true)
                AudioCallbackObject.SetActive(true);
        }
    }
}
#endif
