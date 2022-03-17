using System;
using Animancer;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {
        
        [Serializable]
        public class MoveState : GroundedState {
            
            [SerializeField] private float _SprintSpeed = 9f;
            [SerializeField] private float _SprintFadeSpeed = 2f;
            [SerializeField] private LinearMixerTransition _MoveAnim;
            
            /************************************************************************************************************************/

            private bool _IsInitialized;
            private float _DefaultMoveSpeed;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();

                if (!_IsInitialized) {
                    Initialize();
                }
                
                Instance.Animancer.Play(_MoveAnim);
            }

            public override void Update() {
                base.Update();

                float inputMag = Instance.MoveInput.magnitude;

                if (inputMag >= 0.9f && Instance.IsSprinting) {
                    _MoveSpeed = _SprintSpeed;
                    _MoveAnim.State.Parameter = 2f;
                    
                    return;
                }
                
                _MoveSpeed = _DefaultMoveSpeed;

                Vector3 velocity = Motor.Velocity;
                Vector3 planarVelocity = new Vector3(velocity.x, 0, velocity.z);
                _MoveAnim.State.Parameter = planarVelocity.magnitude / _MoveSpeed;

                if (inputMag == 0) {
                    StateMachine.TrySetState(Instance._IdleState);
                }
            }
            
            /************************************************************************************************************************/

            private void Initialize() {
                _DefaultMoveSpeed = _MoveSpeed;

                _IsInitialized = true;
            }
            
        }
        
    }
}