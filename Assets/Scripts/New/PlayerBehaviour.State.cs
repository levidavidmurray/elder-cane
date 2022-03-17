
using System;
using Animancer;
using UnityEngine;
using Animancer.FSM;

namespace New {
    partial class PlayerBehaviour {

        [Serializable]
        public abstract class State : Animancer.FSM.State {

            /************************************************************************************************************************/

            protected AnimancerComponent _Animancer => Instance.Animancer;
            
            /************************************************************************************************************************/

            public virtual void Update() { }
            
            public virtual void FixedUpdate() { }
            
            /************************************************************************************************************************/

            protected void log(object message) {
                var clsName = this.GetType().Name;
                print($"[{clsName}]: {message}");
            }

            /************************************************************************************************************************/
            
            public static implicit operator bool(State state) {
                return state != null;
            }
            
        }
        
    }
}