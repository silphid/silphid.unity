namespace Silphid.Showzup
{
    public interface IViewResolver
    {
        ViewInfo Resolve(object input, Options options = null);
    }
}