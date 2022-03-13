using EC.Core.SuperStates;
using KinematicCharacterController;
using UnityEngine;

namespace EC.Core.SubStates {
    public class PlayerJumpState : PlayerAbilityState {
        
        private KinematicCharacterMotor Motor;
        private int jumpsRemaining;
        private bool velocityDidUpdate;
        
        private float initialJumpVelocity;
        private float maxJumpTime;
        private float maxJumpHeight;

        public PlayerJumpState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
            ResetJumps();
            Motor = Controller.Motor;
        }

        public override void Enter() {
            base.Enter();
            jumpsRemaining--;
            velocityDidUpdate = false;

            maxJumpTime = controllerData.MaxJumpTime;
            maxJumpHeight = controllerData.MaxJumpHeight;
            
            float timeToApex = maxJumpTime / 2f;
            Controller.InAirState.Gravity = (-2 * maxJumpHeight) / (timeToApex * timeToApex);
            initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        }

        public override void LogicUpdate() {
            base.LogicUpdate();

            if (isAbilityDone) return;
            
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
            // currentVelocity += (jumpDirection * controllerData.JumpUpSpeed) -
            //                    Vector3.Project(currentVelocity, Motor.CharacterUp);
            currentVelocity.y = initialJumpVelocity;
            currentVelocity += (Controller.MoveInputVector * controllerData.JumpScalableForwardSpeed);
            
            velocityDidUpdate = true;
        }

        public override float GetAbilityCooldown() => controllerData.JumpCooldown;

        public bool CanJump() => jumpsRemaining > 0 && AbilityIsAvailable();

        public void ResetJumps() => jumpsRemaining = controllerData.jumpCount;

    }
}