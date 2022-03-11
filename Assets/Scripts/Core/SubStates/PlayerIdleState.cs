using EC.Core.SuperStates;
using UnityEngine;

namespace EC.Core.SubStates {
    public class PlayerIdleState : PlayerGroundedState {
        
        public PlayerIdleState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
        }

        public override void Enter() {
            base.Enter();
        }

        public override void Exit() {
            base.Exit();
        }

        public override void LogicUpdate() {
            base.LogicUpdate();

            if (isExitingState) return;

            if (MoveInput.magnitude != 0f) {
                stateMachine.ChangeState(Controller.MoveState);
            }
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            base.UpdateVelocity(ref currentVelocity, deltaTime);

            currentVelocity = Vector3.zero;
        }
    }
}