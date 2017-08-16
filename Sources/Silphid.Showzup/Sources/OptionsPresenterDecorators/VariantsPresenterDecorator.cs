namespace Silphid.Showzup
{
    internal class VariantsPresenterDecorator : OptionsPresenterDecoratorBase
    {
        private readonly VariantSet _variants;

        public VariantsPresenterDecorator(IPresenter presenter, VariantSet variants) : base(presenter)
        {
            _variants = variants;
        }

        protected override void UpdateOptions(Options options)
        {
            options.Variants = _variants;
        }
    }
}