using System;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup.Layout
{
    public interface IBox
    {
        IReactiveProperty<float> XMin { get; }
        IReactiveProperty<float> YMin { get; }
        IReactiveProperty<float> XMax { get; }
        IReactiveProperty<float> YMax { get; }
        IReactiveProperty<float> Width { get; }
        IReactiveProperty<float> Height { get; }
    }
}