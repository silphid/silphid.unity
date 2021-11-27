namespace Silphid.Showzup
{
    public class TypeModelToTypeModelMapping : Mapping
    {
        public TypeModel Target => _target;
        public override string TargetName => _target.Type.Name;

        public new TypeModel Source { get; }

        private readonly TypeModel _target;

        public TypeModelToTypeModelMapping(TypeModel source, TypeModel target, VariantSet variants)
            : base(source.Type, variants)
        {
            Source = source;
            _target = target;
        }
    }
}