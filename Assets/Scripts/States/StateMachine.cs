﻿using System;
using System.Collections.Generic;

namespace StateGeneratorStates.Scripts.States
{
    public class StateMachine
    {
        private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type,List<Transition>>();
        private List<Transition> _currentTransitions = new List<Transition>();
        private List<Transition> _anyTransitions = new List<Transition>();
   
        private static readonly List<Transition> EmptyTransitions = new List<Transition>(0);

        public StateGeneratorStates.Scripts.States.IState CurrentState { get; private set; }

        public void Tick()
        {
            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);
      
            CurrentState?.OnTick();
        }

        public void SetState(StateGeneratorStates.Scripts.States.IState state)
        {
            if (state == CurrentState)
                return;
      
            CurrentState?.OnExit();
            CurrentState = state;
      
            _transitions.TryGetValue(CurrentState.GetType(), out _currentTransitions);
            if (_currentTransitions == null)
                _currentTransitions = EmptyTransitions;
      
            CurrentState.OnEnter();
        }

        public void AddTransition(StateGeneratorStates.Scripts.States.IState from, StateGeneratorStates.Scripts.States.IState to, Func<bool> predicate)
        {
            if (_transitions.TryGetValue(from.GetType(), out var transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;
            }
      
            transitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(StateGeneratorStates.Scripts.States.IState state, Func<bool> predicate)
        {
            _anyTransitions.Add(new Transition(state, predicate));
        }

        private class Transition
        {
            public Func<bool> Condition {get; }
            public StateGeneratorStates.Scripts.States.IState To { get; }

            public Transition(StateGeneratorStates.Scripts.States.IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        private Transition GetTransition()
        {
            foreach(var transition in _anyTransitions)
                if (transition.Condition())
                    return transition;
      
            foreach (var transition in _currentTransitions)
                if (transition.Condition())
                    return transition;

            return null;
        }
    }
}