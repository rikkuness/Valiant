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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;

namespace BlueprintReality.MixCast
{
    public class OutputMixCastToScreenshot : CameraComponent
    {
        private static List<OutputMixCastToScreenshot> encodeQueue = new List<OutputMixCastToScreenshot>();

        public string filename = "image.jpg";

        private List<Thread> activeSaves = new List<Thread>();

        //static string savedImage = null;

        protected override void OnDisable()
        {
            activeSaves.ForEach(ar =>
            {
                if (ar.IsAlive)
                {
                    ar.Abort();
                }
            });
            activeSaves.Clear();
            base.OnDisable();
        }

        public virtual void Run()
        {
            StartCoroutine(RunAsync());
        }

        private void ScreenshotError()
        {
            EventCenter.HandleEvent(
                    EventCenter.Category.Saving,
                    EventCenter.Result.Error,
                    Text.Localization.Get("Warning_Screenshot_Error"),
                    false
                    );
            Debug.Log("Error saving screenshot");
        }

        IEnumerator RunAsync()
        {

            MixCastCamera cam = MixCastCamera.FindCamera(context);
            if (cam == null)
            {
                ScreenshotError();
                yield break;
            }
            
            RenderTexture srcTex = RenderTexture.GetTemporary(cam.Output.width, cam.Output.height, 0);
            Graphics.Blit(cam.Output, srcTex);

            //Distribute encoding so only one texture encodes per frame (since not threadable)
            encodeQueue.Add(this);
            yield return new WaitForEndOfFrame();
            while (encodeQueue[0] != this)
            {
                if (encodeQueue[0] == null)
                {
                    encodeQueue.RemoveAt(0);    //mechanism so 2nd instance still doesn't trigger same frame
                }     
                yield return null; // waits for next frame; used to spread out function over multiple frames
            }

            //Reserve file
            string finalFilename = MixCastFiles.GetAvailableFilename(filename);

            yield return null;

            Texture2D tex = new Texture2D(cam.Output.width, cam.Output.height, TextureFormat.RGB24, false, QualitySettings.activeColorSpace == ColorSpace.Linear);

            yield return null;

            RenderTexture.active = srcTex;
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            RenderTexture.active = null;

            yield return null;

            srcTex.Release();
            srcTex = null;

            yield return null;

            JPGEncoder encoder = new JPGEncoder(tex, 100, finalFilename);

            yield return null;

            DestroyImmediate(tex);

            while (!encoder.isDone)
            {
                yield return null;
            }

            encodeQueue[0] = null;  //Release encoding lock

            EventCenter.HandleEvent(
                    EventCenter.Category.Saving,
                    EventCenter.Result.Success,
                    string.Format("{0} {1}", Text.Localization.Get("Info_Saved_Screenshot"), finalFilename.Replace("/", "\\")),
                    false
                    );
        }

        private void Update() {

            /* This appears to be deprecated code, as saving screenshots does not depend on it
             
            // to fix a main-thread-only issue
            if(!string.IsNullOrEmpty( savedImage )) {
                EventCenter.HandleEvent(
                    EventCenter.Category.Saving,
                    EventCenter.Result.Success,
                    string.Format( "{0} {1}", Text.Localization.Get( "Info_Saved_Screenshot" ), savedImage.Replace("/", "\\")), 
                    false
                    );

                Debug.Log("Screenshot saved successfully");
                savedImage = null;
            }
            */
        }
    }
}
#endif
