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
using UnityEngine;
using System.IO;

namespace BlueprintReality.MixCast
{
    public static class SteamConfigUtility
    {
        public const string CFG_FILENAME = "externalcamera.cfg";

        //returns the generated config file path
        public static string Run(string exePath)
        {
            string dataFilePath = GetSrcPath();
            if (!File.Exists(dataFilePath))
            {
                Debug.LogError("MixCast OpenVR data file not found");
                return null;
            }
            string txt = File.ReadAllText(dataFilePath);

            string targetFolderPath = Path.GetDirectoryName(exePath);
            string targetFilePath = Path.Combine(targetFolderPath, CFG_FILENAME);

            File.WriteAllText(targetFilePath, txt);
            return targetFilePath;
        }

        public static bool ShouldRun(string exePath)
        {
            string dst = GetDstPath(exePath);
            if (!File.Exists(dst))
                return true;
            string src = GetSrcPath();
            string srcTxt = File.ReadAllText(src);
            string dstTxt = File.ReadAllText(dst);
            return srcTxt != dstTxt;
        }

        static string GetSrcPath()
        {
            string dataFolderPath = Application.persistentDataPath;
            dataFolderPath = Directory.GetParent(dataFolderPath).FullName;
            dataFolderPath = Path.Combine(dataFolderPath, "MixCast Studio");
            return Path.Combine(dataFolderPath, CFG_FILENAME);
        }
        static string GetDstPath(string exePath)
        {
            string targetFolderPath = Path.GetDirectoryName(exePath);
            return Path.Combine(targetFolderPath, CFG_FILENAME);
        }
    }
}
#endif
