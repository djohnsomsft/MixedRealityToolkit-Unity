// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors.Editor
{
    [CustomEditor(typeof(AzureSpatialAnchorsProviderProfile))]
    public class AzureSpatialAnchorsProviderProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        private static readonly string AzureSpatialAnchorsDefine = "MRTK_USING_AZURESPATIALANCHORS";

        private static readonly BuildTargetGroup[] GroupsToBuildAzureSpatialAnchorsFor = new BuildTargetGroup[]
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
            BuildTargetGroup.WSA
        };

        private SerializedProperty accountId;
        private SerializedProperty accountKey;
        private SerializedProperty autoStart;
        private SerializedProperty verboseLogging;

        private static readonly string ProfileTitle = "Azure Spatial Anchors Provider Settings";
        private static readonly string ProfileDescription = "The Azure Spatial Anchors Provider profile configures your integration with an Azure Spatial Anchors service.";

        protected override void OnEnable()
        {
            base.OnEnable();

            accountId = serializedObject.FindProperty("accountId");
            accountKey = serializedObject.FindProperty("accountKey");
            autoStart = serializedObject.FindProperty("autoStart");
            verboseLogging = serializedObject.FindProperty("verboseLogging");
        }

        public override void OnInspectorGUI()
        {
            RenderProfileHeader(ProfileTitle, ProfileDescription, target, true, BackProfileType.Anchors);

            using (new GUIEnabledWrapper(!IsProfileLock((BaseMixedRealityProfile)target)))
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                bool changed = false;

                EditorGUILayout.Space();

                // TODO: Better way to check if ASA plugin is present in the project
                if (!AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Any(t => t.Namespace == "Microsoft.Azure.SpatialAnchors"))
                {
                    EditorGUILayout.HelpBox("Your project does not currently have the Azure Spatial Anchors SDK included.", MessageType.Info);
                    if (GUILayout.Button("Click here to get started"))
                    {
                        Application.OpenURL("https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-hololens");
                    }
                    return;
                }

                if (!IsBuildingWithAzureSpatialAnchors())
                {
                    EditorGUILayout.HelpBox($"You must define {AzureSpatialAnchorsDefine} to turn on Azure Spatial Anchors functionality.", MessageType.Info);
                    if (GUILayout.Button("Click here to set it"))
                    {
                        SetBuildWithAzureSpatialAnchors();
                    }
                    return;
                }
                else
                {
                    if (GUILayout.Button("Remove Azure Spatial Anchors from build"))
                    {
                        UnsetBuildWithAzureSpatialAnchors();
                    }
                }

                if (GUILayout.Button("Click for help getting started"))
                {
                    Application.OpenURL("https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-hololens");
                }

                EditorGUILayout.PropertyField(accountId);
                EditorGUILayout.PropertyField(accountKey);
                EditorGUILayout.PropertyField(autoStart);
                EditorGUILayout.PropertyField(verboseLogging);

                if (!changed)
                {
                    changed |= EditorGUI.EndChangeCheck();
                }
                
                serializedObject.ApplyModifiedProperties();

                if (changed && MixedRealityToolkit.IsInitialized)
                {
                    EditorApplication.delayCall += () => MixedRealityToolkit.Instance.ResetConfiguration(MixedRealityToolkit.Instance.ActiveProfile);
                }
            }
        }

        protected override bool IsProfileInActiveInstance()
        {
            var profile = target as BaseMixedRealityProfile;
            return MixedRealityToolkit.IsInitialized && profile != null &&
                   MixedRealityToolkit.Instance.HasActiveProfile &&
                   MixedRealityToolkit.Instance.ActiveProfile.AnchorsSystemProfile != null &&
                   profile == MixedRealityToolkit.Instance.ActiveProfile.AnchorsSystemProfile.cloudAnchorsProviderProfile;
        }

        private bool IsBuildingWithAzureSpatialAnchors()
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.WSA)?.Split(';');
            return symbols != null && symbols.Any(s => s == AzureSpatialAnchorsDefine);
        }

        private void SetBuildWithAzureSpatialAnchors()
        {
            foreach (var group in GroupsToBuildAzureSpatialAnchorsFor)
            {
                var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols + ";" + AzureSpatialAnchorsDefine);
            }
        }

        private void UnsetBuildWithAzureSpatialAnchors()
        {
            foreach (var group in GroupsToBuildAzureSpatialAnchorsFor)
            {
                var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group)?.Split(';');

                // Isn't present, skip
                if (symbols == null)
                {
                    Debug.LogWarning("Didn't find Azure Spatial Anchors compile define for build group " + group);
                    continue;
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    group,
                    string.Join(";", symbols.Where(s => s != AzureSpatialAnchorsDefine)));
            }
        }
    }
}
