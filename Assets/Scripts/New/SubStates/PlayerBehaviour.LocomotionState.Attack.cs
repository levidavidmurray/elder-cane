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
        public class AttackState : LocomotionState {
            
            /************************************************************************************************************************/
            
            [SerializeField] private float _FadeSpeed = 0.25f;
            [SerializeField] int _ComboIndex = 0;

            [SerializeField] private ClipTransition[] _NewAttackCombo0;
            [SerializeField] private ClipTransition[] _NewAttackCombo1;
            
            /************************************************************************************************************************/

            private AnimancerState _AnimState;
            private int _CurInitIndex;
            private int _CurComboAttackIndex; // Index in current combo array
            private int _CurAttackIndex; // Index of attack in total attacks

            private int _AttackCount;
            
            /************************************************************************************************************************/
            
            public override bool CanEnterState => Instance.LocomotionStateMachine.CurrentState != Instance._RollState;

            protected override void Initialize() {
                base.Initialize();

                _AttackCount += _NewAttackCombo0.Length;
                _AttackCount += _NewAttackCombo1.Length;
            }

            public override void OnEnterState() {
                base.OnEnterState();
                
                _CurComboAttackIndex = 0;
                _CurAttackIndex = 0;
                _ComboIndex = 0;

                HandleAttackNew();
            }

            public override void Update() {
                base.Update();

                if (Instance.IsAttacking) {

                    if (Instance.IsMoveInputLocked) return;

                    if (IsLastAttack) {
                        Instance.InputHandler.UseAttackLightInput();
                        return;
                    }
                        
                    _AnimState.Events.OnEnd = () => {
                        
                        _CurComboAttackIndex++;

                        if (_CurAttackIndex < _AttackCount-1) {
                            _CurAttackIndex++;
                        }
                        
                        if (_CurComboAttackIndex >= CurrentComboAnimData.Length) {
                            _ComboIndex++;
                            _CurComboAttackIndex = 0;
                        
                            if (_ComboIndex >= 2) {
                                _ComboIndex = 0;
                            }
                        }
                        HandleAttackNew();
                    };
                }
                
            }

            /************************************************************************************************************************/
            
            private bool IsInitialized => _CurInitIndex > _CurAttackIndex;

            private bool IsLastAttack => _CurAttackIndex == _AttackCount - 1;

            private ClipTransition[] CurrentComboAnimData {
                get {
                    if (_ComboIndex == 0) {
                        return _NewAttackCombo0;
                    }
                    
                    return _NewAttackCombo1;
                }
            }

            private void HandleAttackNew() {
                Instance.InputHandler.UseAttackLightInput();

                var animData = CurrentComboAnimData[_CurComboAttackIndex];
                _AnimState = Instance.Animancer.Play(animData, _FadeSpeed);

                if (!IsInitialized) {
                    _AnimState.Events.SetCallback("AttackTrailStart", () => {
                        Instance.OnAttackTrailStart();
                    });
                    _AnimState.Events.SetCallback("AttackTrailStop", () => {
                        Instance.OnAttackTrailStop();
                    });
                    
                    _CurInitIndex++;
                }

                if (IsLastAttack) {
                    Instance.LockMoveInput();
                }
                
                _AnimState.Events.OnEnd = () => {
                    StateMachine.TrySetState(Instance._IdleState);
                };
            }

        }
        
    }
}