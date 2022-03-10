using UnityEngine;

namespace EC.Core {
    public class ActorState {
        protected KCController Controller;
        protected ActorStateMachine stateMachine;
        protected bool isExitingState;
        protected float startTime;

        public ActorState(KCController Controller, ActorStateMachine stateMachine) {
            this.stateMachine = stateMachine;
            this.Controller = Controller;
        }

        public virtual void Enter() {
            DoChecks();
            startTime = Time.time;
            isExitingState = false;
        }

        public virtual void Exit() {
            isExitingState = true;
        }

        public virtual void LogicUpdate() {
            
        }

        public virtual void PhysicsUpdate() {
            DoChecks();
        }

        public virtual void DoChecks() {} 
        
    }
}