using System;
using UnityEngine;

namespace EC.Core {
    public class PlayerStateMachine {
        public PlayerState CurrentState { get; private set; }

        public Action<PlayerState, PlayerState> OnStateChange;

        public void Initialize(PlayerState startingState) {
            ChangeState(startingState);
        }

        public void ChangeState(PlayerState newState) {
            CurrentState?.Exit();
            OnStateChange?.Invoke(CurrentState, newState);
            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}