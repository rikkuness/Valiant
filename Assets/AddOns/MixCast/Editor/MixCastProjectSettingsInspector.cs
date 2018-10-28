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
using BlueprintReality.Tools;

namespace BlueprintReality.MixCast {
    [CustomEditor(typeof(MixCastProjectSettings))]
	public class MixCastProjectSettingsInspector : Editor {
        public const string SUPPORT_URL = "https://mixcast.me/route.php?dest=support";

        private readonly static string[] INTEGRATION_URLS = new string[]
        {
            "https://mixcast.me/route.php?dest=steamvrsdk",     //MIXCAST_STEAMVR
            "https://mixcast.me/route.php?dest=oculussdk",     //MIXCAST_OCULUS
        };

        GUIStyle groupBoxStyle;
        GUIStyle headerStyle;
        GUIStyle subHeaderStyle;

        public override void OnInspectorGUI()
        {
            if( groupBoxStyle == null )
            {
                groupBoxStyle = new GUIStyle(EditorStyles.helpBox);
                groupBoxStyle.padding = new RectOffset(10, 10, 10, 10);
                groupBoxStyle.margin = new RectOffset(10, 10, 20, 20);

                headerStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
                headerStyle.fontSize = 26;
                headerStyle.margin = new RectOffset(0, 0, 10, 10);

                subHeaderStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
                subHeaderStyle.fontSize = 18;
                subHeaderStyle.margin = new RectOffset(0, 0, 8, 32);
            }

#if UNITY_5_6_OR_NEWER
            serializedObject.UpdateIfRequiredOrScript();
#else
            serializedObject.UpdateIfDirtyOrScript();
#endif

            EditorGUILayout.LabelField("MixCast Project Settings", headerStyle, GUILayout.Height(36));

            EditorGUIUtility.labelWidth *= 1.5f;

            DrawPluginGroup();
            DrawQualityGroup();
            DrawTransparencyGroup();
            DrawEffectsGroup();
            DrawEditorGroup();

            EditorGUIUtility.labelWidth /= 1.5f;

            serializedObject.ApplyModifiedProperties();
        }
        void DrawTransparencyGroup()
        {
            SerializedProperty usingPmaProp = serializedObject.FindProperty("usingPMA");
            SerializedProperty grabUnfilteredAlphaProp = serializedObject.FindProperty("grabUnfilteredAlpha");

            EditorGUILayout.BeginVertical(groupBoxStyle);
            EditorGUILayout.LabelField("Transparency", subHeaderStyle, GUILayout.Height(28));
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(usingPmaProp);
            if (usingPmaProp.boolValue)
            {
                EditorGUILayout.BeginVertical(groupBoxStyle);

                EditorGUILayout.LabelField("Ensure your project's shaders are compatible with PMA");
                if (GUILayout.Button("Open Wizard"))
                {
                    ShaderTransparencyWizard.ShowWindow();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.PropertyField(grabUnfilteredAlphaProp);

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        void DrawEffectsGroup()
        {
            SerializedProperty specifyLightsManuallyProp = serializedObject.FindProperty("specifyLightsManually");
            SerializedProperty directionalLightPowerProp = serializedObject.FindProperty("directionalLightPower");
            SerializedProperty pointLightPowerProp = serializedObject.FindProperty("pointLightPower");

            EditorGUILayout.BeginVertical(groupBoxStyle);
            EditorGUILayout.LabelField("Effects", subHeaderStyle, GUILayout.Height(28));
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Subject Relighting", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(specifyLightsManuallyProp);

            EditorGUILayout.PropertyField(directionalLightPowerProp);
            if (directionalLightPowerProp.floatValue < 0)
                directionalLightPowerProp.floatValue = 0;

            EditorGUILayout.PropertyField(pointLightPowerProp);
            if (pointLightPowerProp.floatValue < 0)
                pointLightPowerProp.floatValue = 0;

            EditorGUI.indentLevel--;

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        void DrawEditorGroup()
        {
            SerializedProperty displaySubjectInSceneProp = serializedObject.FindProperty("displaySubjectInScene");
            SerializedProperty applyFlagsProp = serializedObject.FindProperty("applySdkFlagsAutomatically");

            EditorGUILayout.BeginVertical(groupBoxStyle);
            EditorGUILayout.LabelField("Editor/Build", subHeaderStyle, GUILayout.Height(28));
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(displaySubjectInSceneProp, new GUIContent("Visualize Subject in Scene View"));

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(applyFlagsProp);
            if (EditorGUI.EndChangeCheck() && applyFlagsProp.boolValue)
                ScriptDefineManager.EnforceAppropriateScriptDefines();

            EditorGUI.indentLevel++;
            if( applyFlagsProp.boolValue )
                EditorGUI.BeginDisabledGroup(true);

            for( int i = 0; i < ScriptDefineManager.FILE_DEFINES.Length; i++ )
            {
                ScriptDefineManager.FileDrivenDefine define = ScriptDefineManager.FILE_DEFINES[i];
                bool wasEnabled = ScriptDefineManager.IsDefineEnabled(define.defineFlag);
                string labelStr = string.Format("Enable {0} support", define.systemName);
                bool isEnabled = EditorGUILayout.Toggle(labelStr, wasEnabled);
                if (wasEnabled != isEnabled)
                {
                    if (isEnabled)
                    {
                        isEnabled = ScriptDefineManager.TryEnableDefine(define);
                        if (!isEnabled)
                        {
                            bool ok = EditorUtility.DisplayDialog(
                                "Missing Dependency",
                                "You haven't imported the required plugin for " + define.systemName + " support",
                                "Get It Now",
                                "Cancel");
                            if (ok)
                                Application.OpenURL(INTEGRATION_URLS[i]);
                        }
                    }
                    else
                    {
                        ScriptDefineManager.DisableDefine(define);
                    }
                }
            }
            if (applyFlagsProp.boolValue)
                EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        void DrawPluginGroup()
        {
            EditorGUILayout.BeginVertical(groupBoxStyle);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(28));
            EditorGUILayout.LabelField("MixCast Plugin (" + MixCast.VERSION_STRING + ")", subHeaderStyle, GUILayout.Height(28));
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            //EditorGUILayout.LabelField("Version:", MixCast.VERSION_STRING, subHeaderStyle, GUILayout.Height(20));
            if (GUILayout.Button("Go To Website"))
                MixCast.GoToWebsite();
            if (GUILayout.Button("Get Support"))
                Application.OpenURL(SUPPORT_URL);
            if (GUILayout.Button("Check for Updates"))
                UpdateChecker.RunCheck();

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        void DrawQualityGroup()
        {
            SerializedProperty overrideAAProp = serializedObject.FindProperty("overrideQualitySettingsAA");
            SerializedProperty aaValProp = serializedObject.FindProperty("overrideAntialiasingVal");

            EditorGUILayout.BeginVertical(groupBoxStyle);
            EditorGUILayout.LabelField("Quality", subHeaderStyle, GUILayout.Height(28));
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(overrideAAProp);
            if( overrideAAProp.boolValue )
            {
                aaValProp.intValue = EditorGUILayout.IntPopup("Anti Aliasing", aaValProp.intValue,
                    new string[] { "Disabled", "2x Multi Sampling", "4x Multi Sampling", "8x Multi Sampling" },
                    new int[] { 0, 1, 2, 3 });
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        [MenuItem("MixCast/Open Project Settings")]
        public static void OpenProjectSettings()
        {
            string[] assetGuids = AssetDatabase.FindAssets("t:MixCastProjectSettings");
            if( assetGuids.Length > 0 )
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<MixCastProjectSettings>(assetPath);
            }
        }
    }
}
#endif