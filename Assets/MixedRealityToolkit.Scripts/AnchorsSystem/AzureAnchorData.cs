// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

#if MRTK_USING_AZURESPATIALANCHORS
using System;
using Microsoft.Azure.SpatialAnchors;
using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// Data wrapper for CloudSpatialAnchor
    /// </summary>
    public class AzureAnchorData : IAzureAnchorData
    {
        private string identifierOverride;

        private static readonly string AzureNameProperty = "PersistentAnchor_Name";

        /// <summary>
        /// For internal use
        /// </summary>
        /// <param name="source">Source anchor</param>
        public AzureAnchorData(CloudSpatialAnchor source = null)
        {
            if (source != null)
            {
                Source = source;
            }
            else
            {
                Source = new CloudSpatialAnchor();
            }
        }

        /// <summary>
        /// Source anchor
        /// </summary>
        public CloudSpatialAnchor Source { get; private set; }

        /// <inheritdoc />
        public string Identifier
        {
            get
            {
                return string.IsNullOrEmpty(Source.Identifier) ? identifierOverride : Source.Identifier;
            }
            set
            {
                if (!string.IsNullOrEmpty(Source.Identifier))
                {
                    throw new InvalidOperationException("Cannot set the identifier for anchor data that has already been synced");
                }

                identifierOverride = value;
            }
        }

        /// <inheritdoc />
        public string Name
        {
            get
            {
                string name;
                Source.AppProperties.TryGetValue(AzureNameProperty, out name);
                return name;
            }
            set
            {
                Source.AppProperties[AzureNameProperty] = value;
            }
        }

        /// <inheritdoc />
        public IDictionary<string, string> Properties => Source.AppProperties;

        /// <inheritdoc />
        public bool Synced => !string.IsNullOrEmpty(Source.Identifier);
    }
}
#endif
