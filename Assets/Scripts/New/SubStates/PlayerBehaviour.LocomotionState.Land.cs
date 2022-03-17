using System;
using Animancer;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {

        [Serializable]
        public class LandState : GroundedState {
            
            /************************************************************************************************************************/
            
            [SerializeField] private float _EntryFadeSpeed;
            [SerializeField] private float _YVelocityStumbleThreshold = -20;
            [SerializeField] private float _UnderThresholdMoveLandEndTime = 0.6f;
            [SerializeField] private LinearMixerTransition _LandAnim;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                Instance.Animancer.Play(_LandAnim, _EntryFadeSpeed);

                _LandAnim.State.Parameter = GetAnimParam();
                _LandAnim.State.NormalizedEndTime = GetAnimEndTime();
                log($"NormalizedEndTime: {_LandAnim.State.NormalizedEndTime}");
                _LandAnim.State.Events.OnEnd = () => {
                    StateMachine.TrySetState(Instance._IdleState);
                };
            }

            public override void Update() {
                base.Update();

                if (_LandAnim.State.Parameter > 0 && !Instance.IsMoving) {
                    _LandAnim.State.Parameter = GetAnimParam();
                }
            }
            
            /************************************************************************************************************************/
            
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
                bool isInVelocityThreshold = Instance.VelocityLastTick.y <= _YVelocityStumbleThreshold;
                return isInVelocityThreshold && Instance.IsMoving;
            }
            
        }
        
    }
}