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
        private string localIdentity = null;

        [SerializeField]
        [Tooltip("Automatically load from the local anchor store")]
        private bool localAutoLoad = false;

        [SerializeField]
        [Tooltip("ID for anchor in Azure Spatial Anchors store")]
        private string azureIdentity = null;

        [SerializeField]
        [Tooltip("Name for anchor in Azure Spatial Anchors store")]
        private string azureName = null;

        [SerializeField]
        [Tooltip("Automatically update the local anchor")]
        private bool azureAutoUpdateLocal = false;
#pragma warning restore 0649

        // TODO: Make a more generic query
        private static readonly string AzureNameProperty = "PersistentAnchor_Name";

        /// <summary>
        /// Name of the anchor to save to the local store
        /// </summary>
        public string LocalIdentity { get => localIdentity; set => localIdentity = value; }

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
                if (CanMatchAzureAnchor() && MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null)
                {
                    MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated -= AzureSpatialAnchorsProvider_AnchorLocated;
                }

                azureAnchorName = value;
                AzureAnchorProperties[AzureNameProperty] = value;

                if (CanMatchAzureAnchor() && MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null)
                {
                    MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated += AzureSpatialAnchorsProvider_AnchorLocated;
                }
            }
        }

        private string azureAnchorName;

        /// <summary>
        /// Anchor ID used by the Azure Spatial Anchors service for this anchor
        /// </summary>
        public string AzureAnchorId
        {
            get
            {
                return azureAnchorId;
            }
            set
            {
                if (CanMatchAzureAnchor() && MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null)
                {
                    MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated -= AzureSpatialAnchorsProvider_AnchorLocated;
                }

                azureAnchorId = value;

                if (CanMatchAzureAnchor() && MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null)
                {
                    MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated += AzureSpatialAnchorsProvider_AnchorLocated;
                }
            }
        }

        private string azureAnchorId;

        /// <summary>
        /// Properties associated with the Azure Spatial Anchors service anchor
        /// </summary>
        public Dictionary<string, string> AzureAnchorProperties { get; set; }

        /// <summary>
        /// The source CloudSpatialAnchor for the anchor, if it has been found in Azure Spatial Anchors
        /// </summary>
        public object AzureAnchorSource { get; private set; }

        /// <summary>
        /// Update the local anchor automatically when the Azure Spatial Anchor is updated
        /// </summary>
        public bool AzureAutoUpdateLocal { get => azureAutoUpdateLocal; set => azureAutoUpdateLocal = value; }

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

        /// <summary>
        /// Clears the cached Azure anchor data
        /// </summary>
        public void ClearCachedAzureAnchor()
        {
            AzureAnchorSource = null;
        }

        private void Awake()
        {
            CurrentAnchor = GetComponent<AnchorComponentType>();
            AzureAnchorId = azureIdentity;
            AzureAnchorProperties = new Dictionary<string, string>();
            AzureAnchorName = azureName;
            AzureAnchorSource = null;

            if (localAutoLoad)
            {
                LoadLocalAnchor();
            }
        }

        private bool CanMatchAzureAnchor()
        {
            return !string.IsNullOrEmpty(AzureAnchorId) || !string.IsNullOrEmpty(AzureAnchorName);
        }

        private void AzureSpatialAnchorsProvider_AnchorLocated(AzureAnchorLocatedEventArgs args)
        {
            // Match by the cached anchor ID or by the user-defined ID
            string persistentId;
            if (args.Identifier == AzureAnchorId ||
                (args.Properties.TryGetValue(AzureNameProperty, out persistentId) && AzureAnchorName == persistentId))
            {
                if (CurrentAnchor == null)
                {
                    // Set directly so that AnchorUpdated event isn't fired prematurely
                    currentAnchor = gameObject.AddComponent<AnchorComponentType>();
                }

                args.SyncToWorldAnchor(gameObject);
                AzureAnchorId = args.Identifier;
                AzureAnchorProperties = new Dictionary<string, string>(args.Properties);
                string nameValue;
                if (AzureAnchorProperties.TryGetValue(AzureNameProperty, out nameValue))
                {
                    AzureAnchorName = nameValue;
                }
                AzureAnchorSource = args.Source;
                CurrentAnchor = GetComponent<AnchorComponentType>();

                if (AzureAutoUpdateLocal)
                {
                    if (string.IsNullOrEmpty(localIdentity))
                    {
                        // Automatically create a name for the local cached anchor if not specified
                        localIdentity = string.IsNullOrEmpty(AzureAnchorName) ?
                            AzureAnchorId :
                            AzureAnchorName;
                    }

                    SaveLocalAnchor();
                }
            }
        }
    }
}
