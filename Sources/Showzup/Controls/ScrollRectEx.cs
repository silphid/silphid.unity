using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;

namespace Silphid.Showzup
{
    public class ScrollRectEx : ScrollRect
    {
        private bool _routeToParent;
        private readonly FieldInfo _prevPositionField;

        protected ScrollRectEx()
        {
            var type = typeof(ScrollRect);
            _prevPositionField = type.GetField("m_PrevPosition", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Do action for all parents
        /// </summary>
        private void DoForParents<T>(Action<T> action) where T : IEventSystemHandler
        {
            var parent = transform.parent;
            while (parent != null)
            {
                foreach (var component in parent.GetComponents<Component>()
                                                .OfType<T>())
                    action(component);

                parent = parent.parent;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Always route initialize potential drag event to parents
        /// </summary>
        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            DoForParents<IInitializePotentialDragHandler>(parent => parent.OnInitializePotentialDrag(eventData));
            base.OnInitializePotentialDrag(eventData);
        }

        /// <inheritdoc />
        /// <summary>
        /// Drag event
        /// </summary>
        public override void OnDrag(PointerEventData eventData)
        {
            if (_routeToParent)
                DoForParents<IDragHandler>(parent => parent.OnDrag(eventData));
            else
                base.OnDrag(eventData);
        }

        /// <inheritdoc />
        /// <summary>
        /// Begin drag event
        /// </summary>
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
                _routeToParent = true;
            else if (!vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
                _routeToParent = true;
            else
                _routeToParent = false;

            if (_routeToParent)
                DoForParents<IBeginDragHandler>(parent => parent.OnBeginDrag(eventData));
            else
                base.OnBeginDrag(eventData);
        }

        /// <inheritdoc />
        /// <summary>
        /// End drag event
        /// </summary>
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_routeToParent)
                DoForParents<IEndDragHandler>(parent => parent.OnEndDrag(eventData));
            else
                base.OnEndDrag(eventData);

            _routeToParent = false;
        }

        /// <summary>
        /// Adjust scrolling by given offset, even if being dragged
        /// </summary>
        public void Adjust(Vector2 offset)
        {
            SetContentAnchoredPosition(content.anchoredPosition + offset);

            m_ContentStartPosition += offset;

            // Update private m_PrevPosition
            var prevPosition = (Vector2) _prevPositionField.GetValue(this);
            _prevPositionField.SetValue(this, prevPosition + offset);

            UpdateBounds();
        }
    }
}