using System;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class TransitionRef
    {
        [SerializeField] public ITransition Transition;
    }
}