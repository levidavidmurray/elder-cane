using System;
using Animancer;
using DarkTonic.MasterAudio;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {

        [Serializable]
        public class LandState : GroundedState {
            
            /************************************************************************************************************************/
            
            [SerializeField] private float _EntryFadeSpeed;
            [SerializeField] private float _StumbleLandVelocityThreshold = -20;
            [SerializeField] private float _UnderThresholdMoveLandEndTime = 0.4f;
            [SerializeField] private float _HardLandVelocityThreshold = -30;
            [SerializeField] private float _HardLandMoveTimeout = 0.5f;
            
            [SerializeField] private LinearMixerTransition _LandAnim;
            
            /************************************************************************************************************************/

            private bool _IsInitialized;
            private bool _IsHardLanding;
            private bool _CanMoveFromHardLanding;
            private float _DefaultMoveSpeed;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();

                if (!_IsInitialized) {
                    Initialize();
                }
                
                MasterAudio.PlaySound3DAtTransformAndForget("Land", Instance.transform);
                
                _IsHardLanding = Instance.VelocityLastTick.y <= _HardLandVelocityThreshold;
                
                if (_IsHardLanding) {
                    HandleHardLand();
                    return;
                }

                HandleLand();
            }

            public override void OnExitState() {
                base.OnExitState();

                _MoveSpeed = _DefaultMoveSpeed;
                _CanMoveFromHardLanding = false;
            }

            public override void Update() {
                base.Update();

                if (_IsHardLanding) {

                    if (_CanMoveFromHardLanding && Instance.IsMoving) {
                        StateMachine.TrySetState(Instance._MoveState);
                    }
                    
                    return;
                }

                if (_LandAnim.State.Parameter > 0 && !Instance.IsMoving) {
                    _LandAnim.State.Parameter = GetAnimParam();
                }
            }
            
            /************************************************************************************************************************/

            private void Initialize() {
                _DefaultMoveSpeed = _MoveSpeed;
                
                _LandAnim.Events.SetCallback("CanMove", () => _CanMoveFromHardLanding = true);

                _IsInitialized = true;
            }

            private void HandleHardLand() {
                Instance.Animancer.Play(_LandAnim, _EntryFadeSpeed);
                
                // Hard landing animation
                _LandAnim.State.Parameter = 2f;
                
                // Tie animation duration to how long movement should be suspended for
                // _LandAnim.State.NormalizedEndTime = _HardLandMoveTimeout / _LandAnim.State.Duration;
                _LandAnim.State.Speed = _LandAnim.State.Duration / _HardLandMoveTimeout;

                _MoveSpeed = 0;
                _LandAnim.State.Events.OnEnd = () => {
                    StateMachine.TrySetState(Instance._IdleState);
                };
            }

            private void HandleLand() {
                Instance.Animancer.Play(_LandAnim, _EntryFadeSpeed);

                _LandAnim.State.Parameter = GetAnimParam();
                _LandAnim.State.NormalizedEndTime = GetAnimEndTime();
                _LandAnim.State.Events.OnEnd = () => {
                    StateMachine.TrySetState(Instance._IdleState);
                };
            }
            
            private float GetAnimParam() {
                return ShouldStumble() ? 1 : 0;
            }

            private float GetAnimEndTime() {
                if (!ShouldStumble() && Instance.IsMoving) {
                    return _UnderThresholdMoveLandEndTime;
                }

                return 1f;
            }
            
            private bool ShouldStumble() {
                bool isInVelocityThreshold = Instance.VelocityLastTick.y <= _StumbleLandVelocityThreshold;
                return isInVelocityThreshold && Instance.IsMoving;
            }
            
        }
        
    }
}