using System.Linq;
using Silphid.Extensions;
using Silphid.Tweenzup;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Silphid.Showzup
{
    public class CarrouselControl : ListControl
    {
        private readonly SerialDisposable _motionDisposable = new SerialDisposable();
        private readonly ReactiveProperty<Item[]> _items = new ReactiveProperty<Item[]>();
        private float _totalWidth;
        private readonly ReactiveProperty<float> _currentPosition = new ReactiveProperty<float>();

        public float Spacing;
        public float MotionDuration = 1f;

        private struct Item
        {
            public RectTransform RectTransform;
            public float ReferencePosition;
            public float Width => RectTransform.rect.width;

            public Item(RectTransform rectTransform, float referencePosition) : this()
            {
                RectTransform = rectTransform;
                ReferencePosition = referencePosition;
            }
        }

        private void OnPresentCompleted()
        {
            _totalWidth = 0f;
            _items.Value = _views
                .Select(x =>
                {
                    var rectTransform = x.GameObject.RectTransform();
                    var item = new Item(rectTransform, _totalWidth);
                    _totalWidth += rectTransform.rect.width + Spacing;
                    return item;
                })
                .ToArray();
        }

        protected override void Start()
        {
            base.Start();

            Views.WhereNotNull()
                .SelectMany(Container.UpdateAsObservable().First())
                .Subscribe(_ => OnPresentCompleted())
                .AddTo(this);

            _items
                .WhereNotNull()
                .CombineLatest(ChosenIndex, (x, y) => y)
                .Subscribe(x =>
                {
                    if (x == null)
                        return;

                    FindShortestPath(_items.Value[x.Value].ReferencePosition);
                    
                    _motionDisposable.Disposable = _currentPosition
                        .TweenTo(_items.Value[x.Value].ReferencePosition, MotionDuration, Easer.OutQuadratic)
                        .SubscribeAndForget();
                })
                .AddTo(this);

            _items
                .WhereNotNull()
                .CombineLatest(_currentPosition, (x, y) => y)
                .Subscribe(UpdateAll)
                .AddTo(this);
        }

        private void FindShortestPath(float targetPosition)
        {
            var deltaToPosition = targetPosition.Delta(_currentPosition.Value);

            // Which side is the target?
            if (targetPosition < _currentPosition.Value)
            {
                // Left
                var deltaToRight = (targetPosition + _totalWidth).Delta(_currentPosition.Value);
                if (deltaToRight < deltaToPosition)
                    _currentPosition.Value -= _totalWidth;
            }
            else
            {
                // Right
                var deltaToLeft = (targetPosition - _totalWidth).Delta(_currentPosition.Value);
                if (deltaToLeft < deltaToPosition)
                    _currentPosition.Value += _totalWidth;
            }
        }

        private void UpdateAll(float currentPosition)
        {
            currentPosition = currentPosition.Wrap(_totalWidth);

            foreach (var item in _items.Value)
            {
                var pos = item.ReferencePosition - currentPosition;
                if (pos < 0 - item.Width)
                    pos += _totalWidth;

                item.RectTransform.anchoredPosition =
                    item.RectTransform.anchoredPosition.WithX(pos);
            }
        }
    }
}