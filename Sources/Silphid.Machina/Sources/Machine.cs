using System;
using System.Collections.Generic;
using Silphid.Extensions;
using Silphid.Requests;
using UniRx;
using UnityEngine;

namespace Silphid.Machina
{
    public class Machine : IMachine, IDisposable
    {
        private readonly object _initialState;
        private readonly bool _disposeOnCompleted;
        private bool _isDisposed;
        private readonly List<Rule> _rules = new List<Rule>();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly ReactiveProperty<object> _state;
        public ReadOnlyReactiveProperty<object> State { get; }
        public IObservable<Transition> Transitions { get; }

        protected virtual string MachineName => GetType().Name;

        public Machine(object initialState = null, bool disposeOnCompleted = false)
        {
            _initialState = initialState;
            _disposeOnCompleted = disposeOnCompleted;
            _state = new ReactiveProperty<object>(initialState);
            State = _state.ToReadOnlyReactiveProperty();
            Transitions = _state.PairWithPrevious().Select(x => new Transition(x.Item1, x.Item2));

            this.Entering<IMachine>()
                .Subscribe(x => x.Start())
                .AddTo(_disposables);

            this.Exiting<IMachine>()
                .Subscribe(x => x.Complete())
                .AddTo(_disposables);
        }

        public void Start(object initialState = null)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Machine));
            
            OnStarting(initialState);
        }

        public void Enter(object state)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Machine));
            
            OnEnter(state);
        }

        public void Complete()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Machine));
            
            OnCompleted();
        }

        public IRequest Handle(IRequest request)
        {
            if (State.Value == null)
                return request;

            var state = State.Value;
            Debug.Log($"{MachineName}.{state} trying to handle {request}");

            request = HandleWithState(request, state);
            if (request == null)
                return null;

            return HandleWithRules(request, state);
        }

        private IRequest HandleWithState(IRequest request, object state)
        {
            var handler = state as IRequestHandler;
            if (handler == null)
                return request;
            
            var outRequest = handler.Handle(request);
            if (outRequest == null)
                Debug.Log($"IRequestHandler implementation of {MachineName}.{state} handled {request}");
            else if (outRequest != request)
                Debug.Log($"IRequestHandler implementation of {MachineName}.{state} handled {request} and returned {outRequest}");

            return outRequest;
        }

        private IRequest HandleWithRules(IRequest request, object state)
        {
            foreach (var rule in _rules)
            {
                if (rule.MatchesState(state))
                {
                    var outRequest = rule.Handle(request);
                    
                    if (outRequest == null)
                    {
                        Debug.Log($"Rule for {MachineName}.{state} handled {request}");
                        return null;
                    }

                    if (outRequest != request)
                    {
                        Debug.Log($"Rule for {MachineName}.{state} handled {request} and returned {outRequest}");
                        return Handle(outRequest);
                    }
                }
            }

            return request;
        }

        public IRule When<TState>()
        {
            var rule = new Rule(x => x is TState);
            _rules.Add(rule);
            return rule;
        }

        public IRule When<TState>(TState state)
        {
            var rule = new Rule(x => Equals(x, state));
            _rules.Add(rule);
            return rule;
        }

        public IRule When<TState>(Predicate<TState> predicate)
        {
            var rule = new Rule(x => x is TState && predicate((TState) x));
            _rules.Add(rule);
            return rule;
        }

        protected virtual void OnStarting(object initialState)
        {
            _state.Value = initialState ?? _initialState;
        }

        protected virtual void OnEnter(object state)
        {
            _state.Value = state;
        }

        protected virtual void OnCompleted()
        {
            _state.Value = null;
            
            if (_disposeOnCompleted)
                Dispose();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            
            _disposables.Dispose();
            _state.Dispose();
            State.Dispose();
            _isDisposed = true;
        }
    }
}