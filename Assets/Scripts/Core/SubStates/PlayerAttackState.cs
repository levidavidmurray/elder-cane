using EC.Core.SuperStates;

namespace EC.Core.SubStates {
    public class PlayerAttackState : PlayerAbilityState {
        
        private bool AttackLightInput;
        private bool canAttackAgain;
        private bool didAttackAgain;
        
        public PlayerAttackState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) : base(Controller, stateMachine, controllerData) {
            Controller.OnAttackComplete += () => {
                if (didAttackAgain) {
                    stateMachine.ChangeState(this);
                    return;
                }

                isAbilityDone = true;
            };
        }

        public override void Enter() {
            base.Enter();
            Controller.Anim.SetTrigger(Controller.AnimProp_AttackLight);
            Controller.InputHandler.UseAttackLightInput();
            
            AttackLightInput = Controller.InputHandler.AttackLightInput;
            if (!AttackLightInput) {
                canAttackAgain = true;
            }
        }

        public override void Exit() {
            base.Exit();
            canAttackAgain = false;
            didAttackAgain = false;
        }

        public override void LogicUpdate() {
            base.LogicUpdate();
            AttackLightInput = Controller.InputHandler.AttackLightInput;
            if (!canAttackAgain && !AttackLightInput) {
                canAttackAgain = true;
                return;
            }

            if (canAttackAgain && AttackLightInput) {
                didAttackAgain = true;
            }
        }

        public override float GetAbilityCooldown() {
            return 0;
        }
    }
}