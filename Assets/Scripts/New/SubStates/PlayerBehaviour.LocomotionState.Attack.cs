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

            [SerializeField] private AnimationClipData[] _AttackCombo0;
            [SerializeField] private AnimationClipData[] _AttackCombo1;
            [SerializeField] private LinearMixerTransition _AttackAnim;
            
            /************************************************************************************************************************/

            private AnimancerState _AnimState;
            private int _CurAttackIndex;
            private int _AttackAnimIndex;
            private bool _IsInitialized;
            
            /************************************************************************************************************************/
            
            public override bool CanEnterState => Instance.LocomotionStateMachine.CurrentState != Instance._RollState;

            public override void OnEnterState() {
                base.OnEnterState();

                _CurAttackIndex = 0;
                _AttackAnimIndex = 0;
                _ComboIndex = 0;

                HandleAttackOld();
            }

            public override void Update() {
                base.Update();

                if (Instance.IsAttacking) {

                    if (Instance.IsMoveInputLocked) return;
                        
                    _AnimState.Events.OnEnd = () => {
                        _CurAttackIndex++;
                        // _AttackAnimIndex++;
                        // if (_AttackAnimIndex >= _AttackAnim.Animations.Length) {
                        //     _AttackAnimIndex = 0;
                        // }
                        if (_CurAttackIndex >= CurrentComboAnimData.Length) {
                            _ComboIndex++;
                            _CurAttackIndex = 0;
                        
                            if (_ComboIndex >= 2) {
                                _ComboIndex = 0;
                            }
                        }
                        HandleAttackOld();
                    };
                }
                
            }

            /************************************************************************************************************************/

            private AnimationClipData[] CurrentComboAnimData {
                get {
                    if (_ComboIndex == 0) {
                        return _AttackCombo0;
                    }
                    
                    return _AttackCombo1;
                }
            }

            private void HandleAttackOld() {
                Instance.InputHandler.UseAttackLightInput();

                var animData = CurrentComboAnimData[_CurAttackIndex];
                _AnimState = Instance.Animancer.Play(animData.Clip, _FadeSpeed);
                _AnimState.Speed = animData.Speed;
                
                _AnimState.Events.OnEnd = () => {
                    StateMachine.TrySetState(Instance._IdleState);
                };
            }

            private void HandleAttack() {
                // Instance.LockMoveInput();
                Instance.InputHandler.UseAttackLightInput();
                // var animData = CurrentComboAnimData[_CurAttackIndex];
                // _AnimState = Instance.Animancer.Play(animData.Clip, _FadeSpeed);
                // _AnimState.Speed = animData.Speed;
                if (!_IsInitialized) {
                    _AnimState = Instance.Animancer.Play(_AttackAnim, _FadeSpeed);
                    // _AnimState.Events.SetCallback("AttackTrailStart", () => {
                    //     print($"AttackTrailStart!");
                    // });
                    //
                    // _AnimState.Events.SetCallback("AttackTrailStop", () => {
                    //     print($"AttackTrailStop!");
                    // });
                    
                    _AnimState.Events.OnEnd = () => {
                        StateMachine.TrySetState(Instance._IdleState);
                    };
                    
                    _IsInitialized = true;
                }
                
                _AttackAnim.State.Parameter = _AttackAnimIndex;
                _AnimState.Play();
            }

        }
        
    }
}