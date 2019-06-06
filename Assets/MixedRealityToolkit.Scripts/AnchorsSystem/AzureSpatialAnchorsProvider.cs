// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if MRTK_USING_AZURESPATIALANCHORS && !UNITY_EDITOR
using Microsoft.Azure.SpatialAnchors;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_IOS
using UnityEngine.XR.iOS;
#elif WINDOWS_UWP
using UnityEngine.XR.WSA;
#endif

namespace Microsoft.MixedReality.Toolkit.Anchors
{
    /// <summary>
    /// Implementation of Azure Spatial Anchors service wrapper
    /// </summary>
    public class AzureSpatialAnchorsProvider : IAzureSpatialAnchorsProvider
    {
        private string accountId;
        private string accountKey;

        private CloudSpatialAnchorSession cloudSession;
        private const int BadSessionId = -2;

#if UNITY_IOS
        private UnityARSessionNativeInterface arkitSession;
#endif

        /// <summary>
        /// Create a provider with session information. <see href="https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-hololens"/>
        /// </summary>
        /// <param name="accountId">Azure Spatial Anchors account ID</param>
        /// <param name="accountKey">Azure Spatial Anchors account key</param>
        public AzureSpatialAnchorsProvider(string accountId, string accountKey)
        {
            this.accountId = accountId;
            this.accountKey = accountKey;
        }

        /// <inheritdoc />
        public bool VerboseLogging { get; set; }

