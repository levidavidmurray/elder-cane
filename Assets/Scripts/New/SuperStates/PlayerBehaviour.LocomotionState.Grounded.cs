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
                Instance.OnEnterGroundedState(this);
            }

            public override void Update() {
                base.Update();

                if (!Instance.IsGrounded) {
                    StateMachine.TrySetState(Instance._InAirState);
                    return;
                }

                if (Instance.IsJumping) {
                    StateMachine.TrySetState(Instance._JumpState);
                    return;
                }

                if (Instance.IsRolling) {
                    log("DO ROLL!");
                    Instance.OnEnterRollState(this);
                }
                
            }
            
            /************************************************************************************************************************/
            
        }
        
    }
}