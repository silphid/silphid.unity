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
        private const bool IsLogEnabled = true;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly EventSystem _eventSystem;

        #endregion

        #region Constructor

        public NavigationService(EventSystem eventSystem)
        {
            _eventSystem = eventSystem;
            if (Instance != null)
                throw new InvalidOperationException(
                    "NavigationService should only be instantiated once by InputModule.");
            Instance = this;

            // Ensure to synch Selection property with actual selected object
            // just in case the NavigationService was by-passed for changing
            // selection (should not happen if we refactor everything properly).
            Observable
                .EveryUpdate()
                .Select(_ => eventSystem.currentSelectedGameObject)
                .DistinctUntilChanged()
                .Subscribe(SetSelection)
                .AddTo(_disposables);

            Selection
                .PairWithPrevious()
                .Subscribe(OnSelectionChanged);

            if (IsLogEnabled)
            {
                SelectionAndAncestors.Subscribe(x =>
                    Debug.Log($"Selection: {x.ToDelimitedString(" > ")}"));
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Selection.Dispose();
            SelectionAndAncestors.Dispose();
            _disposables.Dispose();
        }

        #endregion

        #region INavigationService

        public void SetSelection(GameObject gameObject)
        {
            _selection.Value = ForwardSelection(gameObject);
        }

        private GameObject ForwardSelection(GameObject gameObject)
        {
            if (gameObject == null)
                return null;
            
            var redirectFocus = gameObject.GetComponent<IForwardSelectable>()?.ForwardSelection();

            if (redirectFocus == gameObject)
                throw new InvalidOperationException($"{gameObject.name} is forwarding selection to the same gameObject");

            return redirectFocus ? ForwardSelection(redirectFocus) : gameObject;
        }

        private readonly ReactiveProperty<GameObject> _selection = new ReactiveProperty<GameObject>();
        
        public ReadOnlyReactiveProperty<GameObject> Selection => _selection.ToReadOnlyReactiveProperty();

        public ReactiveProperty<GameObject[]> SelectionAndAncestors { get; } =
            new ReactiveProperty<GameObject[]>(new GameObject[] { });

        #endregion

        #region Public

        public static INavigationService Instance { get; private set; }

        public void ExecuteBubbling<T>(GameObject target, BaseEventData eventData,
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
                throw new InvalidOperationException(
                    $"Cannot change selection from {change.Item1.name} to {change.Item2.name} within a IsSelected subscription. Use IForwardSelection instead.");

            _isSelecting = true;

            // Defocus old game object
            change.Item1?
                .GetComponents<ISelectable>()
                .ForEach(x => x.IsSelected.Value = false);

            // Focus new game object
            change.Item2?
                .GetComponents<ISelectable>()
                .ForEach(x => x.IsSelected.Value = true);

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
            var newItems = newSelectedGameObject?.SelfAndAncestors().Reverse().ToArray() ?? new GameObject[] { };

            // Goes through old and new lists in parallel, finds where old and new items
            // diverge into two branches, and updates old items to false and new items to true
            // (but only in their respective diverging branches, to improve performance).
            var isDiverged = false;
            for (var i = 0; i < oldItems.Length || i < newItems.Length; i++)
            {
                // Get old and new items at current position 
                var oldItem = i < oldItems.Length ? oldItems[i] : null;
                var newItem = i < newItems.Length ? newItems[i] : null;

                // Detect if branches have diverged
                if (!isDiverged && oldItem == null || newItem == null || oldItem != newItem)
                    isDiverged = true;

                // Only update items passed diverging point
                if (!isDiverged)
                    continue;

                if (oldItem != null)
                    oldItem
                        .GetComponents<ISelectable>()
                        .ForEach(x => x.IsSelfOrDescendantSelected.Value = false);

                if (newItem != null)
                    newItem
                        .GetComponents<ISelectable>()
                        .ForEach(x => x.IsSelfOrDescendantSelected.Value = true);
            }

            // Update reactive property with complete list of new items (not just diverging branch)
            SelectionAndAncestors.Value = newItems;
        }

        #endregion
    }
}