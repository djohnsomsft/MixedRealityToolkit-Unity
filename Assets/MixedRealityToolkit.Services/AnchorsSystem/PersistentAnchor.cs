// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_IOS
using UnityEngine.XR.iOS;
#elif WINDOWS_UWP
using UnityEngine.XR.WSA;
#endif

#if UNITY_IOS
using AnchorComponentType = UnityEngine.XR.iOS.UnityARUserAnchorComponent;
#elif WINDOWS_UWP
using AnchorComponentType = UnityEngine.XR.WSA.WorldAnchor;
#else // UNITY_ANDROID || UNITRY_EDITOR
// TODO: Android not supported
using AnchorComponentType = UnityEngine.Transform;
#endif

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// Attach to a GameObject to persist an anchor locally and/or in the cloud
    /// </summary>
    public class PersistentAnchor : MonoBehaviour
    {
        // Disable warnings for fields not set as these are set in the editor
#pragma warning disable 0649
        [SerializeField]
        [Tooltip("ID in local anchor store for this anchor")]
        private string localIdentity;

        [SerializeField]
        [Tooltip("Automatically load from the local anchor store")]
        private bool localAutoLoad;

        [SerializeField]
        [Tooltip("ID for anchor in Azure Spatial Anchors store")]
        private string azureIdentity;

        [SerializeField]
        [Tooltip("Automatically update the local anchor")]
        private bool azureAutoUpdateLocal;
#pragma warning restore 0649

        // TODO: Make a more generic query
        private static readonly string IdPropertyName = "PersistentAnchor_Id";

        /// <summary>
        /// Anchor name assigned in Azure Spatial Anchors for this anchor
        /// </summary>
        public string AzureAnchorName
        {
            get
            {
                return azureAnchorName;
            }
            set
            {
                if (azureAnchorName != null)
                {
                    MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated -= AzureSpatialAnchorsProvider_AnchorLocated;
                    AzureAnchorId = null;
                    AzureAnchorProperties = new Dictionary<string, string>();
                    AzureAnchorSource = null;
                }

                azureAnchorName = value;
#if MRTK_USING_AZURESPATIALANCHORS
                if (!string.IsNullOrEmpty(azureAnchorName))
                {
                    MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated += AzureSpatialAnchorsProvider_AnchorLocated;
                }
#endif
            }
        }

        private string azureAnchorName;

        public string AzureAnchorId { get; private set; }
        public Dictionary<string, string> AzureAnchorProperties { get; private set; }
        public object AzureAnchorSource { get; private set; }

        /// <summary>
        /// Called when the anchor is created or updated
        /// </summary>
        public UnityEvent AnchorUpdated = new UnityEvent();

        /// <summary>
        /// The current anchor component attached to the GameObject, if any
        /// </summary>
        public AnchorComponentType CurrentAnchor
        {
            get
            {
                return currentAnchor;
            }
            private set
            {
#if !UNITY_ANDROID
                if (currentAnchor != value)
                {
                    currentAnchor = value;
                    AnchorUpdated.Invoke();
                }
#endif
            }
        }

        private AnchorComponentType currentAnchor = null;

        /// <summary>
        /// Creates a new anchor on the GameObject
        /// </summary>
        public void CreateAnchor()
        {
            if (CurrentAnchor != null)
            {
                Debug.Log("Object already has an anchor, ignoring CreateAnchor call");
                return;
            }

            CurrentAnchor = gameObject.AddComponent<AnchorComponentType>();
        }

        /// <summary>
        /// Loads the anchor from the local anchor store
        /// </summary>
        public void LoadLocalAnchor()
        {
#if WINDOWS_UWP
            MixedRealityToolkit.AnchorsSystem.RegisterForLocalAnchorsReadyCallback(() =>
                CurrentAnchor = MixedRealityToolkit.AnchorsSystem.LocalAnchors.Load(localIdentity, gameObject));
#elif !UNITY_EDITOR
            Debug.LogWarning("Local anchors currently only supported on UWP.");
#endif
        }

        /// <summary>
        /// Saves the anchor to the local anchor store
        /// </summary>
        public void SaveLocalAnchor()
        {
#if WINDOWS_UWP
            if (CurrentAnchor == null)
            {
                Debug.LogError("Cannot save a local anchor if a WorldAnchor hasn't been set");
                return;
            }

            MixedRealityToolkit.AnchorsSystem.RegisterForLocalAnchorsReadyCallback(() =>
                MixedRealityToolkit.AnchorsSystem.LocalAnchors.Save(localIdentity, CurrentAnchor));
#elif !UNITY_EDITOR
            Debug.LogWarning("Local anchors currently only supported on UWP.");
#endif
        }

        private void Awake()
        {
            CurrentAnchor = GetComponent<AnchorComponentType>();
            AzureAnchorName = azureIdentity;
            AzureAnchorProperties = new Dictionary<string, string>();
            AzureAnchorSource = null;

            if (localAutoLoad)
            {
                LoadLocalAnchor();
            }
        }

        private void AzureSpatialAnchorsProvider_AnchorLocated(AzureAnchorLocatedEventArgs args)
        {
            // Match by the cached anchor ID or by the user-defined ID
            string persistentId;
            if (args.Identifier == AzureAnchorId ||
                (args.Properties.TryGetValue(IdPropertyName, out persistentId) && azureIdentity == persistentId))
            {
                if (CurrentAnchor == null)
                {
                    // Set directly so that AnchorUpdated event isn't fired prematurely
                    currentAnchor = gameObject.AddComponent<AnchorComponentType>();
                }

                args.SyncToWorldAnchor(gameObject);
                AzureAnchorId = args.Identifier;
                AzureAnchorProperties = new Dictionary<string, string>(args.Properties);
                AzureAnchorSource = args.Source;
                CurrentAnchor = GetComponent<AnchorComponentType>();

                if (!string.IsNullOrEmpty(localIdentity) && azureAutoUpdateLocal)
                {
                    SaveLocalAnchor();
                }
            }
        }
    }
}
