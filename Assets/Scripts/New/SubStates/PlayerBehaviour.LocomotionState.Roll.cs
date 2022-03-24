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
            private bool _IsInitialized;
            private bool _CanJump;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                
                Instance.InputHandler.UseRollInput();
                
                Instance.Animancer.Play(_RollAnim);
                _CanJump = false;

                if (!_IsInitialized) {
                    _MoveState = Instance._MoveState;
                    _RollAnim.State.Events.SetCallback("PlaySfx", () => {
                        MasterAudio.PlaySound3DAtTransformAndForget("Roll", Instance.transform);
                    });
                    
                    _RollAnim.State.Events.SetCallback("CanJump", () => _CanJump = true);
                    
                    _RollAnim.State.Events.OnEnd = () => {
                        StateMachine.TrySetState(Instance._IdleState);
                    };

                    _IsInitialized = true;
                }
                
                Instance.OrientationMethod = OrientationMethodType.TowardsMovement;
            }

            public override void OnExitState() {
                base.OnExitState();
                Instance.ResetOrientationMethod();
            }

            public override void Update() {
                base.Update();
                Instance.InputHandler.UseRollInput();
                _RollAnim.State.Parameter = Instance.MoveInput.magnitude;

                if (Instance.IsJumping && _CanJump) {
                    StateMachine.TrySetState(Instance._JumpState);
                }
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