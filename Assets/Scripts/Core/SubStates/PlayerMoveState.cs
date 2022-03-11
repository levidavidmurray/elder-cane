using EC.Core.SuperStates;
using UnityEngine;

namespace EC.Core.SubStates {
    public class PlayerMoveState : PlayerGroundedState {

        public PlayerMoveState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
        }

        public override void Enter() {
            base.Enter();
        }

        public override void Exit() {
            base.Exit();
        }

        public override void DoChecks() {
            base.DoChecks();
        }

        public override void LogicUpdate() {
            base.LogicUpdate();

            if (MoveInput.magnitude == 0) {
                stateMachine.ChangeState(Controller.IdleState);
                return;
            }

            Vector3 curVel = Controller.Velocity;
            float speedPercent = curVel.magnitude / controllerData.MaxStableMoveSpeed;
            Controller.Anim.SetFloat(AnimProp_SpeedPercent, speedPercent);
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            base.UpdateVelocity(ref currentVelocity, deltaTime);
            
            float currentVelocityMagnitude = currentVelocity.magnitude;

            Vector3 effectiveGroundNormal = Controller.Motor.GroundingStatus.GroundNormal;
            Vector3 MoveInputVector = Controller.MoveInputVector;

            // Reorient velocity on slope
            currentVelocity = Controller.Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                              currentVelocityMagnitude;

            float speedMultiplier = controllerData.VelocityMagnitudeSpeedCurve.Evaluate(MoveInput.magnitude);
            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(MoveInputVector, Controller.Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized *
                                      MoveInputVector.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * (controllerData.MaxStableMoveSpeed * speedMultiplier);

            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1f - Mathf.Exp(-controllerData.StableMovementSharpness * deltaTime));
        }
    }
}