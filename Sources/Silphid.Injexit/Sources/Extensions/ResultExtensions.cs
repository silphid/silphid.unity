namespace Silphid.Injexit
{
    public static class ResultExtensions
    {
        public static object ResolveInstance(this Result This, IResolver resolver)
        {
            if (This.Exception != null)
                throw This.Exception;
            
            return This.Factory?.Invoke(resolver)
                ?? This.Instance;
        }
    }
}