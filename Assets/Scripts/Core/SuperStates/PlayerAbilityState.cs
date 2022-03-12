using UnityEngine;

namespace EC.Core.SuperStates {
    public abstract class PlayerAbilityState : PlayerState {
        
        protected bool isAbilityDone;
        
        private float lastFinishTime;
        private bool isGrounded;

        public PlayerAbilityState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
        }
        
        public abstract float GetAbilityCooldown();
        
        public bool AbilityIsAvailable() => Time.time - lastFinishTime >= GetAbilityCooldown();

        public override void DoChecks() {
            base.DoChecks();
            isGrounded = Controller.CheckIfGrounded();
        }

        public override void Enter() {
            base.Enter();

            isAbilityDone = !AbilityIsAvailable();
        }

        public override void Exit() {
            base.Exit();
            lastFinishTime = Time.time;
        }

        public override void LogicUpdate() {
            base.LogicUpdate();

            if (!isAbilityDone) return;

            if (isGrounded && Controller.Velocity.y < 0.01f) {
                stateMachine.ChangeState(Controller.IdleState);
            }
            else {
                stateMachine.ChangeState(Controller.InAirState);
            }
        }
        
    }
}