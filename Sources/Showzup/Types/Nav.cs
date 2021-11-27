using System;

namespace Silphid.Showzup
{
    public class Nav
    {
        public static readonly Nav Empty = new Nav();

        public object Input;
        public Type ViewType;
        public IView View;
        public IOptions Options;
        
        public bool IsInvalid { get; private set; }

        public Nav() {}

        public Nav(object input, IOptions options = null)
        {
            Input = input;
            Options = options;
        }

        public Nav(object input, Type viewType, IOptions options = null)
        {
            Input = input;
            ViewType = viewType;
            Options = options;
        }

        public Nav(object input, IView view, IOptions options = null)
        {
            Input = input;
            View = view;
            ViewType = view?.GetType();
            Options = options;
        }

        public void Invalidate()
        {
            IsInvalid = true;
        }

        public void Validate()
        {
            IsInvalid = false;
        }

        public void DestroyView()
        {
            Invalidate();
            if (View == null)
                return;

            UnityEngine.Object.Destroy(View.GameObject);
            (View as IDisposable)?.Dispose();
            View = null;
        }

        public override string ToString()
        {
            return $"NavHistory - Input:{Input} - View:{View} - ViewType:{ViewType}";
        }
    }
}