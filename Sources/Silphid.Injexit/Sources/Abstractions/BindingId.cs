namespace Silphid.Injexit
{
    public class BindingId
    {
        public string Name { get; }

        public BindingId(string name)
        {
            Name = name;
        }

        internal Binding Binding { get; set; }

        public override string ToString() => Name;
    }
}