// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    [CustomEditor(typeof(PersistentAnchor))]
    public class PersistentAnchorInspector : UnityEditor.Editor
    {
        public enum IdentityTypes
        {
            None = 0,
            Local = 1 << 0,
            Azure = 1 << 1
        }

        private PersistentAnchor instance;

        private IdentityTypes identitiesEnabled;

        private bool showLocalIdentity = true;
        private SerializedProperty localIdentity;
        private SerializedProperty localAutoLoad;

        private bool showAzureIdentity = true;
        private SerializedProperty azureIdentity;
        private SerializedProperty azureName;
        private SerializedProperty azureAutoUpdateLocal;

        private static readonly GUIContent IdLabel = new GUIContent("ID");
        private static readonly GUIContent AutoLoadLabel = new GUIContent("Auto Load");
        private static readonly GUIContent NameLabel = new GUIContent("Name");
        private static readonly GUIContent AutoUpdateLocalLabel = new GUIContent("Auto Update Local");

        protected virtual void OnEnable()
        {
            instance = (PersistentAnchor)target;
            identitiesEnabled = IdentityTypes.None;

            localIdentity = serializedObject.FindProperty("localIdentity");
            if (!string.IsNullOrEmpty(localIdentity.stringValue))
            {
                identitiesEnabled |= IdentityTypes.Local;
            }

            localAutoLoad = serializedObject.FindProperty("localAutoLoad");

            azureIdentity = serializedObject.FindProperty("azureIdentity");
            azureName = serializedObject.FindProperty("azureName");
            if (!string.IsNullOrEmpty(azureIdentity.stringValue) || !string.IsNullOrEmpty(azureName.stringValue))
            {
                identitiesEnabled |= IdentityTypes.Azure;
            }

            azureAutoUpdateLocal = serializedObject.FindProperty("azureAutoUpdateLocal");
        }

        public sealed override void OnInspectorGUI()
        {
            identitiesEnabled = (IdentityTypes)EditorGUILayout.EnumFlagsField("Identities", identitiesEnabled);

            if ((identitiesEnabled & IdentityTypes.Local) != IdentityTypes.None)
            {
                showLocalIdentity = EditorGUILayout.Foldout(showLocalIdentity, "Local Identity", true);
                if (showLocalIdentity)
                {
                    EditorGUILayout.PropertyField(localIdentity, IdLabel);
                    EditorGUILayout.PropertyField(localAutoLoad, AutoLoadLabel);
                }
            }
            if ((identitiesEnabled & IdentityTypes.Azure) != IdentityTypes.None)
            {
                showAzureIdentity = EditorGUILayout.Foldout(showAzureIdentity, "Azure Identity", true);
                if (showAzureIdentity)
                {
                    EditorGUILayout.PropertyField(azureIdentity, IdLabel);
                    EditorGUILayout.PropertyField(azureName, NameLabel);
                    EditorGUILayout.PropertyField(azureAutoUpdateLocal, AutoUpdateLocalLabel);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
