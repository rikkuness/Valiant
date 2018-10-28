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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

using UnityEngine;

namespace BlueprintReality.MixCast
{
    public class SdkTamperDetection
    {
        [Serializable]
        public class TamperDetectionFileEntry
        {
            public string rootSearchPath;
            public string filename;
            public byte[] hash;
            public bool mustBeExact = false;
        }

        private static SHA1 _HashProvider;

        static SdkTamperDetection()
        {
            _HashProvider = SHA1.Create();
        }

        public static bool CheckFiles()
        {
#if ENABLE_IL2CPP
            return true;
#else
            TamperDetectionFileEntry entry = new TamperDetectionFileEntry()
            {      
                rootSearchPath = Application.dataPath,
                filename = "BouncyCastle.dll",
                hash = Convert.FromBase64String("tDGyqbczdzjiX4KT3wsHDIiQuVQ=")
            };

            return CheckFileEntry(entry);
#endif
        }

        private static bool CheckFileEntry(TamperDetectionFileEntry entry)
        {
            string[] files = Directory.GetFiles(entry.rootSearchPath, entry.filename, entry.mustBeExact ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);

            if (files.Length == 0) {
                Debug.LogError(string.Format("File ({0}) not found in folder ({1})", entry.filename, entry.rootSearchPath));
                return false;
            }
            //Debug.Log(string.Format("File ({0}) found in folder ({1})", entry.filename, entry.rootSearchPath));

            byte[] hash = _HashProvider.ComputeHash(File.OpenRead(files[0]));
            bool result = MixCastCryptoUtils.BytesEqual(hash, entry.hash);
            if (!result) {
#if UNITY_EDITOR
                Debug.LogError(string.Format("hash mismatch: {0}, expected {1} got {2}",
                    entry.filename, Convert.ToBase64String(entry.hash), Convert.ToBase64String(hash)));
#else
                Debug.LogError("file change detected: " + files[0]);
#endif
            }
            return result;
        }
    }
}
#endif
