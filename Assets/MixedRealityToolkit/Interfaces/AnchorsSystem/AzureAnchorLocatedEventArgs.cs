// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// Event data when an anchor is found by the Azure Spatial Anchors service
    /// </summary>
    public class AzureAnchorLocatedEventArgs : EventArgs
    {
        /// <summary>
        /// The anchor that was found
        /// </summary>
        public IAzureAnchorData Anchor { get; private set; }

        /// <summary>
        /// True if the anchor has already been consumed
        /// </summary>
        public bool Consumed { get; private set; }

        /// <summary>
        /// Consumes the anchor so that it is marked as having been matched
        /// </summary>
        public void Consume()
        {
            Consumed = true;
        }

        /// <summary>
        /// For internal use
        /// </summary>
        /// <param name="anchor">Anchor found</param>
        /// <param name="callbackOnMatched">Callback</param>
        public AzureAnchorLocatedEventArgs(IAzureAnchorData anchor)
        {
            Anchor = anchor;
        }
    }
}