        /// <inheritdoc />
        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                if (active != value)
                {
                    if (cloudSession != null)
                    {
                        if (value)
                        {
                            cloudSession.Start();
                        }
                        else
                        {
                            cloudSession.Stop();
                        }
                    }
                    active = value;
                }
            }
        }
        private bool active = false;

        /// <summary>
        /// The active session with the Azure Spatial Anchors service
        /// </summary>
        public CloudSpatialAnchorSession ActiveSession => cloudSession;

        /// <summary>
        /// Value provided from <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.spatialanchors.sessionstatus"/>SessionStatus</see>
        /// </summary>
        public float ReadyForCreateProgress { get; private set; }

        /// <summary>
        /// Value provided from <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.spatialanchors.sessionstatus"/>SessionStatus</see>
        /// </summary>
        public float RecommendedForCreateProgress { get; private set; }

        /// <summary>
        /// Value provided from <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.spatialanchors.sessionstatus"/>SessionStatus</see>
        /// </summary>
        public int SessionCreateHash { get; private set; }

        /// <summary>
        /// Value provided from <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.spatialanchors.sessionstatus"/>SessionStatus</see>
        /// </summary>
        public int SessionLocateHash { get; private set; }

        /// <summary>
        /// Value provided from <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.spatialanchors.sessionstatus"/>SessionStatus</see>
        /// </summary>
        public SessionUserFeedback UserFeedback { get; private set; }

        /// <inheritdoc />
        public void Initialize()
        {
            if (cloudSession != null)
            {
                Debug.LogError("Attempted to initialize AzureSpatialAnchorsPrivoder twice.");
                return;
            }

            ReadyForCreateProgress = 0;
            RecommendedForCreateProgress = 0;
            SessionCreateHash = 0;
            SessionLocateHash = 0;
            UserFeedback = SessionUserFeedback.None;

            cloudSession = new CloudSpatialAnchorSession();
            cloudSession.LogLevel = SessionLogLevel.All;
            cloudSession.Configuration.AccountId = accountId;
            cloudSession.Configuration.AccountKey = accountKey;

            cloudSession.SessionUpdated += CloudSession_SessionUpdated;
            cloudSession.Error += CloudSession_Error;
            cloudSession.OnLogDebug += CloudSession_OnLogDebug;
            cloudSession.AnchorLocated += CloudSession_AnchorLocated;
            cloudSession.LocateAnchorsCompleted += CloudSession_LocateAnchorsCompleted;

#if UNITY_IOS
            if (arkitSession == null)
            {
                arkitSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
                UnityARSessionNativeInterface.ARFrameUpdatedEvent += UnityARSessionNativeInterface_ARFrameUpdatedEvent;
            }
            cloudSession.Session = arkitSession.GetNativeSessionPointer();
#elif UNITY_ANDROID
            // TODO
            // GoogleARCoreInternal.ARCoreAndroidLifecycleManager.Instance.NativeSession.SessionHandle;
#endif

            if (Active)
            {
                cloudSession.Start();
            }
        }

        /// <inheritdoc />
        public void Destroy()
        {
            if (cloudSession != null)
            {
                cloudSession.Dispose();
                cloudSession = null;
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            cloudSession.Reset();
        }

        /// <inheritdoc />
        public IAzureAnchorData GetNewAnchor()
        {
            return new AzureAnchorData();
        }

        /// <inheritdoc />
        public void UpdatePlatformAnchor(GameObject objectToUpdate, IAzureAnchorData anchorToUpdateTo)
        {
            var anchorData = (AzureAnchorData)anchorToUpdateTo;
            
            if (!anchorData.Synced)
            {
                throw new InvalidOperationException("Cannot update to an anchor that wasn't synced with the service");
            }

            var cloudAnchor = anchorData.Source;
            var ptr = cloudAnchor.LocalAnchor;
#if WINDOWS_UWP
            var worldAnchor = objectToUpdate.GetComponent<WorldAnchor>();
            if (worldAnchor == null)
            {
                worldAnchor = objectToUpdate.AddComponent<WorldAnchor>();
            }
            worldAnchor.SetNativeSpatialAnchorPtr(ptr);
#elif UNITY_IOS
            var anchorData = UnityARSessionNativeInterface.UnityAnchorDataFromArkitAnchorPtr(ptr);
            Matrix4x4 matrix4X4 = GetMatrix4x4FromUnityAr4x4(anchorData.transform);
            var worldAnchor = objectToUpdate.GetComponent<UnityARUserAnchorComponent>();
            if (worldAnchor != null)
            {
                Component.DestroyImmediate(worldAnchor);
                worldAnchor = null;
            }
            loadedAnchorObject.transform.position = UnityARMatrixOps.GetPosition(matrix4X4);
            loadedAnchorObject.transform.rotation = UnityARMatrixOps.GetRotation(matrix4X4);
            objectToUpdate = objectToAnchor.AddComponent<UnityARUserAnchorComponent>();
#elif UNITY_ANDROID
            Debug.LogError("Android not currently supported");
#endif
        }

        /// <inheritdoc />
        public async void CommitAnchorAsync(GameObject anchoredObject)
        {
            AzureAnchorCommitCompletedEventArgs result;

            try
            {
                var persistentAnchor = anchoredObject.GetComponent<PersistentAnchor>();

                if (persistentAnchor == null)
                {
                    persistentAnchor = anchoredObject.AddComponent<PersistentAnchor>();
                }

                if (persistentAnchor.CurrentAnchor == null)
                {
                    persistentAnchor.CreateAnchor();
                }

                try
                {
                    CloudSpatialAnchor workingAnchor = ((AzureAnchorData)persistentAnchor.AzureAnchor).Source;
                    if (workingAnchor == null && !persistentAnchor.AzureAnchor.Synced)
                    {
                        workingAnchor = await cloudSession.GetAnchorPropertiesAsync(persistentAnchor.AzureAnchor.Identifier);
                    }

                    bool newAnchor = (workingAnchor == null);

                    if (newAnchor)
                    {
                        workingAnchor = new CloudSpatialAnchor();
                    }

                    IntPtr anchorRawPointer = IntPtr.Zero;
#if WINDOWS_UWP
            anchorRawPointer = persistentAnchor.CurrentAnchor.GetNativeSpatialAnchorPtr();
#elif UNITY_IOS
            anchorRawPointer = UnityARSessionNativeInterface.GetARSessionNativeInterface().GetArAnchorPointerForId(persistentAnchor.CurrentAnchor..AnchorId);
#elif UNITY_ANDROID
            Debug.LogError("Android currently not supported.");
#endif

                    if (anchorRawPointer == null)
                    {
                        // TODO: Return error
                    }

                    workingAnchor.LocalAnchor = anchorRawPointer;
                    workingAnchor.AppProperties.Clear();
                    foreach (var prop in persistentAnchor.AzureAnchor.Properties)
                    {
                        workingAnchor.AppProperties.Add(prop.Key, prop.Value);
                    }

                    if (newAnchor)
                    {
                        await cloudSession.CreateAnchorAsync(workingAnchor);
                    }
                    else
                    {
                        await cloudSession.UpdateAnchorPropertiesAsync(workingAnchor);
                    }

                    result = new AzureAnchorCommitCompletedEventArgs(anchoredObject, persistentAnchor.AzureAnchor);
                }
                // Exception while committing the CloudSpatialAnchor
                catch (Exception e)
                {
                    result = new AzureAnchorCommitCompletedEventArgs(anchoredObject, persistentAnchor.AzureAnchor, e);
                }
            }
            // Exception before CloudSpatialAnchor was acquired
            catch (Exception e)
            {
                result = new AzureAnchorCommitCompletedEventArgs(anchoredObject, null, e);
            }

            // Call back on the main thread when results come in
            SyncContextUtility.UnitySynchronizationContext.Send(x =>
            {
                if (AnchorCommitCompleted != null)
                {
                    AnchorCommitCompleted.Invoke(result);
                }
            }, null);
        }

        /// <inheritdoc />
        public async void DeleteAnchorAsync(IAzureAnchorData anchor)
        {
            await cloudSession.DeleteAnchorAsync(((AzureAnchorData)anchor).Source);
        }

        /// <inheritdoc />
        public int StartSearchingForAnchors(
            string[] identifiersToSearchFor = null,
            bool bypassCache = false,
            bool useRelationshipInformation = true)
        {
            return StartSearchingForAnchors(new AnchorLocateCriteria()
            {
                Identifiers = identifiersToSearchFor,
                BypassCache = bypassCache,
                Strategy = useRelationshipInformation ? LocateStrategy.AnyStrategy : LocateStrategy.VisualInformation,
                RequestedCategories = AnchorDataCategory.Properties | AnchorDataCategory.Spatial
            });
        }

        /// <inheritdoc />
        public int StartSearchingForAnchorsNear(
            GameObject persistentAnchoredObject,
            float distanceInMeters,
            int maxResultCount = 20,
            bool bypassCache = false,
            bool useRelationshipInformation = true)
        {
            if (persistentAnchoredObject == null)
            {
                Debug.LogError("Argument must be a valid GameObject with an attached PersistentAnchor to search near");
                return BadSessionId;
            }

            var persistentAnchor = persistentAnchoredObject.GetComponent<PersistentAnchor>();
            if (persistentAnchor == null)
            {
                Debug.LogError("GameObject to search near must have an attached PersistentAnchor component");
                return BadSessionId;
            }

            if (persistentAnchor.AzureAnchor == null || !persistentAnchor.AzureAnchor.Synced)
            {
                Debug.LogError("GameObject's PersistentAnchor has not yet been found, cannot search near it");
                return BadSessionId;
            }
            
            return StartSearchingForAnchors(new AnchorLocateCriteria()
            {
                NearAnchor = new NearAnchorCriteria()
                {
                    SourceAnchor = ((AzureAnchorData)persistentAnchor.AzureAnchor).Source,
                    DistanceInMeters = distanceInMeters,
                    MaxResultCount = maxResultCount
                },
                BypassCache = bypassCache,
                Strategy = useRelationshipInformation ? LocateStrategy.AnyStrategy : LocateStrategy.VisualInformation,
                RequestedCategories = AnchorDataCategory.Properties | AnchorDataCategory.Spatial
            });
        }

        /// <inheritdoc />
        private int StartSearchingForAnchors(AnchorLocateCriteria locateCriteria)
        {
            var watcher = cloudSession.CreateWatcher(locateCriteria);
            return watcher.Identifier;
        }

        /// <inheritdoc />
        public bool IsSearchingForAnchors(int sessionId = -1)
        {
            if (sessionId == BadSessionId)
            {
                return false;
            }

            var activeWatchers = cloudSession.GetActiveWatchers();

            return (sessionId == -1 && activeWatchers.Any()) || (sessionId != -1 && activeWatchers.Any(w => w.Identifier == sessionId));
        }

        /// <inheritdoc />
        public void StopSearchingForAnchors(int sessionId = -1)
        {
            if (sessionId == BadSessionId)
            {
                return;
            }

            var activeWatchers = cloudSession.GetActiveWatchers();

            if (sessionId == -1)
            {
                foreach (var watcher in activeWatchers)
                {
                    watcher.Stop();
                }
            }
            else
            {
                var watcher = activeWatchers.FirstOrDefault(w => w.Identifier == sessionId);
                if (watcher != null)
                {
                    watcher.Stop();
                }
            }
        }

        /// <inheritdoc />
        public event Action<AzureAnchorLocatedEventArgs> AnchorLocated;

        /// <inheritdoc />
        public event Action<AzureAnchorLocatedEventArgs> NewAnchorLocated;

        /// <inheritdoc />
        public event Action<AzureAnchorCommitCompletedEventArgs> AnchorCommitCompleted;

        private void CloudSession_SessionUpdated(object sender, SessionUpdatedEventArgs args)
        {
            ReadyForCreateProgress = args.Status.ReadyForCreateProgress;
            RecommendedForCreateProgress = args.Status.RecommendedForCreateProgress;
            SessionCreateHash = args.Status.SessionCreateHash;
            SessionLocateHash = args.Status.SessionLocateHash;
            UserFeedback = args.Status.UserFeedback;
        }

        private void CloudSession_Error(object sender, SessionErrorEventArgs args)
        {
            if (VerboseLogging)
            {
                Debug.LogError(args.ErrorMessage);
            }
        }

        private void CloudSession_OnLogDebug(object sender, OnLogDebugEventArgs args)
        {
            if (VerboseLogging)
            {
                Debug.Log(args.Message);
            }
        }

        private void CloudSession_AnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            // We only respond to anchors actually located
            if (args.Status != LocateAnchorStatus.Located && args.Status != LocateAnchorStatus.AlreadyTracked)
            {
                return;
            }

            // Call back on the main thread
            var eventArgs = new AzureAnchorLocatedEventArgs(new AzureAnchorData(args.Anchor));
            SyncContextUtility.UnitySynchronizationContext.Send(x =>
            {
                if (AnchorLocated != null)
                {
                    AnchorLocated.Invoke(eventArgs);
                }
            }, null);

            // If no existing subscribers claimed the anchor, we fire another event to allow responses such as creating a new object
            if (!eventArgs.Consumed)
            {
                SyncContextUtility.UnitySynchronizationContext.Send(x =>
                {
                    if (NewAnchorLocated != null)
                    {
                        NewAnchorLocated.Invoke(eventArgs);
                    }
                }, null);
            }
        }

        private void CloudSession_LocateAnchorsCompleted(object sender, EventArgs e)
        {
            // Nothing interesting to do here
        }

#if UNITY_IOS
        private void UnityARSessionNativeInterface_ARFrameUpdatedEvent(UnityARCamera camera)
        {
            if (cloudSession != null)
            {
                cloudSession.ProcessFrame(arkitSession.GetLatestFramePointer());
            }
        }

        private Matrix4x4 GetMatrix4x4FromUnityAr4x4(UnityARMatrix4x4 input)
        {
            Matrix4x4 retval = new Matrix4x4(input.column0, input.column1, input.column2, input.column3);
            return retval;
        }
#endif
    }
}
#endif
