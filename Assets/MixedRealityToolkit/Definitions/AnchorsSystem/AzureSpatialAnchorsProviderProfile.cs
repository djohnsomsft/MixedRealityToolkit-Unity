using System.Collections;
using System.Collections.Generic;
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
        [Tooltip("Enable verbose logging of message from the Azure Spatial Anchors service")]
        public bool verboseLogging = false;
    }
}
