using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using Silphid.Showzup.Navigation;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class Control : MonoBehaviour, ISelectable
    {
        public IReactiveProperty<bool> IsSelected { get; } = new ReactiveProperty<bool>();
        public IReactiveProperty<bool> IsSelfOrDescendantSelected { get; } = new ReactiveProperty<bool>();

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
        }

        protected virtual void InsertView(GameObject container, int index, IView view)
        {
            if (view == null)
                return;

            SetViewParent(container, view.GameObject);
            view.GameObject.transform.SetSiblingIndex(index);
        }
    }
}