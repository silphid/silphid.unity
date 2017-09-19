using System;
using System.Collections.Generic;
using Silphid.Extensions;
using Silphid.Requests;
using UniRx;
using UnityEngine;

namespace Silphid.Machina
{
    public class Machine<TState> : IMachine<TState>, IDisposable
    {
        private readonly object _initialState;
        private readonly bool _disposeOnCompleted;
        private readonly List<Rule> _rules = new List<Rule>();
        private readonly ReactiveProperty<object> _state;
        public ReadOnlyReactiveProperty<object> State { get; }
        public IObservable<Transition> Transitions { get; }

        protected virtual string MachineName => GetType().Name;
        protected readonly CompositeDisposable Disposables = new CompositeDisposable();
        protected bool IsDisposed { get; private set; }

        public Machine(object initialState = null, bool disposeOnCompleted = false)
        {
            _initialState = initialState;
            _disposeOnCompleted = disposeOnCompleted;
            _state = new ReactiveProperty<object>(initialState);
            State = _state.ToReadOnlyReactiveProperty();
            Transitions = _state.PairWithPrevious().Select(x => new Transition(x.Item1, x.Item2));

            this.Entering<IMachine>()
                .Subscribe(x => x.Start())
                .AddTo(Disposables);

            this.Exiting<IMachine>()
                .Subscribe(x => x.Complete())
                .AddTo(Disposables);
        }

        public void Start(object initialState = null)
        {
            AssertNotDisposed();
            OnStarting(initialState);
        }

        public void Enter(TState state)
        {
            EnterInternal(state);
        }

        public void Enter(IMachine machine)
        {
            EnterInternal(machine);
        }

        private void EnterInternal(object state)
        {
            AssertNotDisposed();
            OnEnter(state);
        }

        public void Complete()
        {
            AssertNotDisposed();
            OnCompleted();
        }

        private void AssertNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Machine<TState>));
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

        public IRule Always()
        {
            var rule = new Rule(_ => true);
            _rules.Add(rule);
            return rule;
        }

        public IRule When<T>() where T : TState =>
            WhenInternal<T>();

        public IRule When<T>(T state) where T : TState =>
            WhenInternal(state);

        public IRule When<T>(Predicate<T> predicate) where T : TState =>
            WhenInternal(predicate);

        public IRule WhenMachine<TMachine>() where TMachine : IMachine =>
            WhenInternal<TMachine>();

        public IRule WhenMachine<TMachine>(TMachine state) where TMachine : IMachine =>
            WhenInternal(state);

        public IRule WhenMachine<TMachine>(Predicate<TMachine> predicate) where TMachine : IMachine =>
            WhenInternal(predicate);

        private IRule WhenInternal<T>()
        {
            var rule = new Rule(x => x is T);
            _rules.Add(rule);
            return rule;
        }

        private IRule WhenInternal<T>(T state)
        {
            var rule = new Rule(x => Equals(x, state));
            _rules.Add(rule);
            return rule;
        }

        private IRule WhenInternal<T>(Predicate<T> predicate)
        {
            var rule = new Rule(x => x is T && predicate((T) x));
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

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            
            Disposables.Dispose();
            _state.Dispose();
            State.Dispose();
            IsDisposed = true;
        }
    }
}