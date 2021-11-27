using System;
using Silphid.Showzup.Recipes;
using UnityEngine;

namespace Silphid.Showzup.Virtual
{
    internal class Entry
    {
        private IView _view;
        private RectTransform _rectTransform;

        public object Model { get; }
        public Recipe? Recipe { get; set; }
        public IDisposable Disposable { get; set; }
        public bool IsViewLoaded => _view != null;
        public bool IsLayoutValid { get; set; }

        public IView View
        {
            get => _view;
            set
            {
                _view = value;
                _rectTransform = null;
            }
        }

        public RectTransform RectTransform =>
            _rectTransform
                ? _rectTransform
                : _rectTransform = _view.GameObject.GetComponent<RectTransform>();

        public Entry(object model)
        {
            Model = model;
        }
    }
}