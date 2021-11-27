using Silphid.Showzup;
using UnityEngine;

namespace App
{
    public class FormFactor : Variant<FormFactor>
    {
        public static readonly FormFactor Mobile = Add(nameof(Mobile));
        public static readonly FormFactor Tablet = Add(nameof(Tablet));
        public static readonly FormFactor Tv = Add(nameof(Tv));

        protected static FormFactor Add(string name) =>
            Add(new FormFactor(name));
        
        protected FormFactor(string name) : base(name)
        {
        }
    }
}