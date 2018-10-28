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
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;


namespace BlueprintReality.MixCast
{
    public class DirectShowInputFeedStream
    {
		private IntPtr vidDec = IntPtr.Zero; //decoder codec pointer
		private IntPtr cfgVidDec = IntPtr.Zero; //cfg for video pointer
		private IntPtr vidTxfDec = IntPtr.Zero; //video transform pointer
		private int decoderInterface = -1;

        public virtual int width { get; set; }
        public virtual int height { get; set; }
        private ulong udatasize = 0;
        public ulong frameDataSize { get { return udatasize; } }
        public Texture Texture { get; protected set; }

        private volatile bool startRun = false;
        public bool getStartRun {  get { return startRun; } }

        /// <summary>
        ///     WebcamFeedLibAv(StringBuilder deviceName, int w, int h, int fps, StringBuilder pixFmtStr, bool forceRGB = true)
        ///     Build the decoder with the info supplied in the parameters. This is non-threaded.
        /// </summary>
        /// <param name="deviceName">The device name which supports alternative hardware name</param>
        /// <param name="w">The requested width of the device and also the width of the texture</param>
        /// <param name="h">The requested height of the device and also the height of the texture</param>
        /// <param name="fps">The requested framerate of device</param>
        /// <param name="pixFmtStr">The requested pixel format of the supported device</param>
        /// <param name="forceRGB">Toggle Forced RGB24 for texture</param>
        public DirectShowInputFeedStream(string deviceName, int w, int h, int fps, string pixFmtStr, int rtBufSize, int forceRGB = MixCastAV.FORCE_RGBA)
        {
            width = w;
            height = h;

			if (cfgVidDec != IntPtr.Zero || vidDec != IntPtr.Zero || vidTxfDec != IntPtr.Zero)
				return;

			cfgVidDec = MixCastAV.getVideoDecodeCfg(deviceName, w, h, fps, pixFmtStr, w, h, fps, MixCastAV.texturePixelFormat, 1);

			if (vidDec == IntPtr.Zero)
				vidDec = MixCastAV.getVideoDecodeContext(cfgVidDec);
			
			if (vidDec == IntPtr.Zero)
			{
				Texture = null;
				return;
			}
			
			vidTxfDec = MixCastAV.getVideoTransformContext(cfgVidDec);

			if (vidTxfDec != IntPtr.Zero && vidDec != IntPtr.Zero && cfgVidDec != IntPtr.Zero)
				Debug.Log("Started Device Feed");
			else
			{
				Texture = null;
				return;
			}
			
			udatasize = MixCastAV.getCfgOutputDataSize(cfgVidDec);

			//initialize the texture format
			Texture = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            Texture.wrapMode = TextureWrapMode.Clamp;

			// Initialize decoder camera thread
			decoderInterface = MixCastAV.CreateDecodeInterface(vidDec, cfgVidDec, vidTxfDec);
			MixCastAV.SetDecodeInterfaceTexture(decoderInterface, Texture.GetNativeTexturePtr());
        }

        public void RenderFrame()
        {
            GL.IssuePluginEvent(MixCastAV.GetDecodeInterfaceRenderCallback(), decoderInterface);
        }

        public void Stop()
        {
			if (vidTxfDec != IntPtr.Zero || vidDec != IntPtr.Zero || cfgVidDec != IntPtr.Zero)
				_killDecoder();

            //throw away the texture
            UnityEngine.Object.Destroy(Texture);
            Texture = null;

            startRun = false;
        }

        public void Play()
        {
            startRun = true;
            MixCastAV.StartDecodeInterface(decoderInterface);
        }

        private void _killDecoder()
        {
            bool resFreeDec = false;
			bool resFreeCfg = false;
			bool resFreeTxf = false;
			
			MixCastAV.ReleaseDecodeInterface(decoderInterface);
			System.Threading.Thread.Sleep(2); //untested amount of sleep time in ms needed to avoid race condition


			//free the decoder
			if (vidDec != IntPtr.Zero)
				resFreeDec = MixCastAV.freeDecodeContext(vidDec) == 0 ? true : false;
			vidDec = IntPtr.Zero;

			//free the data config
			if (cfgVidDec != IntPtr.Zero)
				resFreeCfg = MixCastAV.freeVideoCfg(cfgVidDec) == 0 ? true : false;
			cfgVidDec = IntPtr.Zero;

			//free the transformer
			if (vidTxfDec != IntPtr.Zero)
				resFreeTxf = MixCastAV.freeVideoTransform(vidTxfDec) == 0 ? true : false;
			vidTxfDec = IntPtr.Zero;


			if (resFreeDec == false || resFreeCfg == false || resFreeTxf == false)
            {
                Debug.LogError("Error Freeing Device Feed. " + vidDec);
            }
        }

        protected void CompleteFrame()
        {
        }

    }// Public Class
}//namespace
#endif
