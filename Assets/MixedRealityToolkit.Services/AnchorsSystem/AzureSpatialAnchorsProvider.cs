#if MRTK_USING_AZURESPATIALANCHORS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{

    internal class AzureSpatialAnchorsProvider
    {
        private string accountId;
        private string accountKey;

        public AzureSpatialAnchorsProvider(string accountId, string accountKey)
        {
            this.accountId = accountId;
            this.accountKey = accountKey;
        }
    }
}
#endif
