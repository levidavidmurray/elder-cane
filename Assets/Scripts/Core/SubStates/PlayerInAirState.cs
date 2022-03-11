using KinematicCharacterController;
using UnityEngine;

namespace EC.Core.SubStates {
    public class PlayerInAirState : PlayerState {

        private static readonly int AnimProp_YVelocity = Animator.StringToHash("yVelocity");
        
        private bool isGrounded;

        private KinematicCharacterMotor Motor;

        public PlayerInAirState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
            Motor = Controller.Motor;
        }

        public override void DoChecks() {
            base.DoChecks();

            isGrounded = Controller.CheckIfGrounded();
        }

        public override void Enter() {
            base.Enter();
            
            Controller.Anim.SetBool(Controller.AnimProp_IsGrounded, false);
        }

        public override void LogicUpdate() {
            base.LogicUpdate();

            if (isGrounded && Controller.Motor.Velocity.y < 0.01f) {
                stateMachine.ChangeState(Controller.LandState);
            }
            
            Controller.Anim.SetFloat(AnimProp_YVelocity, Controller.Velocity.y);
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            base.UpdateVelocity(ref currentVelocity, deltaTime);

            if (Controller.MoveInputVector.sqrMagnitude > 0f) {
                Vector3 addedVelocity = Controller.MoveInputVector * controllerData.AirAccelerationSpeed * deltaTime;

                Vector3 currentVelocityOnInputsPlane =
                    Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < controllerData.MaxAirMoveSpeed) {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                    Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity,
                        controllerData.MaxAirMoveSpeed);
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

                // Apply added velocity
                currentVelocity += addedVelocity;
            }

            // Gravity
            currentVelocity += controllerData.Gravity * deltaTime;

            // Drag
            currentVelocity *= (1f / (1f + (controllerData.Drag * deltaTime)));
        }
        
    }
}