using System;
using Animancer;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {
    
        [SerializeField] private LandState _LandState;

        [Serializable]
        public class LandState : LocomotionState {
            
            /************************************************************************************************************************/
            
            [SerializeField] private LinearMixerTransition _LandAnim;
            [SerializeField] private float _EntryFadeSpeed;
            [SerializeField] private float _ExitFadeSpeed;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                Instance.Animancer.Play(_LandAnim, _EntryFadeSpeed);
                _LandAnim.State.Parameter = Instance.MoveInput.magnitude;
                _LandAnim.State.Events.OnEnd = () => {
                    Instance.LocomotionStateMachine.TrySetState(Instance._IdleState);
                };
            }

            public override void Update() {
                base.Update();
                _LandAnim.State.Parameter = Mathf.MoveTowards(
                    _LandAnim.State.Parameter,
                    Instance.MoveInput.magnitude,
                    _ExitFadeSpeed
                );
            }
        }
        
    }
}