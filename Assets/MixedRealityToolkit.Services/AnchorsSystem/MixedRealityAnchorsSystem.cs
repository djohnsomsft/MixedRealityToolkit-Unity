// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// The Anchoring system controls interactions between local anchors and anchoring services such as Azure Spatial Anchors.
    /// </summary>
    public class MixedRealityAnchorsSystem : BaseCoreSystem, IMixedRealityAnchorsSystem
    {
#if MRTK_USING_AZURESPATIALANCHORS
        AzureSpatialAnchorsProvider azureSpatialAnchorsProvider = null;
#endif


        public MixedRealityAnchorsSystem(
            IMixedRealityServiceRegistrar registrar,
            MixedRealityAnchorsSystemProfile profile) : base(registrar, profile)
        {
            if (profile != null)
            {
                if (profile.cloudAnchorsProviderProfile != null)
                {
                    if (profile.cloudAnchorsProviderProfile is AzureSpatialAnchorsProviderProfile)
                    {
#if MRTK_USING_AZURESPATIALANCHORS
                        var azureSpatialAnchorsProviderProfile = (AzureSpatialAnchorsProviderProfile)profile.cloudAnchorsProviderProfile;
                        azureSpatialAnchorsProvider = new AzureSpatialAnchorsProvider(
                            azureSpatialAnchorsProviderProfile.accountId,
                            azureSpatialAnchorsProviderProfile.accountKey);
#elif !UNITY_EDITOR
                        Debug.LogWarning("AzureSpatialAnchorsProvider was not loaded because it is not enabled in this build. If this wasn't intentional, enable it from the profile.");
#endif
                    }
                }
            }
        }

#region IMixedRealityService Implementation

        /// <inheritdoc/>
        public override void Initialize()
        {
            if (!Application.isPlaying) { return; }

#if MRTK_USING_AZURESPATIALANCHORS
            // TODO: Initialize AzureSpatialAnchorsProvider
#endif
        }

        public override void Destroy()
        {
#if MRTK_USING_AZURESPATIALANCHORS
            // TODO: Cleanup AzureSpatialAnchorsProvider
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
