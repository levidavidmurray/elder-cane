using System;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {

        [SerializeField] private IdleState _IdleState;
        
        [Serializable]
        public class IdleState : LocomotionState {
            
            [SerializeField] private float _FadeSpeed = 0.25f;
            [SerializeField] private AnimationClip _IdleClip;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                Instance.Animancer.Play(_IdleClip, _FadeSpeed);
            }

            public override void Update() {
                base.Update();

                if (Instance.MoveInput.magnitude > 0f) {
                    // Change to MoveState
                    Instance.LocomotionStateMachine.TrySetState(Instance._MoveState);
                }
            }
            
            /************************************************************************************************************************/
            
        }
        
    }
}