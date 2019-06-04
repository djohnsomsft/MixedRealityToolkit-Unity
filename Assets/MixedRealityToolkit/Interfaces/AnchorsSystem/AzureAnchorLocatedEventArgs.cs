// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// Event data when an anchor is found by the Azure Spatial Anchors service
    /// </summary>
    public class AzureAnchorLocatedEventArgs : EventArgs
    {
        private Action<GameObject> callbackOnMatched;

        /// <summary>
        /// Identifier for the found anchor
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Properties of the found anchor
        /// </summary>
        public IDictionary<string, string> Properties { get; private set; }

        /// <summary>
        /// Source CloudSpatialAnchor for the anchor
        /// </summary>
        public object Source { get; private set; }

        /// <summary>
        /// For internal use
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="properties">Properties</param>
        /// <param name="source">Source</param>
        /// <param name="callbackOnMatched">Callback</param>
        public AzureAnchorLocatedEventArgs(
            string identifier,
            IDictionary<string, string> properties,
            object source,
            Action<GameObject> callbackOnMatched)
        {
            Identifier = identifier;
            Properties = properties;
            Source = source;
            this.callbackOnMatched = callbackOnMatched;
        }

        /// <summary>
        /// Call to request the WorldAnchor be synced to the anchor found
        /// </summary>
        /// <param name="objectToAnchor">Object to sync anchor to</param>
        public void SyncToWorldAnchor(GameObject objectToAnchor)
        {
            callbackOnMatched(objectToAnchor);
        }
    }
}
