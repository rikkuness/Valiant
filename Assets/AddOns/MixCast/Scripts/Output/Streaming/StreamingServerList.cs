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

namespace BlueprintReality.MixCast
{
    public static class StreamingServerList
    {
        public static readonly Dictionary<MixCastData.StreamingService, StreamingServer[]> serviceUrls = new Dictionary<MixCastData.StreamingService, StreamingServer[]>()
        {
            {
                MixCastData.StreamingService.Facebook, new []
                {
                    new StreamingServer { name = "Default", url = "rtmp://rtmp-api.facebook.com:80/rtmp/" },
                }
            },
            {
                MixCastData.StreamingService.Mixer, new []
                {
                    // https://watchbeam.zendesk.com/hc/en-us/articles/209659883-How-to-change-your-Ingest-Server
                    new StreamingServer { name = "Asia: Hong Kong",      url = "rtmp://ingest-hkg.mixer.com:1935/beam/" },
                    new StreamingServer { name = "Asia: Tokyo",          url = "rtmp://ingest-tok.mixer.com:1935/beam/" },
                    new StreamingServer { name = "Australia: Melbourne", url = "rtmp://ingest-mel.mixer.com:1935/beam/" },
                    new StreamingServer { name = "Australia: Sydney",    url = "rtmp://ingest-syd.mixer.com:1935/beam/" },
                    new StreamingServer { name = "Brazil: Sao Paulo",    url = "rtmp://ingest-sao.mixer.com:1935/beam/" },
                    new StreamingServer { name = "Canada: Toronto",      url = "rtmp://ingest-tor.mixer.com:1935/beam/" },
                    new StreamingServer { name = "EU: Amsterdam",        url = "rtmp://ingest-ams.mixer.com:1935/beam/" },
                    new StreamingServer { name = "EU: Frankfurt",        url = "rtmp://ingest-fra.mixer.com:1935/beam/" },
                    new StreamingServer { name = "EU: London",           url = "rtmp://ingest-lon.mixer.com:1935/beam/" },
                    new StreamingServer { name = "EU: Milan",            url = "rtmp://ingest-mil.mixer.com:1935/beam/" },
                    new StreamingServer { name = "EU: Oslo",             url = "rtmp://ingest-osl.mixer.com:1935/beam/" },
                    new StreamingServer { name = "EU: Paris",            url = "rtmp://ingest-par.mixer.com:1935/beam/" },
                    new StreamingServer { name = "India: Chennai",       url = "rtmp://ingest-che.mixer.com:1935/beam/" },
                    new StreamingServer { name = "Mexico: Mexico City",  url = "rtmp://ingest-mex.mixer.com:1935/beam/" },
                    new StreamingServer { name = "South Korea: Seoul",   url = "rtmp://ingest-seo.mixer.com:1935/beam/" },
                    new StreamingServer { name = "US: Dallas, TX",       url = "rtmp://ingest-dal.mixer.com:1935/beam/" },
                    new StreamingServer { name = "US: San Jose, CA",     url = "rtmp://ingest-sjc.mixer.com:1935/beam/" },
                    new StreamingServer { name = "US: Seattle",          url = "rtmp://ingest-sea.mixer.com:1935/beam/" },
                    new StreamingServer { name = "US: Washington DC",    url = "rtmp://ingest-wdc.mixer.com:1935/beam/" },
                }
            },
            {
                MixCastData.StreamingService.Twitch, new []
                {
                    // https://stream.twitch.tv/ingests/
                    new StreamingServer { name = "Asia: Hong Kong",                       url = "rtmp://live-hkg.twitch.tv/app/" },
                    new StreamingServer { name = "Asia: Seoul, South Korea",              url = "rtmp://live-sel.twitch.tv/app/" },
                    new StreamingServer { name = "Asia: Singapore",                       url = "rtmp://live-sin.twitch.tv/app/" },
                    new StreamingServer { name = "Asia: Taipei, Taiwan",                  url = "rtmp://live-tpe.twitch.tv/app/" },
                    new StreamingServer { name = "Asia: Tokyo, Japan",                    url = "rtmp://live-tyo.twitch.tv/app/" },
                    new StreamingServer { name = "Australia: Sydney",                     url = "rtmp://live-syd.twitch.tv/app/" },
                    new StreamingServer { name = "Default",                               url = "rtmp://live.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Amsterdam, NL",                     url = "rtmp://live-ams.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Berlin, Germany",                   url = "rtmp://live-ber.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Frankfurt, Germany",                url = "rtmp://live-fra.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Lisbon, Portugal",                  url = "rtmp://live-lis.twitch.tv/app/" },
                    new StreamingServer { name = "EU: London, UK",                        url = "rtmp://live-lhr.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Madrid, Spain",                     url = "rtmp://live-mad.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Milan, Italy",                      url = "rtmp://live-mil.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Paris, FR",                         url = "rtmp://live-cdg.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Prague, CZ",                        url = "rtmp://live-prg.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Stockholm, Sweden",                 url = "rtmp://live-arn.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Vienna, Austria",                   url = "rtmp://live-vie.twitch.tv/app/" },
                    new StreamingServer { name = "EU: Warsaw, Poland",                    url = "rtmp://live-waw.twitch.tv/app/" },
                    new StreamingServer { name = "Helsinki",                              url = "rtmp://live-hel.twitch.tv/app/" },
                    new StreamingServer { name = "Marseille, France",                     url = "rtmp://live-mrs.twitch.tv/app/" },
                    new StreamingServer { name = "NA: Mexico",                            url = "rtmp://live-qro.twitch.tv/app/" },
                    new StreamingServer { name = "NA: Quebec, Canada",                    url = "rtmp://live-ymq.twitch.tv/app/" },
                    new StreamingServer { name = "NA: Toronto, Canada",                   url = "rtmp://live-yto.twitch.tv/app/" },
                    new StreamingServer { name = "South America: Argentina",              url = "rtmp://live-eze.twitch.tv/app/" },
                    new StreamingServer { name = "South America: Chile",                  url = "rtmp://live-scl.twitch.tv/app/" },
                    new StreamingServer { name = "South America: Lima, Peru",             url = "rtmp://live-lim.twitch.tv/app/" },
                    new StreamingServer { name = "South America: Medellin, Colombia",     url = "rtmp://live-mde.twitch.tv/app/" },
                    new StreamingServer { name = "South America: Rio de Janeiro, Brazil", url = "rtmp://live-rio.twitch.tv/app/" },
                    new StreamingServer { name = "South America: Sao Paulo, Brazil",      url = "rtmp://live-sao.twitch.tv/app/" },
                    new StreamingServer { name = "US Central: Dallas, TX",                url = "rtmp://live-dfw.twitch.tv/app/" },
                    new StreamingServer { name = "US Central: Denver, CO",                url = "rtmp://live-den.twitch.tv/app/" },
                    new StreamingServer { name = "US Central: Houston, TX",               url = "rtmp://live-hou.twitch.tv/app/" },
                    new StreamingServer { name = "US Central: Salt Lake City, UT",        url = "rtmp://live-slc.twitch.tv/app/" },
                    new StreamingServer { name = "US East: Ashburn, VA",                  url = "rtmp://live-iad.twitch.tv/app/" },
                    new StreamingServer { name = "US East: Atlanta, GA",                  url = "rtmp://live-atl.twitch.tv/app/" },
                    new StreamingServer { name = "US East: Chicago",                      url = "rtmp://live-ord.twitch.tv/app/" },
                    new StreamingServer { name = "US East: Miami, FL",                    url = "rtmp://live-mia.twitch.tv/app/" },
                    new StreamingServer { name = "US East: New York, NY",                 url = "rtmp://live-jfk.twitch.tv/app/" },
                    new StreamingServer { name = "US West: Los Angeles, CA",              url = "rtmp://live-lax.twitch.tv/app/" },
                    new StreamingServer { name = "US West: Phoenix, AZ",                  url = "rtmp://live-phx.twitch.tv/app/" },
                    new StreamingServer { name = "US West: Portland, Oregon",             url = "rtmp://live-pdx.twitch.tv/app/" },
                    new StreamingServer { name = "US West: San Francisco, CA",            url = "rtmp://live-sfo.twitch.tv/app/" },
                    new StreamingServer { name = "US West: San Jose, CA",                 url = "rtmp://live-sjc.twitch.tv/app/" },
                    new StreamingServer { name = "US West: Seattle, WA",                  url = "rtmp://live-sea.twitch.tv/app/" },
                }
            },
            {
                MixCastData.StreamingService.Twitter, new []
                {
                    new StreamingServer { name = "Asia Pacific (Tokyo)",      url = "rtmp://jp.pscp.tv:80/x/" },
                    new StreamingServer { name = "Asia Pacific (Seoul)",      url = "rtmp://jp.pscp.tv:80/x/" },
                    new StreamingServer { name = "Asia Pacific (Mumbai)",     url = "rtmp://sg.pscp.tv:80/x/" },
                    new StreamingServer { name = "Asia Pacific (Singapore)",  url = "rtmp://sg.pscp.tv:80/x/" },
                    new StreamingServer { name = "Asia Pacific (Sydney)",     url = "rtmp://au.pscp.tv:80/x/" },
                    new StreamingServer { name = "Canada (Central)",          url = "rtmp://va.pscp.tv:80/x/" },
                    new StreamingServer { name = "EU (Frankfurt)",            url = "rtmp://de.pscp.tv:80/x/" },
                    new StreamingServer { name = "EU (Ireland)",              url = "rtmp://ie.pscp.tv:80/x/" },
                    new StreamingServer { name = "South America (São Paulo)", url = "rtmp://br.pscp.tv:80/x/" },
                    new StreamingServer { name = "US East (N. Virginia)",     url = "rtmp://va.pscp.tv:80/x/" },
                    new StreamingServer { name = "US East (Ohio)",            url = "rtmp://va.pscp.tv:80/x/" },
                    new StreamingServer { name = "US West (Oregon)",          url = "rtmp://or.pscp.tv:80/x/" },
                    new StreamingServer { name = "US West (N. California)",   url = "rtmp://ca.pscp.tv:80/x/" },
                }
            },
            {
                MixCastData.StreamingService.YouTube, new []
                {
                    new StreamingServer { name = "Primary Server", url = "rtmp://a.rtmp.youtube.com/live2/" },
                    new StreamingServer { name = "Backup Server",  url = "rtmp://b.rtmp.youtube.com/live2?backup=1" },
                }
            },
        };
    }
}
#endif
