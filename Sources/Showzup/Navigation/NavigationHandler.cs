using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup.Navigation
{
    public class NavigationHandler : IMoveHandler, ICancelHandler
    {
        private class MoveBinding
        {
            public GameObject Source { get; }
            public GameObject Target { get; }
            public MoveDirection Direction { get; }
            public bool IsBidirectional { get; }
            public Func<bool> Condition { get; }
            public Func<bool> BackwardCondition { get; }
            public Action<MoveDirection> OnHandledAction { get; }

            public MoveBinding(GameObject source,
                               GameObject target,
                               MoveDirection direction,
                               bool isBidirectional,
                               Func<bool> condition,
                               Func<bool> backwardCondition = null,
                               Action<MoveDirection> onHandledAction = null)
            {
                Source = source;
                Target = target;
                Direction = direction;
                IsBidirectional = isBidirectional;
                Condition = condition;
                BackwardCondition = backwardCondition;
                OnHandledAction = onHandledAction;
            }
        }

        private class CancelBinding
        {
            public GameObject Source { get; }
            public GameObject Target { get; }
            public Func<bool> Condition { get; }
            public Action OnHandledAction { get; }

            public CancelBinding(GameObject source,
                                 GameObject target,
                                 Func<bool> condition,
                                 Action onHandledAction = null)
            {
                Source = source;
                Target = target;
                Condition = condition;
                OnHandledAction = onHandledAction;
            }
        }

        private readonly List<MoveBinding> _moveBindings = new List<MoveBinding>();
        private readonly List<CancelBinding> _cancelBindings = new List<CancelBinding>();
        private readonly HashSet<GameObject> _gameObjects = new HashSet<GameObject>();

        public void BindUnidirectional(GameObject source,
                                       GameObject target,
                                       MoveDirection direction,
                                       Func<bool> condition = null,
                                       Action<MoveDirection> onHandledAction = null)
        {
            AddBinding(source, target, direction, false, condition, null, onHandledAction);
        }

        public void BindBidirectional(GameObject source,
                                      GameObject target,
                                      MoveDirection direction,
                                      Func<bool> condition = null,
                                      Func<bool> backwardCondition = null,
                                      Action<MoveDirection> onHandledAction = null)
        {
            AddBinding(source, target, direction, true, condition, backwardCondition ?? condition, onHandledAction);
        }

        public void BindUnidirectional(Component source,
                                       Component target,
                                       MoveDirection direction,
                                       Func<bool> condition = null,
                                       Action<MoveDirection> onHandledAction = null)
        {
            BindUnidirectional(source.gameObject, target.gameObject, direction, condition, onHandledAction);
        }

        public void BindBidirectional(Component source,
                                      Component target,
                                      MoveDirection direction,
                                      Func<bool> condition = null,
                                      Func<bool> backwardCondition = null,
                                      Action<MoveDirection> onHandledAction = null)
        {
            BindBidirectional(
                source.gameObject,
                target.gameObject,
                direction,
                condition,
                backwardCondition,
                onHandledAction);
        }

        public void BindCancel(GameObject source,
                               GameObject target,
                               Func<bool> condition = null,
                               Action onHandledAction = null)
        {
            _cancelBindings.Add(new CancelBinding(source, target, condition, onHandledAction));
            _gameObjects.Add(source);
        }

        public void BindCancel(Component source,
                               Component target,
                               Func<bool> condition = null,
                               Action onHandledAction = null)
        {
            BindCancel(source.gameObject, target.gameObject, condition, onHandledAction);
        }

        private void AddBinding(GameObject source,
                                GameObject target,
                                MoveDirection direction,
                                bool isBidirectional,
                                Func<bool> condition,
                                Func<bool> backwardCondition = null,
                                Action<MoveDirection> onHandledAction = null)
        {
            _moveBindings.Add(
                new MoveBinding(
                    source,
                    target,
                    direction,
                    isBidirectional,
                    condition,
                    backwardCondition,
                    onHandledAction));
            _gameObjects.Add(source);
            if (isBidirectional)
                _gameObjects.Add(target);
        }

        public void OnMove(AxisEventData eventData)
        {
            if (_gameObjects.Count == 0)
                throw new InvalidOperationException("No bindings have been set up");

            var selected = _gameObjects.FirstOrDefault(x => x.IsSelfOrDescendantSelected());
            if (selected == null)
                throw new InvalidOperationException("Game object should not receive OnMove() when not selected.");

            var target = GetForwardTarget(selected, eventData.moveDir) ??
                         GetBackwardTarget(selected, eventData.moveDir.Opposite());

            if (target != null)
            {
                target.Select();
                eventData.Use();
            }
        }

        public void OnCancel(BaseEventData eventData)
        {
            var selected = _gameObjects.FirstOrDefault(x => x.IsSelfOrDescendantSelected());
            if (selected == null)
                throw new InvalidOperationException("Game object should not receive OnCancel() when not selected.");

            var target = _cancelBindings.FirstOrDefault(
                                             x =>
                                             {
                                                 var isHandled = x.Source == selected &&
                                                                 x.Target.activeInHierarchy &&
                                                                 (x.Condition?.Invoke() ?? true);

                                                 if (isHandled)
                                                     x.OnHandledAction?.Invoke();

                                                 return isHandled;
                                             })
                                       ?.Target;

            if (target != null)
            {
                target.Select();
                eventData.Use();
            }
        }

        private GameObject GetForwardTarget(GameObject selected, MoveDirection direction) =>
            _moveBindings.FirstOrDefault(
                              x =>
                              {
                                  var isHandled = x.Direction == direction && x.Source == selected &&
                                                  x.Target.activeInHierarchy && (x.Condition?.Invoke() ?? true);

                                  if (isHandled)
                                      x.OnHandledAction?.Invoke(x.Direction);

                                  return isHandled;
                              })
                        ?.Target;

        private GameObject GetBackwardTarget(GameObject selected, MoveDirection oppositeDirection) =>
            _moveBindings.FirstOrDefault(
                              x =>
                              {
                                  var isHandled = x.IsBidirectional && x.Direction == oppositeDirection &&
                                                  x.Target == selected && x.Source.activeInHierarchy &&
                                                  (x.BackwardCondition?.Invoke() ?? true);

                                  if (isHandled)
                                      x.OnHandledAction?.Invoke(x.Direction.Opposite());

                                  return isHandled;
                              })
                        ?.Source;
    }
}