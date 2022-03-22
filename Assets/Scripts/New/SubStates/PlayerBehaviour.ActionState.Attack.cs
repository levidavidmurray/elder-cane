using System;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {

        [Serializable]
        public class AttackState : ActionState {
            
            /************************************************************************************************************************/
            
            [SerializeField] private float _FadeSpeed = 0.25f;
            [SerializeField] private AnimationClip _AttackClip;
            
            /************************************************************************************************************************/
            
            public override bool CanEnterState => Instance.LocomotionStateMachine.CurrentState != Instance._RollState;

            public override void OnEnterState() {
                base.OnEnterState();

                AnimLayer.Stop();
                AnimLayer.StartFade(1);
                log($"OnEnterState!");
                Instance.InputHandler.UseAttackLightInput();
                var state = AnimLayer.Play(_AttackClip, _FadeSpeed);
                state.Events.OnEnd = () => {
                    StateMachine.TrySetState(Instance._EmptyState);
                };
            }

            public override void Update() {
                base.Update();

                if (Instance.InputHandler.AttackLightInput) {
                    StateMachine.TryResetState(this);
                }
            }

            /************************************************************************************************************************/

        }
        
    }
}