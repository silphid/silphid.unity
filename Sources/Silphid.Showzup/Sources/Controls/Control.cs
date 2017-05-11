using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class Control : MonoBehaviour
    {
        public GameObject HistoryContainer;

        protected virtual void RemoveAllViews(GameObject container, GameObject except = null)
        {
            if (container)
                container.Children().Where(x => x != except).ForEach(RemoveView);
        }

        private void RemoveView(GameObject viewObject)
        {
            if (HistoryContainer != null)
            {
                viewObject.SetActive(false);
                viewObject.transform.SetParent(HistoryContainer.transform, false);
            }
            else
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
    }
}