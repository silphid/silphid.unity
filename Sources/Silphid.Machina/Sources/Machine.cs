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

        protected readonly CompositeDisposable Disposables = new CompositeDisposable();
        protected bool IsDisposed { get; private set; }

        public Machine(object initialState = null, bool disposeOnCompleted = false)
        {
            _initialState = initialState;
            _disposeOnCompleted = disposeOnCompleted;
            _state = new ReactiveProperty<object>(initialState);
            State = _state.ToReadOnlyReactiveProperty();
            Transitions = _state
                .DistinctUntilChanged()
                .PairWithPrevious()
                .Select(x => new Transition(x.Item1, x.Item2));

            this.Entering<IMachine>()
                .Subscribe(x => x.Start())
                .AddTo(Disposables);

            this.Exiting<IMachine>()
                .Subscribe(x => x.Complete())
                .AddTo(Disposables);
            
            Transitions
                .Subscribe(x => Debug.Log($"{Name} - {x.Source ?? "null"} -> {x.Target ?? "null"}"))
                .AddTo(Disposables);
        }

        public virtual string Name => GetType().Name;
        public override string ToString() => Name;

        public void Start(object initialState = null)
        {
            AssertNotDisposed();
            Debug.Log($"{Name} - Started");
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

        public void ExitState()
        {
            EnterInternal(null);
        }

        private void EnterInternal(object state)
        {
            AssertNotDisposed();
            OnEnter(state);
        }

        void IMachine.Complete()
        {
            AssertNotDisposed();
            OnCompleted();
            Debug.Log($"{Name} - Completed");
        }

        private void AssertNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Machine<TState>));
        }

        public IRequest Handle(IRequest request)
        {
            var state = State.Value;
            if (state != null)
            {
                request = HandleWithState(request, state);
                if (request == null)
                    return null;
            }
            
            return HandleWithRules(request, state);
        }

        private IRequest HandleWithState(IRequest request, object state)
        {
            var handler = state as IRequestHandler;
            if (handler == null)
                return request;
            
            var outRequest = handler.Handle(request);
            if (outRequest == null)
                Debug.Log($"{Name} - {state} - State handled {request}");
            else if (outRequest != request)
                Debug.Log($"{Name} - {state} - State handled {request} and returned {outRequest}");

            return outRequest;
        }

        private IRequest HandleWithRules(IRequest request, object state)
        {
            foreach (var rule in _rules)
            {
                if (rule.Matches(state, request))
                {
                    var outRequest = rule.Handle(request);
                    
                    if (outRequest == null)
                    {
                        Debug.Log($"{Name} - {state ?? "null"} - Rule handled {request}");
                        return null;
                    }

                    if (outRequest != request)
                    {
                        Debug.Log($"{Name} - {state ?? "null"} - Rule handled {request} and returned {outRequest}");
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