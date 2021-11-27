namespace Silphid.Injexit
{
    public class BindingId
    {
        internal Binding Binding { get; set; }

        public BindingId() {}

        internal BindingId(Binding binding)
        {
            Binding = binding;
        }

        public override string ToString() =>
            Binding?.ConcretionType?.Name ?? "Unknown";
    }
}