#if MRTK_USING_AZURESPATIALANCHORS
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.Azure.SpatialAnchors;

namespace Microsoft.MixedReality.Toolkit.Anchors
{

    public class AzureSpatialAnchorsProvider : IAzureSpatialAnchorsProvider
    {
        private string accountId;
        private string accountKey;

        private CloudSpatialAnchorSession cloudSession;

        public bool VerboseLogging { get; set; }

        public float ReadyForCreateProgress { get; private set; }
        public float RecommendedForCreateProgress { get; private set; }
        public int SessionCreateHash { get; private set; }
        public int SessionLocateHash { get; private set; }


#if UNITY_IOS
        private UnityARSessionNativeInterface arkitSession;
#endif

        public AzureSpatialAnchorsProvider(string accountId, string accountKey)
        {
            this.accountId = accountId;
            this.accountKey = accountKey;
        }

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

            this.cloudSession = new CloudSpatialAnchorSession();
            this.cloudSession.LogLevel = SessionLogLevel.All;
            this.cloudSession.Configuration.AccountId = this.accountId;
            this.cloudSession.Configuration.AccountKey = this.accountKey;

            this.cloudSession.SessionUpdated += CloudSession_SessionUpdated;
            this.cloudSession.Error += CloudSession_Error;
            this.cloudSession.OnLogDebug += CloudSession_OnLogDebug;
            this.cloudSession.AnchorLocated += CloudSession_AnchorLocated;
            this.cloudSession.LocateAnchorsCompleted += CloudSession_LocateAnchorsCompleted;

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
        }

        public void Destroy()
        {
            if (cloudSession != null)
            {
                cloudSession.Dispose();
                cloudSession = null;
            }
        }

        public void Reset()
        {
            Destroy();
            Initialize();
        }

        private void CloudSession_SessionUpdated(object sender, SessionUpdatedEventArgs args)
        {
            ReadyForCreateProgress = args.Status.ReadyForCreateProgress;
            RecommendedForCreateProgress = args.Status.RecommendedForCreateProgress;
            SessionCreateHash = args.Status.SessionCreateHash;
            SessionLocateHash = args.Status.SessionLocateHash;
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
            // TODO
        }

        private void CloudSession_LocateAnchorsCompleted(object sender, EventArgs e)
        {
            // TODO
        }

#if UNITY_IOS
        private void UnityARSessionNativeInterface_ARFrameUpdatedEvent(UnityARCamera camera)
        {
            if (cloudSession != null)
            {
                cloudSession.ProcessFrame(arkitSession.GetLatestFramePointer());
            }
        }
#endif
    }
}
#endif
