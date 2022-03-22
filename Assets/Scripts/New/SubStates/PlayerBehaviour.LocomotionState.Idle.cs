﻿using System;
using UnityEngine;

namespace New {
    partial class PlayerBehaviour {
        
        [Serializable]
        public class IdleState : GroundedState {
            
            [SerializeField] private float _FadeSpeed = 0.25f;
            [SerializeField] public AnimationClip _IdleClip;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();

                if (Instance.MoveInput.magnitude == 0) {
                    Instance._FreezeVelocityThisTick = true;
                }
                
                Instance.Animancer.Play(_IdleClip, _FadeSpeed);
            }

            public override void Update() {
                base.Update();

                if (Instance.MoveInput.magnitude > 0f) {
                    // Change to MoveState
                    StateMachine.TrySetState(Instance._MoveState);
                }
            }

            /************************************************************************************************************************/
            
        }
        
    }
}