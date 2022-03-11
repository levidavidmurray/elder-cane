using UnityEngine;

namespace EC.Core.SuperStates {
    public class PlayerAbilityState : PlayerState {
        
        protected bool isAbilityDone;
        private bool isGrounded;

        public PlayerAbilityState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
        }

        public override void DoChecks() {
            base.DoChecks();

            isGrounded = Controller.CheckIfGrounded();
        }

        public override void Enter() {
            base.Enter();

            isAbilityDone = false;
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