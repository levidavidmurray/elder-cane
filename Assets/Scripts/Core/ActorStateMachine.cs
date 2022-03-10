namespace EC.Core {
    public class ActorStateMachine {
        public ActorState CurrentState { get; private set; }

        public void Initialize(ActorState startingState) {
            CurrentState = startingState;
        }

        public void ChangeState(ActorState newState) {
            CurrentState = newState;
        }
    }
}