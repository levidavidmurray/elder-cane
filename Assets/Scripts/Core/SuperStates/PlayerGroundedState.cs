using UnityEngine;

namespace EC.Core.SuperStates {
    public class PlayerGroundedState : PlayerState {

        protected Vector2 MoveInput;
        
        protected static readonly int AnimProp_SpeedPercent = Animator.StringToHash("SpeedPercent");

        private bool JumpInput;
        private bool RollInput;

        private bool isGrounded;

        public PlayerGroundedState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller,
            stateMachine, controllerData) {
        }

        public override void DoChecks() {
            base.DoChecks();
            isGrounded = Controller.CheckIfGrounded();
        }

        public override void Enter() {
            base.Enter();
            
            Controller.Anim.SetBool(Controller.AnimProp_IsGrounded, true);
            Controller.JumpState.ResetJumps();
            Controller.InputHandler.UseJumpInput();
        }

        public override void Exit() {
            base.Exit();
        }

        public override void LogicUpdate() {
            base.LogicUpdate();

            MoveInput = Controller.InputHandler.MoveInput;
            JumpInput = Controller.InputHandler.JumpInput;
            RollInput = Controller.InputHandler.RollInput;
            

            if (JumpInput && Controller.JumpState.CanJump()) {
                stateMachine.ChangeState(Controller.JumpState);
                return;
            }

            if (RollInput && Controller.RollState.CanRoll()) {
                stateMachine.ChangeState(Controller.RollState);
                return;
            }
        }

        public override void PhysicsUpdate() {
            base.PhysicsUpdate();
        }
    }
}