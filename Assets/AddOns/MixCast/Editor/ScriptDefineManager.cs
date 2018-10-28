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

//Class that handles enforcing the script defines on project necessary for appropriate SDK interaction
namespace BlueprintReality.MixCast
{
    [InitializeOnLoad]
    public class ScriptDefineManager
    {
        public class FileDrivenDefine
        {
            public string defineFlag = "";
            public string fileName = "";
            public string systemName = "";
        }

        public static readonly FileDrivenDefine[] FILE_DEFINES = new FileDrivenDefine[]
        {
            new FileDrivenDefine()
            {
                defineFlag = "MIXCAST_STEAMVR",
                fileName = "SteamVR.cs",
                systemName = "SteamVR"
            },
            new FileDrivenDefine()
            {
                defineFlag = "MIXCAST_OCULUS",
                fileName = "OVRManager.cs",
                systemName = "Oculus"
            },
        };

        static ScriptDefineManager()
        {
            EditorApplication.delayCall += UpdateFlagsAfterLoad;
        }
        static void UpdateFlagsAfterLoad()
        {
            if (MixCast.ProjectSettings != null && MixCast.ProjectSettings.applySdkFlagsAutomatically)      //This isn't working yet, values not loaded yet
                EnforceAppropriateScriptDefines();
            EditorApplication.delayCall -= UpdateFlagsAfterLoad;
        }

        public static bool EnforceAppropriateScriptDefines()
        {
            string defineStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            List<string> defineList = new List<string>();
            if( !string.IsNullOrEmpty(defineStr) )
                defineList.AddRange(defineStr.Split(';'));

            bool anyChanges = false;
            for( int i = 0; i < FILE_DEFINES.Length; i++ )
            {
                FileDrivenDefine define = FILE_DEFINES[i];
                bool changed = EnforceDefineAutomatically(define.defineFlag, define.fileName, defineList);
                if (changed && defineList.Contains(define.defineFlag))
                    Debug.Log("Enabled MixCast " + define.systemName + " support");
                anyChanges |= changed;
            }

            if (anyChanges)
            {
                defineStr = string.Join(";", defineList.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), defineStr);
            }

            return anyChanges;
        }

        //Returns true if the define list has been modified
        public static bool EnforceDefineAutomatically(string libraryFlag, string libraryIdentifier, List<string> currentDefines)
        {
            bool libraryFound = Directory.GetFiles(Application.dataPath, libraryIdentifier, SearchOption.AllDirectories).Length > 0;
            bool modifying = currentDefines.Contains(libraryFlag) != libraryFound;
            if (modifying)
            {
                if (libraryFound)
                    currentDefines.Add(libraryFlag);
                else
                    currentDefines.Remove(libraryFlag);
            }
            return modifying;
        }
        public static bool IsDefineEnabled(string flag)
        {
            string defineStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            List<string> defineList = new List<string>(defineStr.Split(';'));
            return defineList.Contains(flag);
        }
        public static bool TryEnableDefine(FileDrivenDefine define)
        {
            if (IsDefineEnabled(define.defineFlag))
                return true;
            bool libraryFound = Directory.GetFiles(Application.dataPath, define.fileName, SearchOption.AllDirectories).Length > 0;
            if (!libraryFound)
                return false;
            BuildTargetGroup buildTarget = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string defineStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            defineStr += ";" + define.defineFlag;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defineStr);
            return true;
        }
        public static void DisableDefine(FileDrivenDefine define)
        {
            if (!IsDefineEnabled(define.defineFlag))
                return;
            BuildTargetGroup buildTarget = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string defineStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            List<string> defineList = new List<string>(defineStr.Split(';'));
            defineList.Remove(define.defineFlag);
            defineStr = string.Join(";", defineList.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defineStr);
        }
    }
}
#endif