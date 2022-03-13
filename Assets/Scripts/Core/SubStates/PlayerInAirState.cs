using KinematicCharacterController;
using UnityEngine;

namespace EC.Core.SubStates {
    public class PlayerInAirState : PlayerState {

        public float Gravity = -9.81f;
        
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
            Controller.Anim.ResetTrigger(Controller.AnimProp_Land);
        }

        public override void Exit() {
            base.Exit();
            // Controller.InputHandler.UseJumpInput();
        }

        public override void LogicUpdate() {
            base.LogicUpdate();

            if (isGrounded && Controller.Motor.Velocity.y < 0.01f) {
                stateMachine.ChangeState(Controller.LandState);
            }
            
            Controller.Anim.SetFloat(Controller.AnimProp_YVelocity, Controller.Velocity.y);
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            base.UpdateVelocity(ref currentVelocity, deltaTime);

            bool isFalling = currentVelocity.y <= 0f || Controller.InputHandler.JumpInputStop;
            float fallMultiplier = 2f;

            Vector3 addedVelocity = Vector3.zero;
            if (Controller.MoveInputVector.sqrMagnitude > 0f) {
                addedVelocity = Controller.MoveInputVector * controllerData.AirAccelerationSpeed * deltaTime;

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
            currentVelocity.y = nextYVelocity;

            // Drag
            currentVelocity *= (1f / (1f + (controllerData.Drag * deltaTime)));
        }
        
    }
}