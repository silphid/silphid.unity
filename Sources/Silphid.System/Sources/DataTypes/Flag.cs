using System.Linq;

namespace Silphid.DataTypes
{
    public class Flag<T> : TypeSafeEnum<T> where T : Flag<T>
    {
        public static T[] FromBits(ulong bits) =>
            All.Where(x => x.IsMatching(bits)).ToArray();
        
        public ulong BitMask => 1ul << Value;
        public bool IsMatching(ulong bits) => (BitMask & bits) != 0ul;
        
        protected Flag(int value, string name) : base(value, name)
        {
        }
    }
}