// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// Provides access to Azure Spatial Anchors service
    /// </summary>
    public interface IAzureSpatialAnchorsProvider
    {
        /// <summary>
        /// Enable logging of service messages
        /// </summary>
        bool VerboseLogging { get; set; }

        /// <summary>
        /// Initialize the provider
        /// </summary>
        void Initialize();

        /// <summary>
        /// Clean up the provider
        /// </summary>
        void Destroy();

        /// <summary>
        /// Reset the state to initial state for the client
        /// </summary>
        void Reset();

        /// <summary>
        /// Starts searching for anchors, optionally using limiting criteria
        /// </summary>
        /// <param name="identifiersToSearchFor">Specific identifiers to filter to, otherwise searches for any anchor</param>
        /// <param name="bypassCache">Bypass the local cache</param>
        /// <param name="useRelationshipInformation">Use known relational information with existing located anchors</param>
        /// <returns>A session ID for this search which can be used to stop it individually</returns>
        int StartSearchingForAnchors(
            string[] identifiersToSearchFor = null,
            bool bypassCache = false,
            bool useRelationshipInformation = true);

        /// <summary>
        /// Starts searching for anchors near a known anchor
        /// </summary>
        /// <param name="persistentAnchoredObject">Object that has already been located</param>
        /// <param name="distanceInMeters">Distance from the object to search</param>
        /// <param name="maxResultCount">Maximum number of results</param>
        /// <param name="bypassCache">Bypass the local cache</param>
        /// <param name="useRelationshipInformation">Use known relational information with existing located anchors</param>
        /// <returns>A session ID for this search which can be used to stop it individually</returns>
        int StartSearchingForAnchorsNear(
            GameObject persistentAnchoredObject,
            float distanceInMeters,
            int maxResultCount = 20,
            bool bypassCache = false,
            bool useRelationshipInformation = true);

        /// <summary>
        /// Stops searching for anchors
        /// </summary>
        /// <param name="sessionId">Session to stop, or -1 to stop all sessions</param>
        void StopSearchingForAnchors(int sessionId = -1);

        /// <summary>
        /// Event fired when an anchor is located by the service
        /// </summary>
        event Action<AzureAnchorLocatedEventArgs> AnchorLocated;
    }
}
