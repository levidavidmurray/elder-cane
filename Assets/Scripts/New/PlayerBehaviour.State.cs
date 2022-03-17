
using System;
using UnityEngine;
using Animancer.FSM;

namespace New {
    partial class PlayerBehaviour {

        [Serializable]
        public abstract class State : Animancer.FSM.State {

            /************************************************************************************************************************/

            public virtual void Update() { }

            /************************************************************************************************************************/

            protected void log(object message) {
                var clsName = this.GetType().Name;
                print($"[{clsName}]: {message}");
            }
            
        }
        
    }
}