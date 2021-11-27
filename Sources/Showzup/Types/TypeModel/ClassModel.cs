namespace Silphid.Showzup
{
    public class ClassModel : TypeModel
    {
        public ClassModel(string name)
            : base(name) {}

        public string ParentName { get; set; }
        public ClassModel Parent { get; set; }
    }
}