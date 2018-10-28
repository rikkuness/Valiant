using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Intel.RealSense;

namespace BlueprintReality.MixCast.RealSense
{
    public class RealSenseUtility
    {
        // It could be expensive, do not call it very often
        public static Device GetDeviceFromAltName(string requestAltName)
        {
            Context rs_context = new Context();
            DeviceList deviceList = rs_context.QueryDevices();

            for (int i = 0; i < deviceList.Count; i++)
            {
                var rs_device = deviceList[i];
                var device_port = rs_device.Info[CameraInfo.PhysicalPort];
                var unique_id = device_port.Split('&')[3];
                if (requestAltName.Contains(unique_id))
                {
                    return rs_device;
                }
            }

            return null;
        }

        // It could be expensive, do not call it very often
        public static string GetDeviceSerialFromAltName(string requestAltName)
        {
            var request_device = GetDeviceFromAltName(requestAltName);
            if (request_device != null)
            {
                return request_device.Info[CameraInfo.SerialNumber];
            }
            return null;
        }

        public static RealSenseDevice GetRealSenseMixCastDeviceFromAltName(string requestAltName)
        {
            foreach (var feed in InputFeed.ActiveFeeds)
            {
                if (feed is RealSenseInputFeed)
                {
                    var rs_feed = feed as RealSenseInputFeed;
                    var rs_device = rs_feed.device;
                    if (rs_device != null)
                    {
                        var unique_id = rs_device.ActiveProfile.Device.Info[CameraInfo.PhysicalPort].Split('&')[3];
                        if (requestAltName.Contains(unique_id))
                        {
                            return rs_device;
                        }
                    }
                }
            }
            return null;
        }

        public static void SetOption(Sensor sensor, Option option, float normalizedValue)
        {
            if (normalizedValue < 0)
            {
                return;
            }

            try
            {
                // Convert Value to expand effective data range for small value
                if (option.Equals(Option.Exposure))
                {
                    normalizedValue = normalizedValue * normalizedValue * normalizedValue;
                }
                var cameraOption = sensor.Options[option];
                var value = normalizedValue * (cameraOption.Max - cameraOption.Min) + cameraOption.Min;
                sensor.Options[option].Value = value;
            }
            catch
            {
                // should we show a toast here?
                return;
            }
        }
    }
}
