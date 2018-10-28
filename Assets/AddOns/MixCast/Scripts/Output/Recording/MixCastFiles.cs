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
using System.IO;
using UnityEngine;

namespace BlueprintReality.MixCast
{
    public static class MixCastFiles
    {
        public const string FOLDER_NAME = "MixCast";
        private const string COPY_FILE_PATTERN = "{0} ({1})";

        public static string overrideApplicationName = null;

        public static string GetApplicationDirectory()
        {
            var parentPath = GetOutputDirectory();
            var directoryName = GetApplicationFolderName();
            var path = Path.Combine(parentPath, directoryName);
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetOutputDirectory()
        {
            if (string.IsNullOrEmpty(MixCast.Settings.global.rootOutputPath))
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FOLDER_NAME).Replace("\\", "/");
            else
                return MixCast.Settings.global.rootOutputPath;
        }
        
        public static string GetApplicationFolderName()
        {
            if (string.IsNullOrEmpty(overrideApplicationName))
                return Application.productName;
            else
                return overrideApplicationName;
        }

        public static string GenerateProceduralFilename()
        {
            return DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
        }

        public static string GetAvailableFilename(string origName)
        {
            if (!File.Exists(origName))
                return origName;

            string fileExt = Path.GetExtension(origName) ?? "";
            string withoutExt = origName.Substring(0, origName.Length - fileExt.Length);
            int index = 1;
            while (true)
            {
                string newName = string.Format(COPY_FILE_PATTERN, withoutExt, index) + fileExt;
                if (!File.Exists(newName))
                    return newName;
                index++;
            }
        }
    }
}
#endif
