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
        public MixedRealityAnchorsSystem(
            IMixedRealityServiceRegistrar registrar,
            MixedRealityAnchorsSystemProfile profile = null) : base(registrar, profile)
        {
            // TODO
        }

        #region IMixedRealityService Implementation

        /// <inheritdoc/>
        public override void Initialize()
        {
            if (!Application.isPlaying) { return; }

            // TODO
        }

        public override void Destroy()
        {
            // TODO
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
