using Silphid.Extensions;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup.Layout
{
    public class BoxComponent : MonoBehaviour, IBox
    {
        public bool BoxTracksRect;
        public bool RectTracksBox;

        private RectTransform _rect;
        private readonly IBox _box = new Box();
        private Rect _lastRect;
        private Rect _lastBoxRect;

        private void Start()
        {
            _rect = GetComponent<RectTransform>();
            _rect.anchorMin = Vector2.up;
            _rect.anchorMax = Vector2.up;
            _rect.pivot = Vector2.up;
        }

        private void Update()
        {
            var newRect = new Rect(_rect.anchoredPosition, _rect.sizeDelta);
            bool isRectChanged = !newRect.IsAlmostEqualTo(_lastRect);
            var newBoxRect = _box.GetTopDownRect();
            bool isBoxChanged = !newBoxRect.Equals(_lastBoxRect);

            if (BoxTracksRect && isRectChanged)
                _box.SetTopDownRect(newRect);

            if (RectTracksBox && isBoxChanged && !isRectChanged)
            {
                _rect.anchoredPosition = newBoxRect.min;
                _rect.sizeDelta = newBoxRect.size;
            }

            _lastRect = newRect;
            _lastBoxRect = newBoxRect;
        }

        public IReactiveProperty<float> XMin => _box.XMin;
        public IReactiveProperty<float> YMin => _box.YMin;
        public IReactiveProperty<float> XMax => _box.XMax;
        public IReactiveProperty<float> YMax => _box.YMax;
        public IReactiveProperty<float> Width => _box.Width;
        public IReactiveProperty<float> Height => _box.Height;
    }
}