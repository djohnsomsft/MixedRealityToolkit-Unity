// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Anchors;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.Anchors
{
    public class DemoAnchor : MonoBehaviour
    {
        public GameObject prefab;
        public GameObject controls;
        public Renderer cubeRenderer;
        public Material plainMaterial;
        public Material pendingMaterial;
        public Material completeMaterial;

        public GameObject localAnchorUXOff;
        public GameObject localAnchorUXOn;
        public TextMeshPro localAnchorStatus;
        public GameObject azureAnchorUXOff;
        public GameObject azureAnchorUXOn;
        public Renderer azureAutoUpdateButtonBack;
        public TextMeshPro azureAnchorStatus;
        public GameObject azureDeleteButton;
        public Renderer allowMoveButtonBack;
        public GameObject searchLocalButton;
        public GameObject searchAzureButton;
        public TextMeshPro searchAzureButtonText;
        public GameObject textPopup;
        public TextMeshPro textPopupText;

        [HideInInspector]
        public PersistentAnchor anchor;
        private ManipulationHandler manipulationHandler;
        private Interactable interactable;

        private bool allowMove = false;
        private string localName;
        private string azureName;

        private bool localAnchorsAvailable;
        private bool azureAnchorsAvailable;

        public enum LocalAnchorState
        {
            None,
            Unsynced,
            Moved,
            Failed,
            Loading,
            Loaded,
            Saved
        }

        public LocalAnchorState LocalStatus
        {
            get => localState;
            set
            {
                localState = value;
                UpdateVisuals();
            }
        }
        private LocalAnchorState localState = LocalAnchorState.None;

        public enum AzureAnchorState
        {
            None,
            Unsynced,
            Moved,
            Failed,
            Committing,
            Synced
        }

        public AzureAnchorState AzureStatus
        {
            get => azureState;
            set
            {
                azureState = value;
                UpdateVisuals();
            }
        }
        private AzureAnchorState azureState = AzureAnchorState.None;

        public void CreateNewLocalAnchorIdentity()
        {
            anchor.LocalIdentity = "Demo" + Random.Range(0, 1000000).ToString();
            LocalStatus = LocalAnchorState.Unsynced;
        }

        public void LoadLocalAnchor()
        {
            LocalStatus = LocalAnchorState.Loading;
            anchor.LoadLocalAnchor();
        }

        public void SaveLocalAnchor()
        {
            anchor.SaveLocalAnchor();
            LocalStatus = LocalAnchorState.Saved;
        }

        public void DeleteLocalAnchor()
        {
            anchor.DeleteLocalAnchor();
            LocalStatus = LocalAnchorState.Unsynced;
        }

        public void ToggleAutoUpdateLocalFromAzure()
        {
            anchor.AzureAutoUpdateLocal = !anchor.AzureAutoUpdateLocal;
            UpdateVisuals();
        }

        public void SaveAzureAnchor()
        {
            AzureStatus = AzureAnchorState.Committing;
            anchor.SaveAzureAnchorAsync();
        }

        public void DeleteAzureAnchor()
        {
            anchor.DeleteAzureAnchorAsync();
            AzureStatus = AzureAnchorState.Unsynced;
        }

        public void CreateNewAzureAnchorIdentity()
        {
            anchor.AzureAnchor.Name = "Demo" + Random.Range(0, 1000000).ToString();
            AzureStatus = AzureAnchorState.Unsynced;
        }

        public void ToggleAllowMove()
        {
            allowMove = !allowMove;
            UpdateVisuals();
        }

        public void OnAnchorUpdateFailed()
        {
            if (LocalStatus == LocalAnchorState.Loading)
            {
                LocalStatus = LocalAnchorState.Failed;
            }
        }

        public void OnAnchorUpdated()
        {
            if (LocalStatus == LocalAnchorState.Loading)
            {
                LocalStatus = LocalAnchorState.Loaded;
            }
        }

        public void OnClick()
        {
            controls.SetActive(true);
        }

        public void OnManipulationStarted()
        {
            anchor.RemoveAnchor();

            if (LocalStatus != LocalAnchorState.None && LocalStatus != LocalAnchorState.Unsynced)
            {
                LocalStatus = LocalAnchorState.Moved;
            }

            if (AzureStatus != AzureAnchorState.None && AzureStatus != AzureAnchorState.Unsynced)
            {
                AzureStatus = AzureAnchorState.Moved;
            }

            controls.SetActive(false);
        }

        public void OnManipulationEnded()
        {
            anchor.CreateAnchor();
            controls.SetActive(true);
        }

        public void SearchForAllLocalAnchors()
        {
            MixedRealityToolkit.AnchorsSystem.RegisterForLocalAnchorsReadyCallback(() =>
            {
                var localAnchors = MixedRealityToolkit.AnchorsSystem.LocalAnchors.GetAllIds();
                var demoAnchors = FindAllDemoAnchors();
                foreach (var localAnchorId in localAnchors)
                {
                    var matchingDemoAnchor = demoAnchors.FirstOrDefault(da => da.anchor.LocalIdentity == localAnchorId);
                    if (matchingDemoAnchor != null)
                    {
                        matchingDemoAnchor.LoadLocalAnchor();
                    }
                    else
                    {
                        var newAnchorObject = GameObject.Instantiate(prefab);
                        var newDemoAnchor = newAnchorObject.GetComponent<DemoAnchor>();
                        newDemoAnchor.anchor.LocalIdentity = localAnchorId;
                        newDemoAnchor.LoadLocalAnchor();
                    }
                }
            });
        }

        public void ToggleSearchForAllAzureAnchors()
        {
            if (MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.IsSearchingForAnchors())
            {

            }
            else
            {
                MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.StopSearchingForAnchors();
            }
        }

        public void StopSearchingForAzureAnchors()
        {
            MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.StopSearchingForAnchors();
        }

        public void OnLocalNameFocus()
        {
            if (!string.IsNullOrEmpty(localName))
            {
                textPopup.SetActive(true);
                textPopupText.text = localName;
            }
        }

        public void OnAzureNameFocus()
        {
            if (!string.IsNullOrEmpty(azureName))
            {
                textPopup.SetActive(true);
                textPopupText.text = azureName;
            }
        }

        public void HidePopup()
        {
            textPopup.SetActive(false);
        }

        public bool ControlsVisible { get => controls.activeSelf; set => controls.SetActive(value); }

        private void Start()
        {
            anchor = GetComponent<PersistentAnchor>();
            manipulationHandler = GetComponent<ManipulationHandler>();
            interactable = GetComponent<Interactable>();
            localAnchorsAvailable = MixedRealityToolkit.AnchorsSystem != null && MixedRealityToolkit.AnchorsSystem.LocalAnchorsEnabled;
            azureAnchorsAvailable = MixedRealityToolkit.AnchorsSystem != null && MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors != null;
            if (azureAnchorsAvailable)
            {
                MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated += AzureSpatialAnchors_AnchorLocated;
                MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.NewAnchorLocated += AzureSpatialAnchors_NewAnchorLocated;
                MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorCommitCompleted += AzureSpatialAnchors_AnchorCommitCompleted;
            }
            searchLocalButton.SetActive(localAnchorsAvailable);
            searchAzureButton.SetActive(azureAnchorsAvailable);
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (azureAnchorsAvailable)
            {
                MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorLocated -= AzureSpatialAnchors_AnchorLocated;
                MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.AnchorCommitCompleted -= AzureSpatialAnchors_AnchorCommitCompleted;
            }
        }

        private void AzureSpatialAnchors_AnchorLocated(AzureAnchorLocatedEventArgs args)
        {
            if (args.Anchor.Name == anchor.AzureAnchor.Name || args.Anchor.Identifier == anchor.AzureAnchor.Identifier)
            {
                AzureStatus = AzureAnchorState.Synced;
            }
        }

        private void AzureSpatialAnchors_NewAnchorLocated(AzureAnchorLocatedEventArgs args)
        {
            if (args.Consumed)
            {
                return;
            }

            args.Consume();

            var newAnchorObject = GameObject.Instantiate(prefab);
            var newDemoAnchor = newAnchorObject.GetComponent<DemoAnchor>();
            newDemoAnchor.anchor.AzureAnchor = args.Anchor;
            newDemoAnchor.AzureStatus = AzureAnchorState.Synced;
        }

        private void AzureSpatialAnchors_AnchorCommitCompleted(AzureAnchorCommitCompletedEventArgs args)
        {
            if (args.Target == gameObject)
            {
                AzureStatus = args.Succeeded ? AzureAnchorState.Synced : AzureAnchorState.Failed;
            }
        }

        private DemoAnchor[] FindAllDemoAnchors()
        {
            return FindObjectsOfType<DemoAnchor>();
        }

        private void UpdateVisuals()
        {
            // If we're synced in Azure or synced locally without Azure, show green
            var localSynced = (LocalStatus == LocalAnchorState.Loaded || LocalStatus == LocalAnchorState.Saved);
            if (AzureStatus == AzureAnchorState.Synced ||
                (AzureStatus == AzureAnchorState.None && localSynced))
            {
                cubeRenderer.material = completeMaterial;
            }
            // If we're in the middle of an operation, show yellow
            else if (AzureStatus == AzureAnchorState.Committing || LocalStatus == LocalAnchorState.Loading)
            {
                cubeRenderer.material = pendingMaterial;
            }
            // If we're dirty or uncommitted, show gray
            else
            {
                cubeRenderer.material = plainMaterial;
            }

            var hasLocalIdentity = !string.IsNullOrEmpty(anchor.LocalIdentity) && localAnchorsAvailable;
            localAnchorUXOff.SetActive(!hasLocalIdentity && localAnchorsAvailable);
            localAnchorUXOn.SetActive(hasLocalIdentity);
            localName = localAnchorsAvailable ? anchor.LocalIdentity : string.Empty;
            if (hasLocalIdentity)
            {
                localAnchorStatus.text = localState.ToString();
            }

            var hasAzureIdentity = (anchor.AzureAnchor != null) &&
                (anchor.AzureAnchor.Synced || !string.IsNullOrEmpty(anchor.AzureAnchor.Identifier) || !string.IsNullOrEmpty(anchor.AzureAnchor.Name));
            azureAnchorUXOff.SetActive(!hasAzureIdentity && azureAnchorsAvailable);
            azureAnchorUXOn.SetActive(hasAzureIdentity);
            azureName = hasAzureIdentity ?
                ((string.IsNullOrEmpty(anchor.AzureAnchor.Name) ? anchor.AzureAnchor.Identifier : anchor.AzureAnchor.Name)) :
                string.Empty;
            if (hasAzureIdentity)
            {
                azureAnchorStatus.text = azureState.ToString();
            }
            azureAutoUpdateButtonBack.material = anchor.AzureAutoUpdateLocal ? completeMaterial : plainMaterial;
            azureDeleteButton.SetActive(hasAzureIdentity && !string.IsNullOrEmpty(anchor.AzureAnchor.Identifier));

            allowMoveButtonBack.material = allowMove ? completeMaterial : plainMaterial;
            manipulationHandler.enabled = allowMove;
            interactable.enabled = !allowMove;

            if (azureAnchorsAvailable)
            {
                searchAzureButtonText.text = MixedRealityToolkit.AnchorsSystem.AzureSpatialAnchors.IsSearchingForAnchors() ?
                    "Stop Search" :
                    "Search Azure";
            }
        }
    }
}
