// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Anchors;
using Microsoft.MixedReality.Toolkit.UI;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.Anchors
{
    public class DemoAnchor : MonoBehaviour
    {
        public GameObject controls;
        public Renderer cubeRenderer;
        public Material plainMaterial;
        public Material pendingMaterial;
        public Material completeMaterial;

        private PersistentAnchor anchor;
        private ManipulationHandler manipulationHandler;

        public enum State
        {
            Uncommitted,
            Pending,
            Synced
        }

        public State CurrentState
        {
            get => currentState;
            set
            {
                currentState = value;
                switch (currentState)
                {
                    case State.Uncommitted:
                        cubeRenderer.material = plainMaterial;
                        break;
                    case State.Pending:
                        cubeRenderer.material = pendingMaterial;
                        break;
                    case State.Synced:
                        cubeRenderer.material = completeMaterial;
                        break;
                    default:
                        break;
                }
            }
        }
        private State currentState = State.Uncommitted;

        public void OnAnchorUpdated()
        {
            // TODO
        }

        public bool ControlsVisible { get => controls.activeSelf; set => controls.SetActive(value); }


        private void Awake()
        {
            anchor = GetComponent<PersistentAnchor>();
            manipulationHandler = GetComponent<ManipulationHandler>();
        }
    }
}
