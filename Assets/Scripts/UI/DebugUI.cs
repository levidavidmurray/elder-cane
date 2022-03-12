using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    public class DebugUI : MonoBehaviour {
        public Label FpsCounter { get; private set; }
        public Label CurrentState { get; private set; }
        public Label Velocity { get; private set; }
        public Label VelocityMagnitude { get; private set; }
        
        private void OnEnable() {
            var root = GetComponent<UIDocument>().rootVisualElement;
            FpsCounter = root.Q<Label>("FpsCounter");
            CurrentState = root.Q<Label>("CurrentStateValue");
            Velocity = root.Q<Label>("VelocityValue");
            VelocityMagnitude = root.Q<Label>("VelocityMagnitudeValue");
        }

        private void Start() {
            GetComponent<FpsCounter>().OnFpsUpdate += fps => {
                FpsCounter.text = Math.Round(fps).ToString();
            };
        }
        
    }
}