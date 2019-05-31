// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors.Editor
{
    [CustomEditor(typeof(MixedRealityAnchorsSystemProfile))]
    public class MixedRealityAnchorsServiceInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        private static readonly GUIContent CreateAzureProvider = new GUIContent("+ Add Azure Spatial Anchors Provider", "Add Azure Spatial Anchors Provider");

        private static bool showCloudAnchorsProviderProfile = true;
        private SerializedProperty cloudAnchorsProviderProfile;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            cloudAnchorsProviderProfile = serializedObject.FindProperty("cloudAnchorsProviderProfile");
        }

        public override void OnInspectorGUI()
        {
            RenderTitleDescriptionAndLogo(
                "Anchors System Profile",
                "The Anchors System Profile configures how world anchors are managed by the application.");

            if (MixedRealityInspectorUtility.CheckMixedRealityConfigured(true, !RenderAsSubProfile))
            {
                if (DrawBacktrackProfileButton("Back to Configuration Profile", MixedRealityToolkit.Instance.ActiveProfile))
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
            showCloudAnchorsProviderProfile = EditorGUILayout.Foldout(showCloudAnchorsProviderProfile, "Cloud Anchors Providers", true);
            if (showCloudAnchorsProviderProfile)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    if (cloudAnchorsProviderProfile.objectReferenceValue != null)
                    {
                        changed |= RenderProfile(cloudAnchorsProviderProfile);
                    }
                    else
                    {
                        if (GUILayout.Button(CreateAzureProvider, EditorStyles.miniButton))
                        {
                            ScriptableObject instance = CreateInstance<AzureSpatialAnchorsProviderProfile>();
                            var newProfile = instance.CreateAsset(AssetDatabase.GetAssetPath(Selection.activeObject)) as BaseMixedRealityProfile;
                            cloudAnchorsProviderProfile.objectReferenceValue = newProfile;
                            cloudAnchorsProviderProfile.serializedObject.ApplyModifiedProperties();
                            changed = true;
                        }
                    }
                }
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
