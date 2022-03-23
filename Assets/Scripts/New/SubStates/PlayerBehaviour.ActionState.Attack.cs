using System;
using Animancer;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {

        [Serializable]
        public class AnimationClipData {
            public AnimationClip Clip;
            public float Speed = 1;
        }

        [Serializable]
        public class AttackState : ActionState {
            
            /************************************************************************************************************************/
            
            [SerializeField] private float _FadeSpeed = 0.25f;
            [SerializeField] int _ComboxIndex = 0;

            [SerializeField] private AnimationClipData[] _AttackCombo0;
            [SerializeField] private AnimationClipData[] _AttackCombo1;
            
            /************************************************************************************************************************/

            private AnimancerState _AnimState;
            private int _CurAttackIndex;
            
            /************************************************************************************************************************/
            
            public override bool CanEnterState => Instance.LocomotionStateMachine.CurrentState != Instance._RollState;

            public override void OnEnterState() {
                base.OnEnterState();

                _CurAttackIndex = 0;
                _ComboxIndex = 0;

                HandleAttack();
            }

            public override void Update() {
                base.Update();

                if (Instance.InputHandler.AttackLightInput) {
                    _AnimState.Events.OnEnd = () => {
                        _CurAttackIndex++;
                        if (_CurAttackIndex >= CurrentComboAnimData.Length) {
                            _ComboxIndex++;
                            _CurAttackIndex = 0;

                            if (_ComboxIndex >= 2) {
                                _ComboxIndex = 0;
                            }
                        }
                        HandleAttack();
                    };
                }
            }

            /************************************************************************************************************************/

            private AnimationClipData[] CurrentComboAnimData {
                get {
                    if (_ComboxIndex == 0) {
                        return _AttackCombo0;
                    }
                    
                    return _AttackCombo1;
                }
            }

            private void HandleAttack() {
                Instance.InputHandler.UseAttackLightInput();
                var animData = CurrentComboAnimData[_CurAttackIndex];
                _AnimState = AnimLayer.Play(animData.Clip, _FadeSpeed);
                _AnimState.Speed = animData.Speed;
                _AnimState.Events.OnEnd = () => {
                    StateMachine.TrySetState(Instance._EmptyState);
                };
            }

        }
        
    }
}