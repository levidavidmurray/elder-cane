using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    public class DebugUI : MonoBehaviour {
        public Label CurrentState { get; private set; }
        public Label Velocity { get; private set; }
        public Label FpsCounter { get; private set; }
        
        private void OnEnable() {
            var root = GetComponent<UIDocument>().rootVisualElement;
            CurrentState = root.Q<Label>("CurrentStateValue");
            Velocity = root.Q<Label>("VelocityValue");
            FpsCounter = root.Q<Label>("FpsCounter");
        }

        private void Start() {
            GetComponent<FpsCounter>().OnFpsUpdate += fps => {
                FpsCounter.text = Math.Round(fps).ToString();
            };
        }
        
    }
}