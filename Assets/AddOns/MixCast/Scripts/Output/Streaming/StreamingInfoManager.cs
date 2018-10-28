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
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class StreamingInfoManager : MonoBehaviour
    {
        public static readonly string PLATFORM_LIVE = "Live";
        public static readonly string PLATFORM_VIDEO = "Video";

        static readonly Dictionary<MixCastData.StreamingService, Dictionary<string, Dictionary<string, string>>>
            streamInfo = new Dictionary<MixCastData.StreamingService, Dictionary<string, Dictionary<string, string>>>
            {
                {
                    MixCastData.StreamingService.Facebook, new Dictionary<string, Dictionary<string, string>>
                    {
                        {
                            PLATFORM_LIVE, new Dictionary<string, string>
                            {
                                {"30", "1280x720"},
                                {"sampleRates", "44100"}
                            }
                        },
                        {
                            PLATFORM_VIDEO, new Dictionary<string, string>
                            {
                                {"30", "1280x720"},
                                {"sampleRates", "44100"}
                            }
                        }
                    }
                },
                {
                    MixCastData.StreamingService.Mixer, new Dictionary<string, Dictionary<string, string>>
                    {
                        {
                            PLATFORM_LIVE, new Dictionary<string, string>
                            {
                                {"30", "1920x1080,1280x720"},
                                {"60", "1920x1080,1280x720"},
                                {"sampleRates", "44100"}
                            }
                        }
                    }
                },
                {
                    MixCastData.StreamingService.Twitter, new Dictionary<string, Dictionary<string, string>>
                    {
                        {
                            PLATFORM_LIVE, new Dictionary<string, string>
                            {
                                {"30", "1280x720,960x540"},
                                {"sampleRates", "44100"}
                            }
                        }
                    }
                },
                {
                    MixCastData.StreamingService.YouTube, new Dictionary<string, Dictionary<string, string>>
                    {
                        {
                            PLATFORM_VIDEO, new Dictionary<string, string>
                            {
                                {"48", "3840x2160,2560x1440,1920x1080,1280x720,854x480,640x360"},
                                {"50", "3840x2160,2560x1440,1920x1080,1280x720,854x480,640x360"},
                                {"60", "3840x2160,2560x1440,1920x1080,1280x720,854x480,640x360"},
                                {"24", "3840x2160,2560x1440,1920x1080,1280x720,854x480,640x360"},
                                {"25", "3840x2160,2560x1440,1920x1080,1280x720,854x480,640x360"},
                                {"30", "3840x2160,2560x1440,1920x1080,1280x720,854x480,640x360"},
                                {"sampleRates", "48000,96000"}
                            }
                        },
                        {
                            PLATFORM_LIVE, new Dictionary<string, string>
                            {
                                {"30", "3840x2160,2560x1440,1920x1080,1280x720,854x480,640x360,426x240"},
                                {"60", "3840x2160,2560x1440,1920x1080,1280x720"},
                                {"sampleRates", "44100"}
                            }
                        }
                    }
                },
                /*
                {
                    MixCastData.StreamingService.Instagram, new Dictionary<string, Dictionary<string, string>> 
                    {
                        {
                            PLATFORM_VIDEO, new Dictionary<string, string> 
                            {
                                { "30", "1920x1080,1280x720,1800x945,1080x1080,864x1080" },
                                { "sampleRates", "44100" }
                            }
                        }
                    }
                },
                {
                    MixCastData.StreamingService.Vimeo, new Dictionary<string, Dictionary<string, string>> 
                    {
                        {
                            PLATFORM_VIDEO, new Dictionary<string, string> 
                            {
                                { "23.98", "8192x4320,3840x2160,2560x1440,1920x1080,1280x720,640x360" },
                                { "24",    "8192x4320,3840x2160,2560x1440,1920x1080,1280x720,640x360" },
                                { "25",    "8192x4320,3840x2160,2560x1440,1920x1080,1280x720,640x360" },
                                { "29.97", "8192x4320,3840x2160,2560x1440,1920x1080,1280x720,640x360" },
                                { "30",    "8192x4320,3840x2160,2560x1440,1920x1080,1280x720,640x360" },
                                { "50",    "8192x4320,3840x2160,2560x1440,1920x1080,1280x720,640x360" },
                                { "59.94", "8192x4320,3840x2160,2560x1440,1920x1080,1280x720,640x360" },
                                { "60",    "8192x4320,3840x2160,2560x1440,1920x1080,1280x720,640x360" },
                                { "sampleRates", "48000" }
                            }
                        }
                    }
                }
                */
            };

        public static bool GetConfigSupportStringID(MixCastData.StreamingService service, string targetFormat,
            string fps, string res, ref string outString)
        {
            try
            {
                var platform = streamInfo[service];
                if (platform == null)
                {
                    return false;
                }

                var format = platform[targetFormat];
                if (format == null)
                {
                    return false;
                }

                if (format.ContainsKey(fps))
                {
                    if (System.Convert.ToInt32(fps) == 30)
                    {
                        outString = "";
                        return true;
                    }

                    if (format[fps].Contains(res))
                    {
                        outString = "Field_StreamConfigWarn";
                        return true;
                    }
                    else
                    {
                        outString = "Field_ResolutionStreamWarn";
                        return false;
                    }
                }
                else
                {
                    outString = "Field_FPSStreamWarn";
                    return false;
                }
            }
            catch
            {
            }

            return false;
        }

        public static bool SupportsConfig(MixCastData.StreamingService service, string targetFormat, string fps,
            string res, string sampleRate)
        {
            try
            {
                var platform = streamInfo[service];
                if (platform == null)
                {
                    return false;
                }

                var format = platform[targetFormat];
                if (format == null)
                {
                    return false;
                }

                if (format.ContainsKey(fps) && format[fps].Contains(res))
                {
                    if (format["sampleRates"].Contains(sampleRate))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        public static bool SupportsConfig(MixCastData.StreamingService service, string targetFormat, string fps,
            string res)
        {
            try
            {
                var platform = streamInfo[service];
                if (platform == null)
                {
                    return false;
                }

                var format = platform[targetFormat];
                if (format == null)
                {
                    return false;
                }

                if (format.ContainsKey(fps) && format[fps].Contains(res))
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        public static bool SupportsSampleRate(MixCastData.StreamingService service, string targetFormat,
            string sampleRate)
        {
            try
            {
                var platform = streamInfo[service];
                if (platform == null)
                {
                    return false;
                }

                var format = platform[targetFormat];
                if (format == null)
                {
                    return false;
                }

                if (format["sampleRates"].Contains(sampleRate))
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
#endif
