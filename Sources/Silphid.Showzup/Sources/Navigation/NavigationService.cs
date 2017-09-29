using System;
using System.Linq;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup.Navigation
{
    public class NavigationService : INavigationService, IDisposable
    {
        #region Fields
        
        // TODO: To be replaced with Log4net
        private const bool IsLogEnabled = false;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        #endregion
        
        #region Constructor

        public NavigationService(EventSystem eventSystem)
        {
            if (Instance != null)
                throw new InvalidOperationException("NavigationService should only be instantiated once by InputModule.");
            Instance = this;
            
            // Ensure to synch Selection property with actual selected object
            // just in case the NavigationService was by-passed for changing
            // selection (should not happen if we refactor everything properly).
            Observable
                .EveryUpdate()
                .Select(_ => eventSystem.currentSelectedGameObject)
                .DistinctUntilChanged()
                .BindTo(Selection)
                .AddTo(_disposables);
                
            Selection
                .PairWithPrevious()
                .Subscribe(OnSelectionChanged);
                
            Focus
                .PairWithPrevious()
                .Subscribe(OnFocusChanged);

            if (IsLogEnabled)
            {
                SelectionAndAncestors.Subscribe(x =>
                    Debug.Log($"Selection: {x.ToDelimitedString(" > ")}"));

                FocusAndAncestors.Subscribe(x =>
                    Debug.Log($"Focus: {x.ToDelimitedString(" > ")}"));
            }
        }
        
        #endregion

        #region IDisposable

        public void Dispose()
        {
            Selection.Dispose();
            Focus.Dispose();
            SelectionAndAncestors.Dispose();
            FocusAndAncestors.Dispose();
            _disposables.Dispose();
        }
        
        #endregion
        
        #region INavigationService

        public ReactiveProperty<GameObject> Selection { get; } = new ReactiveProperty<GameObject>();
        public ReactiveProperty<GameObject> Focus { get; } = new ReactiveProperty<GameObject>();
        public ReactiveProperty<GameObject[]> SelectionAndAncestors { get; } = new ReactiveProperty<GameObject[]>(new GameObject[]{});
        public ReactiveProperty<GameObject[]> FocusAndAncestors { get; } = new ReactiveProperty<GameObject[]>(new GameObject[]{});

        #endregion

        #region Public

        public static INavigationService Instance { get; private set; }

        public void ExecuteBubbling<T>(GameObject target, BaseEventData eventData, ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
        {
            var current = target;
            while (current != null)
            {
                var oldSelectedObject = eventData.selectedObject;
                ExecuteEvents.Execute(current, eventData, functor);
                if (eventData.used || eventData.selectedObject != oldSelectedObject)
                    return;

                current = current.transform.parent?.gameObject;
            }
        }

        #endregion

        #region Private

        private void OnSelectionChanged(Tuple<GameObject, GameObject> change)
        {
            change.Item1?
                .GetComponents<ISelectable>()
                .ForEach(x => x.IsSelected.Value = false);
                
            change.Item2?
                .GetComponents<ISelectable>()
                .ForEach(x => x.IsSelected.Value = true);
            
            OnChanged(
                SelectionAndAncestors,
                change,
                (obj, value) => obj?
                    .GetComponents<ISelectable>()
                    .ForEach(x => x.IsSelfOrDescendantSelected.Value = value));
        }
        
        private void OnFocusChanged(Tuple<GameObject, GameObject> change)
        {
            change.Item1?
                .GetComponents<IFocusable>()
                .ForEach(x => x.IsFocused.Value = false);
                
            change.Item2?
                .GetComponents<IFocusable>()
                .ForEach(x => x.IsFocused.Value = true);
            
            OnChanged(
                FocusAndAncestors,
                change,
                (obj, value) => obj?
                    .GetComponents<IFocusable>()
                    .ForEach(x => x.IsSelfOrDescendantFocused.Value = value));
        }

        private void OnChanged(ReactiveProperty<GameObject[]> property, Tuple<GameObject, GameObject> change, Action<GameObject, bool> update)
        {
            var oldAncestors = property.Value;
            var newAncestors = change.Item2?.SelfAndAncestors().Reverse().ToArray() ?? new GameObject[]{};

            var isCommonTrunk = true; 
            for (int i = 0; i < oldAncestors.Length || i < newAncestors.Length; i++)
            {
                var oldAncestor = i < oldAncestors.Length ? oldAncestors[i] : null;
                var newAncestor = i < newAncestors.Length ? newAncestors[i] : null;
                
                if (isCommonTrunk && oldAncestor == null || newAncestor == null || oldAncestor != newAncestor)
                    isCommonTrunk = false;

                if (!isCommonTrunk)
                {
                    if (oldAncestor != null)
                        update(oldAncestor, false);

                    if (newAncestor != null)
                        update(newAncestor, true);
                }
            }
            
            property.Value = newAncestors;
        }

        #endregion
    }
}