using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    public interface IAzureSpatialAnchorsProvider
    {
        bool VerboseLogging { get; set; }

        void Initialize();
        void Destroy();
        void Reset();
    }
}
