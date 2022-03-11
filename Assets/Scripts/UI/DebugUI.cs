using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    public class DebugUI : MonoBehaviour {
        public Label CurrentState { get; private set; }
        public Label MoveInputVector { get; private set; }

        private void OnEnable() {
            var root = GetComponent<UIDocument>().rootVisualElement;
            CurrentState = root.Q<Label>("CurrentStateValue");
            MoveInputVector = root.Q<Label>("MoveInputVectorValue");
        }
        
    }
}