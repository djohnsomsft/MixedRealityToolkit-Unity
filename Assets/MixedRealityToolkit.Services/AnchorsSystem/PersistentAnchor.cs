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
#else // UNITY_ANDROID || UNITY_EDITOR
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
        // Disable warnings for fields not set/used as these are set in the editor
#pragma warning disable CS0414, CS0649
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
#pragma warning restore CS0414, CS0649

        private void Awake()
        {
            CurrentAnchor = GetComponent<AnchorComponentType>();
#if MRTK_USING_AZURESPATIALANCHORS && !UNITY_EDITOR
            if (MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null)
            {
                AzureAnchor = MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.GetNewAnchor();
            }
#endif

            if (localAutoLoad)
            {
                LoadLocalAnchor();
            }
        }

        #region Platform Anchor

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
                // If the old anchor was deleted we may need to reacquire it
                if (currentAnchor == null)
                {
                    currentAnchor = GetComponent<AnchorComponentType>();
                }

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
        /// Removes the platform anchor, allowing the object to be moved.
        /// </summary>
        public void RemoveAnchor()
        {
            // TODO: Android currently not supported
#if WINDOWS_UWP || UNITY_IOS
            if (CurrentAnchor != null)
            {
                Component.DestroyImmediate(CurrentAnchor);
                CurrentAnchor = null;
            }
#endif
        }

#endregion Platform Anchor

#region Local Anchor

        /// <summary>
        /// Name of the anchor to save to the local store
        /// </summary>
        public string LocalIdentity { get => localIdentity; set => localIdentity = value; }

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

        public void DeleteLocalAnchor()
        {
#if WINDOWS_UWP
            MixedRealityToolkit.AnchorsSystem.RegisterForLocalAnchorsReadyCallback(() => MixedRealityToolkit.AnchorsSystem.LocalAnchors.Delete(localIdentity));
#elif !UNITY_EDITOR
            Debug.LogWarning("Local anchors currently only supported on UWP.");
#endif
        }

#endregion Local Anchor

#region Azure Anchor

        /// <summary>
        /// Anchor data for Azure Spatial Anchors anchor
        /// </summary>
        public IAzureAnchorData AzureAnchor
        {
            get => azureAnchor;
            set
            {
#if MRTK_USING_AZURESPATIALANCHORS
                if (azureAnchor != value)
                {
                    if (azureAnchor != null && MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null)
                    {
                        MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated -= AzureSpatialAnchorsProvider_AnchorLocated;
                    }

                    azureAnchor = value;

                    if (azureAnchor != null && MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null)
                    {
                        MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated += AzureSpatialAnchorsProvider_AnchorLocated;
                        if (azureAnchor.Synced)
                        {
                            MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.UpdatePlatformAnchor(gameObject, azureAnchor);
                            AnchorUpdated.Invoke();
                        }
                    }
                }
#endif
            }
        }
        private IAzureAnchorData azureAnchor = null;

        /// <summary>
        /// Update the local anchor automatically when the Azure Spatial Anchor is updated
        /// </summary>
        public bool AzureAutoUpdateLocal { get => azureAutoUpdateLocal; set => azureAutoUpdateLocal = value; }

        /// <summary>
        /// Saves the anchor to the Azure Spatial Anchors service
        /// </summary>
        public void SaveAzureAnchorAsync()
        {
#if MRTK_USING_AZURESPATIALANCHORS
            if (MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null)
            {
                MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.CommitAnchorAsync(gameObject);
            }
#endif
        }

        public void DeleteAzureAnchorAsync()
        {
#if MRTK_USING_AZURESPATIALANCHORS
            if (MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null)
            {
                if (string.IsNullOrEmpty(AzureAnchor.Identifier))
                {
                    Debug.LogError("Cannot delete an anchor that has not been synced with the service");
                    return;
                }

                // Copy the properties over but generate a new CloudSpatialAnchor to remove the ID
                var newAnchor = MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.GetNewAnchor();
                foreach (var prop in AzureAnchor.Properties)
                {
                    newAnchor.Properties.Add(prop.Key, prop.Value);
                }

                MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.DeleteAnchorAsync(AzureAnchor);
                AzureAnchor = newAnchor;
            }
#endif
        }

#if MRTK_USING_AZURESPATIALANCHORS
        private void AzureSpatialAnchorsProvider_AnchorLocated(AzureAnchorLocatedEventArgs args)
        {
            // Skip if it has already been consumed
            if (args.Consumed)
            {
                return;
            }

            // Match by the cached anchor ID or by the user-defined Name
            if (args.Anchor.Identifier == AzureAnchor.Identifier || args.Anchor.Name == AzureAnchor.Name)
            {
                if (CurrentAnchor == null)
                {
                    // Set directly so that AnchorUpdated event isn't fired prematurely
                    currentAnchor = gameObject.AddComponent<AnchorComponentType>();
                }

                AzureAnchor = args.Anchor;
                MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.UpdatePlatformAnchor(gameObject, args.Anchor);

                if (AzureAutoUpdateLocal)
                {
                    if (string.IsNullOrEmpty(localIdentity))
                    {
                        // Automatically create a name for the local cached anchor if not specified
                        localIdentity = string.IsNullOrEmpty(AzureAnchor.Name) ?
                            AzureAnchor.Identifier :
                            AzureAnchor.Name;
                    }

                    SaveLocalAnchor();
                }
            }
        }
#endif

#endregion Azure Anchor
    }
}
