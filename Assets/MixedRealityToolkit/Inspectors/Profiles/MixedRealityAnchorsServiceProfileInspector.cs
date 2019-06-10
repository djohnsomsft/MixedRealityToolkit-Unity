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

        private SerializedProperty enableLocalAnchorStore;
        private static bool showCloudAnchorsProviderProfile = true;
        private SerializedProperty cloudAnchorsProviderProfile;

        private const string ProfileTitle = "Anchors System Settings";
        private const string ProfileDescription = "The Anchors System profile controls how world anchors are managed by the application.";

        protected override void OnEnable()
        {
            base.OnEnable();

            enableLocalAnchorStore = serializedObject.FindProperty("enableLocalAnchorStore");
            cloudAnchorsProviderProfile = serializedObject.FindProperty("cloudAnchorsProviderProfile");
        }

        public override void OnInspectorGUI()
        {
            RenderProfileHeader(ProfileTitle, ProfileDescription, target);

            bool changed = false;
            using (new GUIEnabledWrapper(!IsProfileLock((BaseMixedRealityProfile)target)))
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(enableLocalAnchorStore);

                EditorGUILayout.Space();

                showCloudAnchorsProviderProfile = EditorGUILayout.Foldout(showCloudAnchorsProviderProfile, "Cloud Anchors Providers", true);
                if (showCloudAnchorsProviderProfile)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        if (cloudAnchorsProviderProfile.objectReferenceValue != null)
                        {
                            changed |= RenderProfile(cloudAnchorsProviderProfile, typeof(AzureSpatialAnchorsProviderProfile), false);
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
                   profile == MixedRealityToolkit.Instance.ActiveProfile.AnchorsSystemProfile;
        }
    }
}
