using EC.Core.SuperStates;
using UnityEngine;

namespace EC.Core.SubStates {
    public class PlayerRollState : PlayerAbilityState {
        
        private float rollStartTime;
        private Animator Anim;
        private AnimationClip RollClip;
        private float defaultClipDuration;
        private float rollClipDuration;
        
        public PlayerRollState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
            Anim = Controller.Anim;
        }

        public override float GetAbilityCooldown() => controllerData.RollCooldown;

        public override void Enter() {
            base.Enter();

            rollStartTime = Time.time;
            Controller.Anim.SetTrigger(Controller.AnimProp_Roll);
        }

        public override void Exit() {
            base.Exit();
            Controller.Anim.speed = 1f;
        }

        public override void LogicUpdate() {
            base.LogicUpdate();

            // float rollTime = Time.time - rollStartTime;
            // float speedMultiplier = controllerData.RollSpeedCurve.Evaluate(rollTime / controllerData.RollDuration);
            // float rollSpeed = speedMultiplier * controllerData.MaxRollSpeed;
            //
            // Controller.AddVelocity(Controller.transform.forward * rollSpeed);

            isAbilityDone = RollFinished;
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            base.UpdateVelocity(ref currentVelocity, deltaTime);

            if (!RollClip) {
                var animClipInfo = Anim.GetCurrentAnimatorClipInfo(0);
                RollClip = animClipInfo[0].clip;
                defaultClipDuration = RollClip.length;
            }
            
            float currentVelocityMagnitude = currentVelocity.magnitude;

            Vector3 effectiveGroundNormal = Controller.Motor.GroundingStatus.GroundNormal;

            // Reorient velocity on slope
            currentVelocity = Controller.Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                              currentVelocityMagnitude;

            float rollTime = Time.time - rollStartTime;
            // float animSpeed = controllerData.RollAnimSpeedCurve.Evaluate(rollTime / defaultClipDuration);
            Anim.speed = controllerData.RollAnimSpeed;
            rollClipDuration = defaultClipDuration * (1 / Anim.speed);
            float speedMultiplier = controllerData.RollSpeedCurve.Evaluate(rollTime / rollClipDuration);
            
            // Calculate target velocity
            // Vector3 reorientedInput = Controller.ReorientedInput();
            Vector3 targetMovementVelocity = Controller.transform.forward * (controllerData.MaxRollSpeed * speedMultiplier);
            
            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1f - Mathf.Exp(-controllerData.StableMovementSharpness * deltaTime));
        }

        public bool CanRoll() => RollFinished && AbilityIsAvailable();
        
        private bool RollFinished => Time.time - rollStartTime >= rollClipDuration;
        
    }
}