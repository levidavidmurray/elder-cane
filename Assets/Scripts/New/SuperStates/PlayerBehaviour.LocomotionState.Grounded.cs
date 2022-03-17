using System;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {
        
        [Serializable]
        public abstract class GroundedState : LocomotionState {
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                
                Instance._JumpState.ResetJumps();
                Instance.InputHandler.UseJumpInput();
            }

            public override void Update() {
                base.Update();

                if (!Instance.IsGrounded) {
                    StateMachine.TrySetState(Instance._InAirState);
                    return;
                }

                if (Instance.InputHandler.JumpInput) {
                    StateMachine.TrySetState(Instance._JumpState);
                }
            }
            
            /************************************************************************************************************************/
            
        }
        
    }
}