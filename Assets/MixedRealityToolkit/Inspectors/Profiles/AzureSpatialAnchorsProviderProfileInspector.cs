// Copyright (c) Microsoft Corporation. All rights reserved.
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
    }
}
