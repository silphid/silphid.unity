using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup
{
    public abstract class Control : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        protected readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>();

        protected virtual void RemoveAllViews(GameObject container, GameObject except = null)
        {
            if (container)
                container.Children().Where(x => x != except).ForEach(RemoveView);
        }

        protected void RemoveViews(GameObject viewObject, IEnumerable<IView> views)
        {
            foreach (var view in views)
                RemoveView(view?.GameObject); //FIXME [jsricard] views contains sometime a liste of null
        }

        protected virtual void RemoveView(GameObject viewObject)
        {
            if (viewObject != null)
                Destroy(viewObject);
        }

        protected virtual void SetViewParent(GameObject container, GameObject viewObject)
        {
            viewObject.transform.SetParent(container.transform, false);
        }

        protected virtual void ReplaceView(GameObject container, IView view)
        {
            AddView(container, view);
            RemoveAllViews(container, view?.GameObject);
        }

        protected virtual void AddView(GameObject container, IView view)
        {
            if (view == null)
                return;

            SetViewParent(container, view.GameObject);
            view.IsActive = true;
        }

        protected virtual void InsertView(GameObject container, int index, IView view)
        {
            if (view == null)
                return;

            SetViewParent(container, view.GameObject);
            view.GameObject.transform.SetSiblingIndex(index);
            view.IsActive = true;
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            IsSelected.Value = true;
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            IsSelected.Value = false;
        }
    }
}