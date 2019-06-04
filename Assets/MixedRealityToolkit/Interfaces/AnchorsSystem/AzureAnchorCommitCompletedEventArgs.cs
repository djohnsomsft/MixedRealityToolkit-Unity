// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// Event data when an anchor commit operation is completed
    /// </summary>
    public class AzureAnchorCommitCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// For internal use
        /// </summary>
        ///  <param name="target">Target GameObject</param>
        /// <param name="anchor">Anchor</param>
        /// <param name="exception">Exception, if any</param>
        public AzureAnchorCommitCompletedEventArgs(GameObject target, IAzureAnchorData anchor, Exception exception = null)
        {
            Target = target;
            Anchor = anchor;
            Exception = exception;
        }

        /// <summary>
        /// GameObject that the anchor was attached to
        /// </summary>
        public GameObject Target { get; private set; }

        /// <summary>
        /// Anchor that was committed
        /// </summary>
        public IAzureAnchorData Anchor { get; private set; }

        /// <summary>
        /// Exception thrown during the operation, if any
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// True if operation was successful
        /// </summary>
        public bool Succeeded => Exception == null;
    }
}
