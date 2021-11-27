using System;

namespace Silphid.Showzup
{
    public class ResolveException : Exception
    {
        public object Model { get; }
        public VariantSet RequestedVariants { get; }

        public ResolveException(object model,
                                VariantSet requestedVariants,
                                string message,
                                Exception innerException = null)
            : base(message, innerException)
        {
            Model = model;
            RequestedVariants = requestedVariants;
        }

        public override string Message =>
            $"{base.Message}\n" + $"Model: {Model}\n" + $"RequestedVariants: {RequestedVariants}\n";
    }
}