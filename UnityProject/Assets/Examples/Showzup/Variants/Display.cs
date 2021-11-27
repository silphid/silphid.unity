using Silphid.Showzup;

namespace App
{
    public class Display : Variant<Display>
    {
        public static readonly Display Page = Add(nameof(Page));
        public static readonly Display Background = Add(nameof(Background));
        public static readonly Display Panel = Add(nameof(Panel));
        public static readonly Display Tile = Add(nameof(Tile));
        public static readonly Display Thumbnail = Add(nameof(Thumbnail));
        public static readonly Display Item = Add(nameof(Item));
        
        protected static Display Add(string name) =>
            Add(new Display(name));

        protected Display(string name) : base(name)
        {
        }
    }
}