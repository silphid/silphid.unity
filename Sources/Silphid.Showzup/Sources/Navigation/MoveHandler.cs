using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup.Navigation
{
    public class MoveHandler : IMoveHandler
    {
        private class Binding
        {
            public GameObject Source { get; }
            public GameObject Target { get; }
            public MoveDirection Direction { get; }
            public bool IsBidirectional { get; }
            public Func<bool> Condition { get; }
            public Func<bool> BackwardCondition { get; }

            public Binding(GameObject source, GameObject target, MoveDirection direction, bool isBidirectional, Func<bool> condition, Func<bool> backwardCondition = null)
            {
                Source = source;
                Target = target;
                Direction = direction;
                IsBidirectional = isBidirectional;
                Condition = condition;
                BackwardCondition = condition;
            }
        }
        
        private readonly List<Binding> _bindings = new List<Binding>();
        private readonly HashSet<GameObject> _gameObjects = new HashSet<GameObject>();
        
        private readonly ReactiveProperty<GameObject> _selectedGameObject = new ReactiveProperty<GameObject>();

        public ReadOnlyReactiveProperty<GameObject> SelectedGameObject =>
            _selectedGameObject.ToReadOnlyReactiveProperty();

        public void BindUnidirectional(GameObject source, GameObject target, MoveDirection direction,
            Func<bool> condition = null)
        {
            AddBinding(source, target, direction, false, condition);
        }

        public void BindBidirectional(GameObject source, GameObject target, MoveDirection direction,
            Func<bool> condition = null, Func<bool> backwardCondition = null)
        {
            AddBinding(source, target, direction, true, condition, backwardCondition ?? condition);
        }

        public void BindUnidirectional(Component source, Component target, MoveDirection direction,
            Func<bool> condition = null)
        {
            BindUnidirectional(source.gameObject, target.gameObject, direction, condition);
        }

        public void BindBidirectional(Component source, Component target, MoveDirection direction,
            Func<bool> condition = null, Func<bool> backwardCondition = null)
        {
            BindBidirectional(source.gameObject, target.gameObject, direction, condition, backwardCondition);
        }

        private void AddBinding(GameObject source, GameObject target, MoveDirection direction, bool isBidirectional,
            Func<bool> condition, Func<bool> backwardCondition = null)
        {
            _bindings.Add(new Binding(source, target, direction, isBidirectional, condition, backwardCondition));
            _gameObjects.Add(source);
            _gameObjects.Add(target);
        }

        public void OnMove(AxisEventData eventData)
        {
            var selected = _gameObjects.FirstOrDefault(x => x.IsSelfOrDescendantSelected());
            if (selected == null)
                return;

            var target = GetForwardTarget(selected, eventData.moveDir) ??
                         GetBackwardTarget(selected, eventData.moveDir.Opposite());

            if (target != null)
            {
                target.Select();
                _selectedGameObject.Value = target;
                eventData.Use();
            }
        }

        private GameObject GetForwardTarget(GameObject selected, MoveDirection direction) =>
            _bindings
                .FirstOrDefault(x =>
                    x.Direction == direction &&
                    x.Source == selected &&
                    x.Target.activeInHierarchy &&
                    (x.Condition?.Invoke() ?? true))
                ?.Target;

        private GameObject GetBackwardTarget(GameObject selected, MoveDirection oppositeDirection) =>
            _bindings
                .FirstOrDefault(x =>
                    x.IsBidirectional &&
                    x.Direction == oppositeDirection &&
                    x.Target == selected &&
                    x.Source.activeInHierarchy &&
                    (x.BackwardCondition?.Invoke() ?? true))
                ?.Source;
    }
}