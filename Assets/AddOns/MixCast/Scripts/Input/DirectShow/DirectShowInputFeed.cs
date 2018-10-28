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
using BlueprintReality.GameObjects;
using BlueprintReality.MixCast.RealSense;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class DirectShowInputFeed : InputFeed
    {
        private string lastName = "";
        private int lastWidth, lastHeight, lastFramerate;
        private string lastPixelFormat = "";

        private DirectShowInputFeedStream wTexture = null;
        //bool deviceFound = false;


        public override Texture Texture
        {
            get
            {
                if (wTexture != null)
                    return wTexture.Texture;
                else
                    return null;
            }
        }

        protected void Update()
        {
			if (wTexture != null)
				if (wTexture.Texture != null)
					wTexture.RenderFrame();
        }

        protected override void LateUpdate()
        {
            if( BlueprintReality.Utility.ScreenUtility.AreAllScenesLoaded() == false ) {
                return;
            }

            if (context != null && context.Data != null)
            {
                /*
                // old way with gc alloc
				string realSenseFilter = context.Data.deviceName.ToLower();
				if (realSenseFilter.Contains("realsense"))
				{
					if (!realSenseFilter.Contains("sr300") && !realSenseFilter.Contains("rgb"))
						return;
				}
                */

                // alternative way with no gc alloc
                string realSenseFilter = context.Data.deviceName;
                if (realSenseFilter.IndexOf("realsense", System.StringComparison.OrdinalIgnoreCase) != -1) // does contain
                {
                    if (realSenseFilter.IndexOf("sr300", System.StringComparison.OrdinalIgnoreCase) == -1) // does not contain
                    {
                        if (realSenseFilter.IndexOf("rgb", System.StringComparison.OrdinalIgnoreCase) == -1) // does not contain
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                return;
            }

            base.LateUpdate();

            if( isResettingCamera || !wasCameraConnected ) {
                return;
            }

            if(lastName != context.Data.deviceAltName || lastWidth != context.Data.deviceFeedWidth || lastHeight != context.Data.deviceFeedHeight || lastFramerate != context.Data.deviceFramerate || lastPixelFormat != context.Data.devicePixelFormat)
            {
                context.Data.unplugged = false;
                ClearTexture();

                SetTexture();
            }

            
        }

        protected override void SetTexture()
        {
            string devName = context.Data.deviceName;
            string devAltName = context.Data.deviceAltName;

            for (int i = 0; i < RealSenseInputFeed.DEVICE_NAMES.Length; i++)
                if (devName == RealSenseInputFeed.DEVICE_NAMES[i])
                    return;

            int width = context.Data.deviceFeedWidth;
            int height = context.Data.deviceFeedHeight;
            int framerate = context.Data.deviceFramerate;
            string pixfmt = context.Data.devicePixelFormat;

            if (devName == "" || string.IsNullOrEmpty(devAltName) || width == 0 || height == 0 || framerate == 0)
            {
				if (wTexture != null)
					wTexture.Stop();

                wTexture = null;
                return;
            }
            else
            {
                wTexture = new DirectShowInputFeedStream(devAltName, width, height, framerate, pixfmt, MixCastAV.RT_BUFSIZE_DEFAULT, MixCastAV.FORCE_RGBA);
            }

            lastName = devAltName;
            lastWidth = width;
            lastHeight = height;
            lastFramerate = framerate;
            lastPixelFormat = pixfmt;
            context.Data.isInUseElsewhere = false;

            if (wTexture != null)
            {
                if (wTexture.frameDataSize > 0)
                {
                    wTexture.Play();
                }
                else
                {
                    Debug.LogError("Error! Device could not be accessed. [" + devName + "] " + width + "x" + height + ", " + framerate + "fps (" + pixfmt + ")");
                    context.Data.isInUseElsewhere = true;
                    wTexture.Stop();
                    wTexture = null;
                    ShowInUseElsewherePopup();
                }
            }
            else if (wTexture == null)
            {
                return;
            }
        }

        protected override void ClearTexture()
        {
            if (wTexture != null) //&& wTexture.Texture != null
            {
                wTexture.Stop();
                wTexture = null;

                lastName = "";
                lastWidth = 0;
                lastHeight = 0;
                lastFramerate = 0;
                lastPixelFormat = "";
            }
        }

        void ShowInUseElsewherePopup()
        {
            var popUpWindow = GetComponent<OpenPopupWindow>();

            if (!popUpWindow)
            {
                return;
            }

            popUpWindow.Open();
        }
    }
}
#endif
