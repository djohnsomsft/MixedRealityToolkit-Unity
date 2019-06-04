// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.WSA.Persistence;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// The Anchoring system controls interactions between local anchors and anchoring services such as Azure Spatial Anchors.
    /// </summary>
    public class MixedRealityAnchorsSystem : BaseCoreSystem, IMixedRealityAnchorsSystem
    {
        private bool localAnchorStoreEnabled;
        private List<Action> localAnchorsReadyCallbacks = new List<Action>();
        private WorldAnchorStore worldAnchorStore = null;

#if MRTK_USING_AZURESPATIALANCHORS
        private IAzureSpatialAnchorsProvider azureSpatialAnchorsProvider = null;
#endif

        public MixedRealityAnchorsSystem(
            IMixedRealityServiceRegistrar registrar,
            MixedRealityAnchorsSystemProfile profile) : base(registrar, profile)
        {
            if (profile != null)
            {
                localAnchorStoreEnabled =
#if WINDOWS_UWP
                    profile.enableLocalAnchorStore;
#else
                    // Don't enable the local anchor store in the editor or unsupported platforms
                    false;
#endif
                if (localAnchorStoreEnabled)
                {
                    WorldAnchorStore.GetAsync(value =>
                    {
                        worldAnchorStore = value;
                        foreach (var callback in localAnchorsReadyCallbacks)
                        {
                            callback.Invoke();
                        }
                        localAnchorsReadyCallbacks.Clear();
                    });
                }

                if (profile.cloudAnchorsProviderProfile != null)
                {
                    if (profile.cloudAnchorsProviderProfile is AzureSpatialAnchorsProviderProfile)
                    {
#if MRTK_USING_AZURESPATIALANCHORS
                        var azureSpatialAnchorsProviderProfile = (AzureSpatialAnchorsProviderProfile)profile.cloudAnchorsProviderProfile;

                        // Because the Azure Spatial Anchors plugin is implmented in user scripts,
                        // we have to infer it at runtime to avoid a circular dependency
                        var azureSpatialAnchorsProviderType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(
                            t => t.Namespace == "Microsoft.MixedReality.Toolkit.Anchors" && t.Name == "AzureSpatialAnchorsProvider");

                        if (azureSpatialAnchorsProviderType != null)
                        {
                            azureSpatialAnchorsProvider = (IAzureSpatialAnchorsProvider)Activator.CreateInstance(
                                azureSpatialAnchorsProviderType,
                                azureSpatialAnchorsProviderProfile.accountId,
                                azureSpatialAnchorsProviderProfile.accountKey);
                            azureSpatialAnchorsProvider.VerboseLogging = azureSpatialAnchorsProviderProfile.verboseLogging;
                        }
                        else
                        {
                            Debug.Log("Unknown error locating AzureSpatialAnchorsProvider");
                        }
#elif !UNITY_EDITOR
                        Debug.LogWarning("AzureSpatialAnchorsProvider was not loaded because it is not enabled in this build. If this wasn't intentional, enable it from the profile.");
#endif
                    }
                }
            }
        }

        /// <inheritdoc />
        public WorldAnchorStore LocalAnchors { get; private set; }

        /// <inheritdoc />
        public void RegisterForLocalAnchorsReadyCallback(Action callback)
        {
            if (!localAnchorStoreEnabled)
            {
                return;
            }

            if (worldAnchorStore != null)
            {
                callback();
            }
            else
            {
                localAnchorsReadyCallbacks.Add(callback);
            }
        }

        /// <inheritdoc />
        public IAzureSpatialAnchorsProvider AzureSpatialAnchors
        {
            get
            {
#if MRTK_USING_AZURESPATIALANCHORS
                return azureSpatialAnchorsProvider;
#else
                return null;
#endif
            }
        }

#region IMixedRealityService Implementation

        /// <inheritdoc/>
        public override void Initialize()
        {
            if (!Application.isPlaying) { return; }

#if MRTK_USING_AZURESPATIALANCHORS
            if (azureSpatialAnchorsProvider != null)
            {
                azureSpatialAnchorsProvider.Initialize();
            }
#endif
        }

        public override void Destroy()
        {
#if MRTK_USING_AZURESPATIALANCHORS
            if (azureSpatialAnchorsProvider != null)
            {
                azureSpatialAnchorsProvider.Destroy();
            }
#endif
        }

#endregion IMixedRealityService Implementation

#region IMixedRealityEventSource Implementation

        /// <inheritdoc />
        public uint SourceId { get; } = 0;

        /// <inheritdoc />
        public string SourceName { get; } = "Mixed Reality Anchors System";

        /// <inheritdoc />
        bool IEqualityComparer.Equals(object x, object y)
        {
            // There shouldn't be other Anchors Systems to compare to.
            return false;
        }

        /// <inheritdoc />
        public int GetHashCode(object obj)
        {
            return Mathf.Abs(SourceName.GetHashCode());
        }

#endregion IMixedRealityEventSource Implementation
        
    }
}
