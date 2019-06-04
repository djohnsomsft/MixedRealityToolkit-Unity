// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    public class AzureSpatialAnchorsProviderProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        [Tooltip("Account ID of Azure Spatial Anchors service to use.")]
        public string accountId = null;

        [SerializeField]
        [Tooltip("Account key of Azure Spatial Anchors service to use.")]
        public string accountKey = null;

        [SerializeField]
        [Tooltip("True to immediately start environmental scanning")]
        public bool autoStart = true;

        [SerializeField]
        [Tooltip("Enable verbose logging of message from the Azure Spatial Anchors service")]
        public bool verboseLogging = false;
    }
}
