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
using UnityEditor;
using System.IO;

namespace BlueprintReality.MixCast {
    [InitializeOnLoad]
	public class SdkImporter {
        static SdkImporter()
        {
            EnsureProjectSettingsExist();
        }

        static void EnsureProjectSettingsExist()
        {
            string[] settingsPaths = Directory.GetFiles(Application.dataPath, "MixCast_ProjectSettings.asset", SearchOption.AllDirectories);
            if (settingsPaths.Length > 0)
                return;

            string[] filePaths = Directory.GetFiles(Application.dataPath, "MixCast.cs", System.IO.SearchOption.AllDirectories);
            if (filePaths.Length != 1)
                return;
            string pluginDirectory = Directory.GetParent(filePaths[0]).Parent.FullName; //MixCast/Scripts/MixCast.cs
            string settingsPath = Path.Combine(pluginDirectory, "Resources/MixCast_ProjectSettings.asset");

            MixCastProjectSettings settingsAsset = ScriptableObject.CreateInstance<MixCastProjectSettings>();
            settingsAsset.name = "MixCast_ProjectSettings";
            int assetFolderIndex = settingsPath.LastIndexOf("\\Assets\\");
            settingsPath = settingsPath.Substring(assetFolderIndex + 1);

            AssetDatabase.CreateAsset(settingsAsset, settingsPath);
            AssetDatabase.SaveAssets();
        }
	}
}
#endif