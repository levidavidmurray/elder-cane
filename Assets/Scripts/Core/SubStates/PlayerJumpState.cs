using EC.Core.SuperStates;
using KinematicCharacterController;
using UnityEngine;

namespace EC.Core.SubStates {
    public class PlayerJumpState : PlayerAbilityState {
        
        private KinematicCharacterMotor Motor;
        private int jumpsRemaining;
        private bool velocityDidUpdate;

        public PlayerJumpState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
            ResetJumps();
            Motor = Controller.Motor;
        }

        public override void Enter() {
            base.Enter();
            Controller.InputHandler.UseJumpInput();
            jumpsRemaining--;
            velocityDidUpdate = false;
        }

        public override void LogicUpdate() {
            base.LogicUpdate();
            isAbilityDone = velocityDidUpdate;
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            base.UpdateVelocity(ref currentVelocity, deltaTime);
            
            Vector3 jumpDirection = Motor.CharacterUp;
            if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround) {
                jumpDirection = Motor.GroundingStatus.GroundNormal;
            }
            
            // TODO: Add PlayerSlideState
            // controllerData.AllowJumpingWhenSliding
            //           ? Motor.GroundingStatus.FoundAnyGround
            //           : Motor.GroundingStatus.IsStableOnGround
        
            // Makes the character skip ground probing/snapping on its next update. 
            // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
            Motor.ForceUnground();
        
            // Add to the return velocity and reset jump state
            currentVelocity += (jumpDirection * controllerData.JumpUpSpeed) -
                               Vector3.Project(currentVelocity, Motor.CharacterUp);
            currentVelocity += (Controller.MoveInputVector * controllerData.JumpScalableForwardSpeed);
            
            velocityDidUpdate = true;
        }

        public bool CanJump() => jumpsRemaining > 0;

        public void ResetJumps() => jumpsRemaining = controllerData.jumpCount;

    }
}