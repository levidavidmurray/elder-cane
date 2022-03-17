using System;
using Animancer;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {

        [SerializeField] private MoveState _MoveState;
        
        [Serializable]
        public class MoveState : LocomotionState {
            
            [SerializeField] private LinearMixerTransition _MoveAnim;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                Instance.Animancer.Play(_MoveAnim);
            }

            public override void Update() {
                base.Update();

                Vector3 velocity = Motor.Velocity;
                Vector3 planarVelocity = new Vector3(velocity.x, 0, velocity.z);
                _MoveAnim.State.Parameter = planarVelocity.magnitude / _MoveSpeed;

                if (Instance.MoveInput.magnitude == 0) {
                    Instance.LocomotionStateMachine.TrySetState(Instance._IdleState);
                }
            }
            
            /************************************************************************************************************************/
            
        }
        
    }
}