using EC.Core.SuperStates;
using UnityEngine;

namespace EC.Core.SubStates {
    public class PlayerRollState : PlayerAbilityState {
        
        private float defaultClipDuration;
        private Vector2 MoveInput;
        private bool isBackflip;
        
        public PlayerRollState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
            Controller.OnRollComplete += () => isAbilityDone = true;
        }

        public override float GetAbilityCooldown() => controllerData.RollCooldown;

        public override void Enter() {
            base.Enter();

            Controller.Anim.SetTrigger(Controller.AnimProp_Roll);

            float animSpeed = controllerData.RollAnimSpeed;
            if (ShouldBackflip) {
                animSpeed = controllerData.FlipAnimSpeed;
            }

            MoveInput = Controller.InputHandler.MoveInput;
            Controller.Anim.speed = animSpeed;
            isBackflip = ShouldBackflip;
        }

        public override void Exit() {
            base.Exit();
            Controller.Anim.speed = 1f;
        }

        public override void LogicUpdate() {
            base.LogicUpdate();
            MoveInput = Controller.InputHandler.MoveInput;

            var jumpInput = Controller.InputHandler.JumpInput;

            if (jumpInput && Controller.JumpState.CanJump()) {
                stateMachine.ChangeState(Controller.JumpState);
            }
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            base.UpdateVelocity(ref currentVelocity, deltaTime);
            HandleRollVelocity(ref currentVelocity, deltaTime);
        }

        void HandleRollVelocity(ref Vector3 currentVelocity, float deltaTime) {
            float currentVelocityMagnitude = currentVelocity.magnitude;

            Vector3 effectiveGroundNormal = Controller.Motor.GroundingStatus.GroundNormal;

            // Reorient velocity on slope
            currentVelocity = Controller.Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                              currentVelocityMagnitude;

            // float animSpeed = controllerData.RollAnimSpeedCurve.Evaluate(rollTime / defaultClipDuration);
            // Anim.speed = controllerData.RollAnimSpeed;
            // float speedMultiplier = controllerData.RollSpeedCurve.Evaluate(rollTime / rollClipDuration);
            float speedMultiplier = 1f;

            int dirSign = 1;
            float rollSpeed = controllerData.MaxRollSpeed;

            if (isBackflip) {
                dirSign = -1;
                rollSpeed = controllerData.MaxFlipSpeed;
            }
            
            // Calculate target velocity
            // Vector3 reorientedInput = Controller.ReorientedInput();
            Vector3 targetMovementVelocity = (Controller.transform.forward * dirSign) * (rollSpeed * speedMultiplier);
            
            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1f - Mathf.Exp(-controllerData.StableMovementSharpness * deltaTime));
            
        }

        public bool CanRoll() => AbilityIsAvailable();
        
        private bool ShouldBackflip => Controller.IsLockedOn && MoveInput.y < 0;
        
    }
}