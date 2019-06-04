// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.WSA.Persistence;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    public interface IMixedRealityAnchorsSystem : IMixedRealityEventSystem, IMixedRealityEventSource
    {
        /// <summary>
        /// Local anchors store provided through Unity
        /// </summary>
        WorldAnchorStore LocalAnchors { get; }

        /// <summary>
        /// Register for a callback when the WorldAnchorStore is loaded
        /// </summary>
        /// <param name="callback">Method to call</param>
        void RegisterForLocalAnchorsReadyCallback(Action callback);

        /// <summary>
        /// Azure Spatial Anchors service provider
        /// </summary>
        IAzureSpatialAnchorsProvider AzureSpatialAnchors { get; }
    }
}
