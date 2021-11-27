using UniRx;
using UnityEngine;

namespace Silphid.Showzup.Layout
{
    public class Box : IBox
    {
        public IReactiveProperty<float> XMin { get; }
        public IReactiveProperty<float> YMin { get; }
        public IReactiveProperty<float> XMax { get; }
        public IReactiveProperty<float> YMax { get; }
        public IReactiveProperty<float> Width { get; }
        public IReactiveProperty<float> Height { get; }

        public Box()
            : this(0, 0, 0, 0) {}

        public Box(Rect rect)
            : this(rect.xMin, rect.yMin, rect.width, rect.height) {}

        public Box(float xMin, float yMin, float width, float height)
        {
            XMin = new ReactiveProperty<float>(xMin);
            YMin = new ReactiveProperty<float>(yMin);
            XMax = new ReactiveProperty<float>(xMin + width);
            YMax = new ReactiveProperty<float>(yMin + height);
            Width = new ReactiveProperty<float>(width);
            Height = new ReactiveProperty<float>(height);
        }
    }
}