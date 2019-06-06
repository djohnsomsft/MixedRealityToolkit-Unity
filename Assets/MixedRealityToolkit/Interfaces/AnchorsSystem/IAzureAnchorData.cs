// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// Data acquired from Azure Spatial Anchors service about an anchor
    /// </summary>
    public interface IAzureAnchorData
    {
        /// <summary>
        /// Identifier for the anchor
        /// </summary>
        string Identifier { get;}

        /// <summary>
        /// Friendly name of the anchor
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Properties of the anchor
        /// </summary>
        IDictionary<string, string> Properties { get; }

        /// <summary>
        /// True if the anchor has been synced with the Azure Spatial Anchors service
        /// </summary>
        bool Synced { get; }
    }
}