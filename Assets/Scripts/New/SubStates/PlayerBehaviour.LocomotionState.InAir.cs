using System;
using Animancer;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {
        
        [Serializable]
        public class InAirState : LocomotionState {

            /************************************************************************************************************************/
            
            public float Gravity = -48f;
            
            /************************************************************************************************************************/
            
            [SerializeField] private float _JumpUpFadeSpeed = 0.15f;
            [SerializeField] private float _FallFadeSpeed = 0.25f;
            [SerializeField] private float _AirAccelerationSpeed = 15f;
            [SerializeField] private float _Drag = 0.1f;
            
            [SerializeField] private LinearMixerTransition _InAirAnim;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                Instance.Animancer.Play(_InAirAnim, _JumpUpFadeSpeed);
            }

            public override void Update() {
                base.Update();
                var isFalling = Motor.Velocity.y < 0;
                var target = isFalling ? 1 : 0;
                _InAirAnim.State.Parameter = Mathf.MoveTowards(
                    _InAirAnim.State.Parameter,
                    target,
                    Time.deltaTime * _FallFadeSpeed
                );
            }

            public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
                bool isFalling = currentVelocity.y <= 0f || !Instance.IsJumping;
                float fallMultiplier = 2f;

                if (Instance.IsMoving) {
                    
                    #region In-Air Planar Velocity
                    var addedVelocity = Instance.MoveInputVector * _AirAccelerationSpeed * deltaTime;

                    Vector3 currentVelocityOnInputsPlane =
                        Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                    // Limit air velocity from inputs
                    if (currentVelocityOnInputsPlane.magnitude < _MoveSpeed) {
                        // clamp addedVel to make total vel not exceed max vel on inputs plane
                        Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity,
                            _MoveSpeed);
                        addedVelocity = newTotal - currentVelocityOnInputsPlane;
                    }
                    else {
                        // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                        if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f) {
                            addedVelocity = Vector3.ProjectOnPlane(addedVelocity,
                                currentVelocityOnInputsPlane.normalized);
                        }
                    }

                    // Prevent air-climbing sloped walls
                    if (Motor.GroundingStatus.FoundAnyGround) {
                        if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f) {
                            Vector3 perpenticularObstructionNormal = Vector3
                                .Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal),
                                    Motor.CharacterUp).normalized;
                            addedVelocity =
                                Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                        }
                    }
                    #endregion

                    // Apply added velocity
                    currentVelocity += addedVelocity;
                }

                #region Velocity Verlet (Gravity)
                // Velocity Verlet Gravity
                float previousYVelocity = currentVelocity.y;
                float newYVelocity;
                if (isFalling) {
                    newYVelocity = previousYVelocity + (Gravity * fallMultiplier * deltaTime);
                }
                else {
                    newYVelocity = previousYVelocity + (Gravity * deltaTime);
                }
                
                float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
                #endregion
                
                currentVelocity.y = nextYVelocity;

                // Drag
                currentVelocity *= (1f / (1f + (_Drag * deltaTime)));

                if (currentVelocity.y < 0 && Instance.IsGrounded) {
                    StateMachine.TrySetState(Instance._LandState);
                }

            }
            
            /************************************************************************************************************************/
            
        }
        
    }
}