namespace Silphid.Injexit
{
    public static class ResultExtensions
    {
        public static object ResolveInstance(this Result This, IResolver resolver)
        {
            if (This.Exception != null)
            {
                var ex = This.Exception as DependencyException;
                if (ex != null)
                    throw ex.With(resolver);
                
                throw This.Exception;
            }
            
            return This.Factory?.Invoke(resolver)
                ?? This.Instance;
        }
    }
}