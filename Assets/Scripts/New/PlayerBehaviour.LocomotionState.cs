using System;
using KinematicCharacterController;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {
        
        [Serializable]
        public abstract class LocomotionState : State {

            [SerializeField] protected float _MoveSpeed;
            
            protected KinematicCharacterMotor Motor;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                Motor = Instance.Motor;

                if (Instance.IsGrounded) {
                    Instance._JumpState.ResetJumps();
                    Instance.InputHandler.UseJumpInput();
                }
            }

            public override void Update() {
                base.Update();

                if (!Instance.IsGrounded) return;

                var jumpInput = Instance.InputHandler.JumpInput;

                if (jumpInput && Instance._JumpState.CanEnterState) {
                    Instance.LocomotionStateMachine.TrySetState(Instance._JumpState);
                }
            }

            public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
                
                float currentVelocityMagnitude = currentVelocity.magnitude;
    
                Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
    
                // Reorient velocity on slope
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                                  currentVelocityMagnitude;
    
                // Calculate target velocity
                Vector3 reorientedInput = ReorientedInput();
                Vector3 targetMovementVelocity = reorientedInput * _MoveSpeed;
    
                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                    1f - Mathf.Exp(-Instance.StableMovementSharpness * deltaTime));
            }
            
            /************************************************************************************************************************/

            // Account for sloped surfaces
            private Vector3 ReorientedInput() {
                Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
                Vector3 inputRight = Vector3.Cross(Instance.MoveInputVector, Motor.CharacterUp);
                return Vector3.Cross(effectiveGroundNormal, inputRight).normalized * Instance.MoveInputVector.magnitude;
            }
            
        }
        
    }
}