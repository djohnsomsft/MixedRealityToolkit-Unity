﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
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

        protected override void OnEnable()
        {
            base.OnEnable();

            accountId = serializedObject.FindProperty("accountId");
            accountKey = serializedObject.FindProperty("accountKey");
        }

        public override void OnInspectorGUI()
        {
            RenderTitleDescriptionAndLogo(
                    "Azure Spatial Anchors Provider Profile",
                    "The Azure Spatial Anchors Provider Profile configures your integration with an Azure Spatial Anchors service.");

            if (MixedRealityInspectorUtility.CheckMixedRealityConfigured(true, !RenderAsSubProfile))
            {
                if (DrawBacktrackProfileButton("Back to Anchors Service Profile", MixedRealityToolkit.Instance.ActiveProfile.AnchorsSystemProfile))
                {
                    return;
                }
            }

            CheckProfileLock(target);

            var previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 160f;

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

            EditorGUILayout.PropertyField(accountId);
            EditorGUILayout.PropertyField(accountKey);


            if (!changed)
            {
                changed |= EditorGUI.EndChangeCheck();
            }

            EditorGUIUtility.labelWidth = previousLabelWidth;
            serializedObject.ApplyModifiedProperties();

            if (MixedRealityToolkit.IsInitialized)
            {
                if (changed)
                {
                    EditorApplication.delayCall += () => MixedRealityToolkit.Instance.ResetConfiguration(MixedRealityToolkit.Instance.ActiveProfile);
                }
            }
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
