using System;

namespace Silphid.Showzup
{
    public class Segue
    {
        public Predicate<Presentation> Predicate { get; set; }
        public Func<Transition> TransitionFactory { get; set; }
    }
}