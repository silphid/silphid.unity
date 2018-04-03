using System.Linq;

namespace Silphid.DataTypes
{
    public class Flag<T> : ObjectEnum<T> where T : Flag<T>
    {
        public static T[] FromBits<T>(ulong bits) =>
            All
                .Where(x => x.IsIncludedIn(bits))
                .Cast<T>()
                .ToArray();
        
        public ulong BitMask => 1ul << Id;
        public bool IsIncludedIn(ulong bits) => (BitMask & bits) != 0ul;
        
        protected Flag(int id, string name = null) : base(id, name)
        {
        }
    }
}