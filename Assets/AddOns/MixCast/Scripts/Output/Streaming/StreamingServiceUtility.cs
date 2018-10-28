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

namespace BlueprintReality.MixCast
{
    public static class StreamingServiceUtility
    {
        const string rtmpScheme = "rtmp://";

        /// <summary>
        /// Creates the URL to stream to.
        /// </summary>
        public static string ConstructStreamUrl(MixCastData.GlobalData data, bool debug = false)
        {
            return ConstructStreamUrl(data.defaultStreamService, data.defaultStreamUrl, data.defaultStreamKey, debug);
        }

        /// <summary>
        /// Creates the URL to stream to.
        /// </summary>
        public static string ConstructStreamUrl(MixCastData.RecordingData data, bool debug = false)
        {
            return ConstructStreamUrl(data.perCamStreamService, data.perCamStreamUrl, data.perCamStreamKey, debug);
        }

        /// <summary>
        /// Creates the URL to stream to.
        /// </summary>
        public static string ConstructStreamUrl(MixCastData.StreamingService service, string streamUrl, string streamKey, bool debug = false)
        {
            switch (service)
            {
                case MixCastData.StreamingService.None:
                    return null;

                case MixCastData.StreamingService.Custom:
                    if (string.IsNullOrEmpty(streamUrl))
                    {
                        return null;
                    }

                    if (!string.IsNullOrEmpty(streamKey))
                    {
                        streamUrl = streamUrl.Trim('/') + "/" + streamKey;
                    }

                    if (!streamUrl.StartsWith(rtmpScheme))
                    {
                        streamUrl = rtmpScheme + streamUrl;
                    }

                    break;

                default:
                    StreamingServer[] servers;

                    if (!StreamingServerList.serviceUrls.TryGetValue(service, out servers))
                    {
                        var message = string.Format("Haven't implemented {0} streaming yet", service);
                        throw new NotImplementedException(message);
                    }

                    if (string.IsNullOrEmpty(streamKey))
                    {
                        return null;
                    }

                    streamUrl = servers[0].url + streamKey;
                    break;
            }

            return AppendServiceParams(service, streamUrl, debug);
        }

        /// <summary>
        /// Adds service-specific parameters to the URL string.
        /// </summary>
        static string AppendServiceParams(MixCastData.StreamingService service, string url, bool debug)
        {
            switch (service)
            {
                case MixCastData.StreamingService.Twitch:
                    var debugParam = debug ? "?bandwidthtest=true" : "";
                    return url + debugParam;

                default:
                    return url;
            }
        }
    }
}
#endif
