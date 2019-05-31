using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// Configuration profile settings for setting up the anchors system.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Mixed Reality Anchors System Profile", fileName = "MixedRealityAnchorsSystemProfile", order = (int)CreateProfileMenuItemIndices.Anchors)]
    [MixedRealityServiceProfile(typeof(IMixedRealityAnchorsSystem))]
    public class MixedRealityAnchorsSystemProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        [Tooltip("Profile of the cloud anchors service to use.")]
        public BaseMixedRealityProfile cloudAnchorsProviderProfile = null;
    }
}
