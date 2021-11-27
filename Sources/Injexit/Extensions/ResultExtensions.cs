namespace Silphid.Injexit
{
    public static class ResultExtensions
    {
        public static object ResolveInstance(this Result This, IResolver resolver)
        {
            if (This.Scope != null)
                throw new UnhandledScopeException(This.Scope.Name);
            
            if (This.Exception != null)
            {
                if (This.Exception is DependencyException ex)
                    throw ex.With(resolver);

                throw This.Exception;
            }

            return This.Factory?.Invoke(resolver);
        }
    }
}