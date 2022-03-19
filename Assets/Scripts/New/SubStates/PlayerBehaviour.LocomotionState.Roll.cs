using System;
using Animancer;
using DarkTonic.MasterAudio;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {

        [Serializable]
        public class RollState : GroundedState {
            
            /************************************************************************************************************************/

            [SerializeField] private float _AddedRollSpeed = 8f;
            [SerializeField] private AnimationCurve _RollSpeedOverLifetimeCurve;
            [SerializeField] private LinearMixerTransition _RollAnim;
            
            /************************************************************************************************************************/

            private MoveState _MoveState;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                
                Instance.OnEnterRollState(this);

                if (!_MoveState) {
                    _MoveState = Instance._MoveState;
                }
                
                Instance.Animancer.Play(_RollAnim);
                MasterAudio.PlaySound3DAtTransformAndForget("Roll", Instance.transform);
                
                _RollAnim.State.Events.OnEnd = () => {
                    StateMachine.TrySetState(Instance._IdleState);
                };

            }

            public override void Update() {
                base.Update();
                _RollAnim.State.Parameter = Instance.MoveInput.magnitude;
            }

            public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
                
                Instance._MoveState.UpdateVelocity(ref currentVelocity, deltaTime);

                float rollSpeedMultiplier = _RollSpeedOverLifetimeCurve.Evaluate(_RollAnim.State.NormalizedTime);

                float moveTargetSpeed = Instance.IsSprinting ? _MoveState._SprintSpeed : _MoveState._MoveSpeed;
                float targetRollSpeed = rollSpeedMultiplier * _AddedRollSpeed;
                
                var targetVelocity = Motor.CharacterForward * (moveTargetSpeed + targetRollSpeed);
                currentVelocity = Vector3.Lerp(
                        currentVelocity,
                        targetVelocity,
                        1f - Mathf.Exp(-Instance.StableMovementSharpness * deltaTime)
                );
                
            }

            /************************************************************************************************************************/

        }
        
    }
}