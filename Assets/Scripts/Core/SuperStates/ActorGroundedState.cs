namespace EC.Core.SuperStates {
    public class ActorGroundedState : ActorState {
        protected int xInput;
        protected int yInput;

        public ActorGroundedState(KCController Controller, ActorStateMachine stateMachine) : base(Controller,
            stateMachine) {
        }
        
    }
}