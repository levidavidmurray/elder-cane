using EC.Core.SuperStates;
using UnityEngine;

namespace EC.Core.SubStates {
    public class PlayerLandState : PlayerGroundedState {
        private static readonly int Land = Animator.StringToHash("Land");

        public PlayerLandState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
        }

        public override void Enter() {
            base.Enter();
            Controller.Anim.SetTrigger(Land);
        }

        public override void Exit() {
            base.Exit();
        }

        public override void LogicUpdate() {
            base.LogicUpdate();

            if (isExitingState) return;

            if (Controller.Velocity.magnitude != 0) {
                stateMachine.ChangeState(Controller.MoveState);
            }
            else {
                stateMachine.ChangeState(Controller.IdleState);
            }
            
        }
    }
}