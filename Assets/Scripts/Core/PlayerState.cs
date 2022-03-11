using KinematicCharacterController;
using UnityEngine;

namespace EC.Core {
    public class PlayerState {
        
        protected KCController Controller;
        protected PlayerStateMachine stateMachine;
        protected KCControllerData controllerData;
        protected bool isExitingState;
        protected float startTime;

        public PlayerState(KCController Controller, PlayerStateMachine stateMachine, KCControllerData controllerData) {
            this.stateMachine = stateMachine;
            this.Controller = Controller;
            this.controllerData = controllerData;
        }
        
        public virtual void DoChecks() {} 

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

        public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
        }

        public virtual void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
        }

    }
}