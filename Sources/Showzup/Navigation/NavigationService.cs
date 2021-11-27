using System;
using System.Linq;
using log4net;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup.Navigation
{
    public class NavigationService : INavigationService, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NavigationService));

        #region Fields

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly EventSystem _eventSystem;
        private readonly bool _isSelectionAllowed;

        #endregion

        #region Constructor

        public NavigationService(EventSystem eventSystem)
        {
            if (Instance != null)
            {
                Log.Error(
                    "NavigationService has already been instantiated. Be sure to dispose it if reinstantiation is needed");
                return;
            }

            _eventSystem = eventSystem;

            Instance = this;

            Disposable.Create(() => Instance = null)
                      .AddTo(_disposables);

            Selection = _selection.ToSequentialReadOnlyReactiveProperty()
                                  .AddTo(_disposables);

            SelectionAndAncestors = new ReactiveProperty<GameObject[]>(new GameObject[] {}).AddTo(_disposables);

            // Ensure to synch Selection property with actual selected object
            // just in case the NavigationService was by-passed for changing
            // selection (should not happen if we refactor everything properly).
            Observable.EveryUpdate()
                      .Select(_ => eventSystem.currentSelectedGameObject)
                      .DistinctUntilChanged()
                      .Subscribe(x => SetSelection(x))
                      .AddTo(_disposables);

            Selection.PairWithPrevious()
                     .Subscribe(OnSelectionChanged)
                     .AddTo(_disposables);

            if (Log.IsDebugEnabled)
                SelectionAndAncestors.Subscribe(x => Log.Debug($"Selection: {x.JoinAsString(" > ")}"))
                                     .AddTo(_disposables);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _disposables.Dispose();
        }

        #endregion

        #region INavigationService

        public void SetSelection(GameObject gameObject, bool forceNotify = false)
        {
            if (!forceNotify)
                _selection.Value = GetSelectable(gameObject);
            else
                _selection.SetValueAndForceNotify(GetSelectable(gameObject));
        }

        private GameObject GetSelectable(GameObject gameObject)
        {
            if (gameObject == null)
                return null;

            var selectableContainer = gameObject.GetComponent<ISelectableContainer>();
            if (selectableContainer == null)
                return gameObject;

            var selectableContent = selectableContainer.SelectableContent;
            if (selectableContent == null || selectableContent == gameObject)
                return gameObject;

            return GetSelectable(selectableContent);
        }

        private readonly ReactiveProperty<GameObject> _selection = new ReactiveProperty<GameObject>();

        public IReadOnlyReactiveProperty<GameObject> Selection { get; }

        public IReactiveProperty<GameObject[]> SelectionAndAncestors { get; }

        #endregion

        #region Public

        public static INavigationService Instance { get; private set; }

        public void ExecuteBubbling<T>(GameObject target,
                                       BaseEventData eventData,
                                       ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
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

        private bool _isSelecting;

        private void OnSelectionChanged(Tuple<GameObject, GameObject> change)
        {
            if (_isSelecting)
            {
                string item1Name = "null";
                string item2Name = "null";

                if (change.Item1 != null)
                    item1Name = change.Item1.name;
                if (change.Item2 != null)
                    item2Name = change.Item2.name;

                throw new InvalidOperationException(
                    $"Cannot change selection from {item1Name} to {item2Name} within a IsSelected subscription. Use ISelectableContainer instead.");
            }

            _isSelecting = true;

            // Defocus old game object
            if (change.Item1 != null)
            {
                change.Item1.GetComponents<ISelectable>()
                      .ForEach(x => x.IsSelected.Value = false);
            }

            // Focus new game object
            if (change.Item2 != null)
            {
                change.Item2.GetComponents<ISelectable>()
                      .ForEach(x => x.IsSelected.Value = true);
            }

            // Update IsSelfOrDescendantFocused on self and ancestors
            UpdateSelfAndAncestors(change.Item2);

            _eventSystem.SetSelectedGameObject(change.Item2);

            _isSelecting = false;
        }

        /// <summary>
        /// Used to update ISelectable.IsSelfOrDescendantSelected
        /// </summary>
        /// <param name="newSelectedGameObject">Newly selected or focused item</param>
        private void UpdateSelfAndAncestors(GameObject newSelectedGameObject)
        {
            // Determine lists of old and new items
            var oldItems = SelectionAndAncestors.Value;
            var newItems = newSelectedGameObject?.SelfAndAncestors()
                                                 .Reverse()
                                                 .ToArray() ?? new GameObject[] {};

            // Goes through old and new lists in parallel, finds where old and new items
            // diverge into two branches, and updates old items to false and new items to true
            // (but only in their respective diverging branches, to improve performance).
            var isDiverged = false;
            for (var i = 0; i < oldItems.Length || i < newItems.Length; i++)
            {
                // Get old and new items at current position 
                var oldItem = i < oldItems.Length
                                  ? oldItems[i]
                                  : null;
                var newItem = i < newItems.Length
                                  ? newItems[i]
                                  : null;

                // Detect if branches have diverged
                if (!isDiverged && oldItem == null || newItem == null || oldItem != newItem)
                    isDiverged = true;

                // Only update items passed diverging point
                if (!isDiverged)
                    continue;

                if (oldItem != null)
                    oldItem.GetComponents<ISelectable>()
                           .ForEach(x => x.IsSelfOrDescendantSelected.Value = false);

                if (newItem != null)
                    newItem.GetComponents<ISelectable>()
                           .ForEach(x => x.IsSelfOrDescendantSelected.Value = true);
            }

            // Update reactive property with complete list of new items (not just diverging branch)
            SelectionAndAncestors.Value = newItems;
        }

        #endregion
    }
}