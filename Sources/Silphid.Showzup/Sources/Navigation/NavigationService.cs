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
            // Deselect old game object
            change.Item1?
                .GetComponents<ISelectable>()
                .ForEach(x => x.IsSelected.Value = false);
                
            // Select new game object
            change.Item2?
                .GetComponents<ISelectable>()
                .ForEach(x => x.IsSelected.Value = true);
            
            // Update IsSelfOrDescendantSelected on self and ancestors
            UpdateSelfAndAncestors(
                SelectionAndAncestors,
                change,
                (obj, value) => obj?
                    .GetComponents<ISelectable>()
                    .ForEach(x => x.IsSelfOrDescendantSelected.Value = value));

            // Ensure focus follows selection (should typically be the other way around,
            // but might happen if selection is set outside of focus for some reason) 
            Focus.Value = change.Item2;
        }
        
        private void OnFocusChanged(Tuple<GameObject, GameObject> change)
        {
            // Defocus old game object
            change.Item1?
                .GetComponents<IFocusable>()
                .ForEach(x => x.IsFocused.Value = false);
                
            // Focus new game object
            change.Item2?
                .GetComponents<IFocusable>()
                .ForEach(x => x.IsFocused.Value = true);
            
            // Update IsSelfOrDescendantFocused on self and ancestors
            UpdateSelfAndAncestors(
                FocusAndAncestors,
                change,
                (obj, value) => obj?
                    .GetComponents<IFocusable>()
                    .ForEach(x => x.IsSelfOrDescendantFocused.Value = value));
        }

        /// <summary>
        /// Used to update either ISelectable.IsSelfOrDescendantSelected or IFocusable.IsSelfOrDescendantFocused.
        /// </summary>
        /// <param name="selfAndAncestorsProperty">Either SelectionAndAncestors or FocusAndAncestors reactive property</param>
        /// <param name="change">A tuple containing old and new items</param>
        /// <param name="update">The action to call for each old and new item to update its value.</param>
        private void UpdateSelfAndAncestors(ReactiveProperty<GameObject[]> selfAndAncestorsProperty, Tuple<GameObject, GameObject> change, Action<GameObject, bool> update)
        {
            // Determine lists of old and new items
            var oldItems = selfAndAncestorsProperty.Value;
            var newItems = change.Item2?.SelfAndAncestors().Reverse().ToArray() ?? new GameObject[]{};

            // Goes through old and new lists in parallel, finds where old and new items
            // diverge into two branches, and updates old items to false and new items to true
            // (but only in their respective diverging branches, to improve performance).
            var isDiverged = false; 
            for (int i = 0; i < oldItems.Length || i < newItems.Length; i++)
            {
                // Get old and new items at current position 
                var oldItem = i < oldItems.Length ? oldItems[i] : null;
                var newItem = i < newItems.Length ? newItems[i] : null;

                // Detect if branches have diverged
                if (!isDiverged && oldItem == null || newItem == null || oldItem != newItem)
                    isDiverged = true;

                // Only update items passed diverging point
                if (isDiverged)
                {
                    if (oldItem != null)
                        update(oldItem, false);

                    if (newItem != null)
                        update(newItem, true);
                }
            }
            
            // Update reactive property with complete list of new items (not just diverging branch)
            selfAndAncestorsProperty.Value = newItems;
        }

        #endregion
    }
}