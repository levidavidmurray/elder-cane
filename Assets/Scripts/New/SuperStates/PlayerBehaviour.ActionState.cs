﻿using Animancer;
using Animancer.FSM;

namespace New {
    partial class PlayerBehaviour {
        
        public abstract class ActionState : State {
            
            /************************************************************************************************************************/
            
            protected StateMachine<ActionState> StateMachine;
            protected AnimancerLayer AnimLayer;
            
            /************************************************************************************************************************/

            public override void OnEnterState() {
                base.OnEnterState();
                StateMachine = Instance.ActionStateMachine;
                int layerIndex = _ActionLayer;
                AnimLayer = Instance.Animancer.Layers[layerIndex];
                
                AnimLayer.StartFade(1);
            }
            
            /************************************************************************************************************************/
            
        }
        
    }
}
