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
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlueprintReality.MixCast
{
    public class MixCastRegistry
    {
        public const string SETTINGS_REGISTRY_KEY = "DATA";
        public const string SECURE_REGISTRY_KEY = "SETUP";
        public const string DECRYPT_REGISTRY_KEY = "HASH";
        public const string SERVICE_DATA_KEY = "SERVICE";

        private static MixCastDecrypter _Decrypt;

        public static bool shouldShowRegistryMismatchWarning = false;

        public static void InitDecrypt()
        {
            byte[] privateKey = MixCastEncryptionKey.ReadKeyFromRegistry(DECRYPT_REGISTRY_KEY);
            _Decrypt = privateKey != null ? new MixCastDecrypter(privateKey) : null;
        }

        public static MixCastData ReadData()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(MixCastPath.REGISTRY_PATH);
            Assert.IsNotNull(reg);
            string dataStr = reg.GetValue(SETTINGS_REGISTRY_KEY, null) as string;

#if UNITY_EDITOR
            string myDocsPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string settingsFilePath = myDocsPath + "\\MixCastSettings.json";

            if (File.Exists(settingsFilePath))
            {
                dataStr = File.ReadAllText(settingsFilePath);
            }
#endif

            MixCastData data = string.IsNullOrEmpty(dataStr) ? new MixCastData() : JsonUtility.FromJson<MixCastData>(dataStr);

            if (!string.IsNullOrEmpty(data.sourceVersion) && data.sourceVersion != MixCast.VERSION_STRING)
            {
                shouldShowRegistryMismatchWarning = true;
                Debug.LogWarning(string.Format("settings version mis-match detected: {0} instead of {1}", data.sourceVersion, MixCast.VERSION_STRING));
                MixCastDataUtility.UpdateForBackwardCompatibility(data, dataStr);
            }

            return data;
        }

        public static MixCastData.SecureData ReadSecureData()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(MixCastPath.REGISTRY_PATH);
            Assert.IsNotNull(reg);
            MixCastData.SecureData data = ReadSecureSettingsFromRegistry(reg);

            if (data == null)
            {
                data = new MixCastData.SecureData();
            }
            else
            {
                MachineId thisComputerId = new MachineId();
                if (!IsSameComputer(data, thisComputerId))
                {
                    Debug.LogError("machine identifier mismatch detected");
                    if (data != null)
                    {
                        data.licenseType = MixCastData.LicenseType.Free;
                    }
                }
            }

            return data;
        }

        public static void WriteData(MixCastData data)
        {
            string dataStr = JsonUtility.ToJson(data);
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(MixCastPath.REGISTRY_PATH);
            reg.SetValue(MixCastRegistry.SETTINGS_REGISTRY_KEY, dataStr);
        }

        private static MixCastData.SecureData ReadSecureSettingsFromRegistry(RegistryKey reg)
        {
            InitDecrypt();

            if (_Decrypt == null) { return null; }

            string encryptedDataStr = reg.GetValue(SECURE_REGISTRY_KEY, null) as string;
            if (string.IsNullOrEmpty(encryptedDataStr))
            {
                Debug.LogWarning("no secure settings data found");
                return null;
            }

            byte[] encryptedData = Convert.FromBase64String(encryptedDataStr);
            byte[] decryptedData = _Decrypt.Decrypt(encryptedData);
            var dataStr = Encoding.UTF8.GetString(decryptedData);

            return string.IsNullOrEmpty(dataStr) ? null : JsonUtility.FromJson<MixCastData.SecureData>(dataStr);
        }

        private static bool IsSameComputer(MixCastData.SecureData data, MachineId machineId)
        {
            if (machineId == null || data == null || string.IsNullOrEmpty(data.machineId))
            {
                return false;
            }

            return MixCastCryptoUtils.BytesEqual(
                    machineId.ComputeHash(),
                    Convert.FromBase64String(data.machineId));
        }

        public static void ShowRegistryMismatchWarningIfNeeded()
        {
            if (shouldShowRegistryMismatchWarning)
            {
                Debug.Log("Registry data is out-of-date");
                //GameObjects.ToastCenter.ShowToast("Field_Warning", "Field_OldVersionData", null, 5.0f);
            }
            shouldShowRegistryMismatchWarning = false;
        }

        public static bool IsServiceRunning()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(MixCastPath.REGISTRY_PATH);
            Assert.IsNotNull(reg);
            string dataStr = reg.GetValue(SERVICE_DATA_KEY, null) as string;
            reg.Close();
            return !string.IsNullOrEmpty(dataStr);
        }

#if UNITY_EDITOR

        [MenuItem("MixCast/Clear User Data")]
        static void ClearUserData()
        {
            var registryKey = Registry.CurrentUser.CreateSubKey(MixCastPath.REGISTRY_PATH);

            try
            {
                registryKey.DeleteValue(SETTINGS_REGISTRY_KEY);
                Debug.Log("Deleted user data from the registry.");
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("No user data in registry to delete.");
            }
        }

#endif
    }
}
#endif
