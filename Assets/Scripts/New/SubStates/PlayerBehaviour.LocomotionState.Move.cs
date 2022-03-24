using System;
using Animancer;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {
        
        [Serializable]
        public class MoveState : GroundedState {
            
            public float _SprintSpeed = 9f;
            [SerializeField] private float _SprintFadeSpeed = 2f;
            [SerializeField] private float _BackwardMoveFadeSpeed = 5f;
            [SerializeField] private float _BackwardMoveMultiplier = 0.5f;
            [SerializeField] private LinearMixerTransition _MoveAnim;
            [SerializeField] private MixerTransition2D _LockedMoveAnim;
            
            /************************************************************************************************************************/

            private bool _IsInitialized;
            private float _DefaultMoveSpeed;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();

                if (!_IsInitialized) {
                    Initialize();
                }
                
                Instance.Animancer.Play(_LockedMoveAnim);
            }

            public override void OnExitState() {
                base.OnExitState();
                
                Instance.ResetOrientationMethod();
            }

            public override void Update() {
                base.Update();

                float inputMag = Instance.MoveInput.magnitude;
                
                // Transition to IdleState if there's no movement input
                if (inputMag == 0) {
                    StateMachine.TrySetState(Instance._IdleState);
                }

                Vector2 planarVelocity = new Vector2(Motor.Velocity.x, Motor.Velocity.z);
                Vector2 animParam = new Vector2(0, planarVelocity.magnitude / _MoveSpeed);
                
                if (inputMag >= 0.9f && Instance.IsSprinting) {
                    Instance.OrientationMethod = OrientationMethodType.TowardsMovement;
                    _LockedMoveAnim.State.Parameter = Vector2.MoveTowards(
                        _LockedMoveAnim.State.Parameter,
                        new Vector2(0, 2f),
                        Time.deltaTime * _SprintFadeSpeed
                    );
                    
                    _MoveSpeed = _SprintSpeed * (_LockedMoveAnim.State.Parameter.y / 2f);
                    
                    return;
                }

                if (StateMachine.CurrentState == this) {
                    Instance.ResetOrientationMethod();
                }

                if (Instance.IsTargetLocked) {
                    animParam = Instance.MoveInput;
                    if (animParam.y < 0) {
                        _MoveSpeed = _DefaultMoveSpeed * _BackwardMoveMultiplier;
                    }
                    else {
                        _MoveSpeed = _DefaultMoveSpeed;
                    }
                
                    _LockedMoveAnim.State.Parameter = animParam;
                    return;
                }
                
                _LockedMoveAnim.State.Parameter = animParam;
                
                _MoveSpeed = _DefaultMoveSpeed;

            }
            
            /************************************************************************************************************************/

            private void Initialize() {
                _DefaultMoveSpeed = _MoveSpeed;

                _IsInitialized = true;
            }
            
        }
        
    }
}