using System;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {

        [Serializable]
        public class EmptyState : ActionState {
            
            /************************************************************************************************************************/
            
            [SerializeField] private float _FadeSpeed = 0.25f;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();

                AnimLayer.StartFade(0, _FadeSpeed);
            }
            
            public override void Update() {
                base.Update();

                if (Instance.IsAttacking) {
                    StateMachine.TrySetState(Instance._AttackState);
                }
                
            }

            /************************************************************************************************************************/
            
        }
        
    }
}